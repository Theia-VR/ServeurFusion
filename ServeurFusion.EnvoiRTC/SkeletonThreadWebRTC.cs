using ServeurFusion.ReceptionUDP.Datas;
using Spitfire;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ServeurFusion.EnvoiRTC
{
    public class SkeletonThreadInfos
    {
        public BlockingCollection<Skeleton> SkeletonToWebRTC { get; set; }
        public Dictionary<string, SpitfireRtc> RTCPeerConnection { get; set; }

        public SkeletonThreadInfos(BlockingCollection<Skeleton> skeletonToWebRTC, Dictionary<string, SpitfireRtc> rtcPeerConnection)
        {
            SkeletonToWebRTC = skeletonToWebRTC;
            RTCPeerConnection = rtcPeerConnection;
        }
    }

    public class SkeletonThreadWebRTC
    {
        private Thread _skeletonThread;
        private SkeletonThreadInfos _skeletonThreadInfos;

        public SkeletonThreadWebRTC(BlockingCollection<Skeleton> skeletonToWebRTC, Dictionary<string, SpitfireRtc> rtcPeerConnection)
        {
            _skeletonThreadInfos = new SkeletonThreadInfos(skeletonToWebRTC, rtcPeerConnection);
            _skeletonThread = new Thread(new ParameterizedThreadStart(StartSkeletonThread));
        }

        private void StartSkeletonThread(object threadInfos)
        {
            SkeletonThreadInfos skeletonThreadInfos = (SkeletonThreadInfos)threadInfos;
            Console.WriteLine("Thread Skeleton sender started");

            while (true)
            {
                Skeleton skeleton = skeletonThreadInfos.SkeletonToWebRTC.Take();

                string formattedSkeletonMessage = "";
                skeleton.SkeletonPoints.ForEach(s => formattedSkeletonMessage += $"{s.X};{s.Y};{s.Z};{s.R};{s.G};{s.B};".Replace(',', '.'));
                formattedSkeletonMessage = formattedSkeletonMessage.Remove(formattedSkeletonMessage.Length - 1, 1);

                // Handle peer disconnected while sending data
                try
                {
                    foreach (KeyValuePair<string, SpitfireRtc> peer in skeletonThreadInfos.RTCPeerConnection)
                    {
                        peer.Value.DataChannelSendText("skeletonChannel", formattedSkeletonMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error, sending data to a disconnected peer : " + ex.Message);
                }
            }
        }

        public void Start()
        {
            _skeletonThread.Start(_skeletonThreadInfos);
        }

        public void Stop()
        {
            _skeletonThread.Abort();
            Console.WriteLine("Thread Skeleton sender stopped");
        }
    } 
}
