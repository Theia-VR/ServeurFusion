using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServeurFusion.ReceptionUDP.Datas;
using Spitfire;
using System;
using WebSocketSharp;
using ServeurFusion.ReceptionUDP.Datas.PointCloud;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ServeurFusion.EnvoiRTC
{
    /// <summary>
    /// WebRTC protocol implementation for communication with a client
    /// </summary>
    public class WebRtcCommunication
    {
        private WebSocket _signallingServer;

        private SkeletonThreadWebRTC _skeletonThread;

        private CloudThreadWebRTC _cloudThread;

        private BlockingCollection<Skeleton> _skeletonToWebRtc { get; set; }

        private BlockingCollection<Cloud> _cloudToWebRtc { get; set; }

        private string _connectedUser;

        private Dictionary<string, SpitfireRtc> _peers;

        public WebRtcCommunication(BlockingCollection<Skeleton> skeletonToWebRtc, BlockingCollection<Cloud> cloudToWebRtc, string signalingServerAdress)
        {
            _peers = new Dictionary<string, SpitfireRtc>();

            _skeletonToWebRtc = skeletonToWebRtc;
            _cloudToWebRtc = cloudToWebRtc;

            _skeletonThread = new SkeletonThreadWebRTC(_skeletonToWebRtc, _peers);
            _cloudThread = new CloudThreadWebRTC(_cloudToWebRtc, _peers);

            _skeletonThread.Start();
            _cloudThread.Start();

            // Setup signaling server
            _signallingServer = new WebSocket(signalingServerAdress);
            _signallingServer.OnMessage += WebSocketOnMessage;

            _signallingServer.OnOpen += (sender, e) =>
            {
                Console.WriteLine("WebSocket Opened");
            };
            _signallingServer.OnError += (sender, e) =>
            {
                Console.WriteLine("WebSocket error : " + e.Message);
            };
            _signallingServer.OnClose += (sender, e) =>
            {
                Console.WriteLine("WebSocket Closed");
            };

            _signallingServer.Connect();

            Connect();
        }

        /// <summary>
        /// Setup new generic RTCPeerConnection
        /// </summary>
        /// <returns></returns>
        public SpitfireRtc HandleNewPeer()
        {
            SpitfireRtc rtcPeerConnection = new SpitfireRtc();

            rtcPeerConnection.AddServerConfig(new ServerConfig()
            {
                Host = "stun.1.google.com",
                Port = 19302,
                Type = ServerType.Stun
            });

            SpitfireRtc.InitializeSSL();
            rtcPeerConnection.InitializePeerConnection();

            rtcPeerConnection.CreateDataChannel(new DataChannelOptions()
            {
                Id = 1,
                Label = "skeletonChannel",
                Reliable = false
            });

            rtcPeerConnection.CreateDataChannel(new DataChannelOptions()
            {
                Id = 3,
                Label = "cloudChannel",
                Reliable = false
            });

            SetupCallbacks(rtcPeerConnection);

            return rtcPeerConnection;
        }

        /// <summary>
        /// Connecting to signaling server
        /// </summary>
        public void Connect()
        {
            _signallingServer.Send("{\"type\":\"login\", \"name\":\"webrtcserver\"}");
        }

        /// <summary>
        /// WebSocket received messages handling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Data sent</param>
        private void WebSocketOnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine("WebSocket received message : " + e.Data);

            string message = e.Data;

            // Ignore Hello World message
            if (message.Contains("Hello world"))
                return;

            // Parsing received message
            var messageJson = JObject.Parse(message);

            // Switch received message type
            string messageType = messageJson.GetValue("type").ToString();
            Console.WriteLine("WebSocket message type : " + messageType);
            switch (messageType)
            {
                // Login message
                case "login":
                    bool success = messageJson.GetValue("success").ToObject<bool>();
                    if (success)
                        Console.WriteLine("WebSocket : Login success");
                    else
                        Console.WriteLine("WebSocket : Login error");
                    break;

                // New user message
                case "new user":
                    _connectedUser = messageJson.GetValue("name").ToString();
                    Console.WriteLine("New peer : " + _connectedUser);
                    _peers.Add(_connectedUser, HandleNewPeer());
                    _peers.Values.Last().CreateOffer();
                    break;

                // Message user leaving
                case "leave":
                    string name = messageJson.GetValue("name").ToString();
                    _peers.Remove(name);
                    Console.WriteLine("Peer disconnected : " + name);
                    break;

                // Offer message
                case "offer":
                    _connectedUser = messageJson.GetValue("name").ToString();
                    var offerJson = JObject.Parse(messageJson.GetValue("offer").ToString());
                    string sdpOffer = offerJson.GetValue("sdp").ToString();
                    _peers.Values.Last().SetOfferRequest(sdpOffer);
                    break;

                // Answer message
                case "answer":
                    var answerJson = JObject.Parse(messageJson.GetValue("answer").ToString());
                    string typeAnswer = answerJson.GetValue("type").ToString();
                    string sdpAnswer = answerJson.GetValue("sdp").ToString();
                    _peers.Values.Last().SetOfferReply(typeAnswer, sdpAnswer);
                    break;

                // Candidate message
                case "candidate":
                    var candidateStr = messageJson.GetValue("candidate").ToString();
                    if (!String.IsNullOrWhiteSpace(candidateStr))
                    {
                        Console.WriteLine($"AddCandidate : {candidateStr}");
                        var candidateJson = JObject.Parse(messageJson.GetValue("candidate").ToString());
                        string sdp = candidateJson.GetValue("candidate").ToString();
                        string sdpMid = candidateJson.GetValue("sdpMid").ToString();
                        int sdpMLineIndex = candidateJson.GetValue("sdpMLineIndex").ToObject<int>();
                        _peers.Values.Last().AddIceCandidate(sdpMid, sdpMLineIndex, sdp);
                    }
                    break;
            }
        }

        /// <summary>
        /// Setup WebRTC events handlers
        /// </summary>
        private void SetupCallbacks(SpitfireRtc rtcPeerConnection)
        {
            rtcPeerConnection.OnIceCandidate += OnIceCandidate;
            rtcPeerConnection.OnDataChannelOpen += DataChannelOpen;
            rtcPeerConnection.OnDataChannelClose += OnDataChannelClose;
            rtcPeerConnection.OnDataMessage += HandleMessage;
            rtcPeerConnection.OnIceStateChange += IceStateChange;
            rtcPeerConnection.OnSuccessAnswer += OnSuccessAnswer;
            rtcPeerConnection.OnFailure += OnFail;
            rtcPeerConnection.OnError += OnError;
            rtcPeerConnection.OnSuccessOffer += OnSuccessOffer;
        }

        /// <summary>
        /// Console message when success Offer
        /// </summary>
        /// <param name="sdp">SDP of the offer</param>
        private void OnSuccessOffer(SpitfireSdp sdp)
        {
            Console.WriteLine("SuccessOffer : " + sdp.Sdp + " ; Type : " + sdp.Type);
            _peers.Values.Last().SetOfferRequest(sdp.Sdp);
            var offerJson = JsonConvert.SerializeObject(new { type = "offer", offer = new { type = "offer", sdp = sdp.Sdp }, name = _peers.Keys.Last() });
            Console.WriteLine("Offer sent : " + offerJson);
            _signallingServer.Send(offerJson);
        }

        /// <summary>
        /// Console message when WebRTC fail
        /// </summary>
        /// <param name="err"></param>
        private void OnFail(string err)
        {
            Console.WriteLine("WebRTC Fail : " + err);
        }

        /// <summary>
        /// Console message when WebRTC error
        /// </summary>
        private void OnError()
        {
            Console.WriteLine("WebRTC error");
        }

        /// <summary>
        /// Handling new IceCandidate
        /// </summary>
        /// <param name="iceCandidate">IceCandidate</param>
        private void OnIceCandidate(SpitfireIceCandidate iceCandidate)
        {
            Console.WriteLine("New IceCandidate : {0} {1} {2}", iceCandidate.Sdp, iceCandidate.SdpMid, iceCandidate.SdpIndex);
            string answerJson;
            if (String.IsNullOrWhiteSpace(_connectedUser))
                answerJson = JsonConvert.SerializeObject(new { type = "candidate", candidate = new { candidate = iceCandidate.Sdp, sdpMid = iceCandidate.SdpMid,
                                                               sdpMLineIndex = iceCandidate.SdpIndex } });
            else
                answerJson = JsonConvert.SerializeObject(new { type = "candidate", candidate = new { candidate = iceCandidate.Sdp, sdpMid = iceCandidate.SdpMid,
                                                               sdpMLineIndex = iceCandidate.SdpIndex }, name = _peers.Keys.Last() });
            _signallingServer.Send(answerJson);
        }

        /// <summary>
        /// Get the SDP of the created Answer
        /// </summary>
        /// <param name="sdp">SDP of the Answer</param>
        private void OnSuccessAnswer(SpitfireSdp sdp)
        {
            Console.WriteLine("SuccessAnswer : " + sdp.Sdp);
            _peers.Values.Last().SetOfferReply("answer", sdp.Sdp);
            string answerJson;
            if (String.IsNullOrWhiteSpace(_connectedUser))
                answerJson = JsonConvert.SerializeObject(new { type = "answer", answer = new { type = "answer", sdp = sdp.Sdp } });
            else
                answerJson = JsonConvert.SerializeObject(new { type = "answer", answer = new { type = "answer", sdp = sdp.Sdp }, name = _peers.Keys.Last() });
            Console.WriteLine("Answer sent : " + answerJson);
            _signallingServer.Send(answerJson);
        }

        /// <summary>
        /// Console message when IceState changed
        /// </summary>
        /// <param name="state">IceSate</param>
        private void IceStateChange(IceConnectionState state)
        {
            Console.WriteLine("IceState changed : " + state.ToString());
        }

        /// <summary>
        /// Console message when a message is received on DataChannel
        /// </summary>
        /// <param name="label">DataChannel name</param>
        /// <param name="msg">Received message</param>
        private void HandleMessage(string label, DataMessage msg)
        {
            if (msg.IsBinary)
                Console.WriteLine(msg.RawData.Length);
            else
                Console.WriteLine(msg.Data);
        }

        /// <summary>
        /// Console message when DataChannel is closed
        /// </summary>
        /// <param name="label">DataChannel name</param>
        private void OnDataChannelClose(string label)
        {
            Console.WriteLine("DataChannel closed : " + label);
        }

        private void DataChannelOpen(string label)
        {
            Console.WriteLine("DataChannel opened : " + label);
        }

        /// <summary>
        /// Closing connection
        /// </summary>
        public void Close()
        {
            if (_signallingServer != null)
                if (_signallingServer.IsAlive)
                    _signallingServer.Close();

            _skeletonThread.Stop();
            _cloudThread.Stop();

            _peers.Clear();
        }
    }
}
