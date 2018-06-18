using ServeurFusion.ReceptionUDP.Datas;
using ServeurFusion.ReceptionUDP.UdpListeners;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace ServeurFusion.ReceptionUDP
{
    /// <summary>
    /// UdpListener of the skeleton sent by the KinectStreamer
    /// </summary>
    public class UdpSkeletonListener : UdpListener<Skeleton>
    {
        /// <summary>
        /// UdpClient
        /// </summary>
        private UdpClient _udp;

        public UdpSkeletonListener(BlockingCollection<Skeleton> dataTransferer, int port)
        {
            _udpThreadInfos = new UdpThreadInfos<Skeleton>(dataTransferer, port);
        }

        /// <summary>
        /// Start listening on specified port and adding data to the BlockingCollection
        /// </summary>
        /// <param name="threadInfos">Thread informations - connection params</param>
        override protected void StartListening(object threadInfos)
        {
            UdpThreadInfos<Skeleton> ti = (UdpThreadInfos<Skeleton>)threadInfos;
            Console.WriteLine("UdpSkeletonListener thread started");

            _udp = new UdpClient(ti.Port);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, ti.Port);

            while (true)
            {
                // Receiving frames from KinectStreamer
                byte[] data = null;
                try
                {
                    data = _udp.Receive(ref remoteEP);
                } catch (Exception ex)
                {

                }
                int count = 0;
                // Processing Skeleton
                Skeleton skeleton = new Skeleton()
                {
                    Timestamp = BitConverter.ToInt64(data, 0),
                    Tag = data[8],
                    SkeletonPoints = new List<SkeletonPoint>()
                };
                count = 9;
                while (count < 409)
                {
                    // Processing SkeletonPoints
                    SkeletonPoint skeletonPoint = new SkeletonPoint();
                    skeletonPoint.X = BitConverter.ToSingle(data, count);
                    count += 4;
                    skeletonPoint.Y = BitConverter.ToSingle(data, count);
                    count += 4;
                    skeletonPoint.Z = BitConverter.ToSingle(data, count);
                    count += 4;
                    skeletonPoint.R = data[count];
                    count += 1;
                    skeletonPoint.G = data[count];
                    count += 1;
                    skeletonPoint.B = data[count];
                    count += 1;
                    skeletonPoint.Tag = data[count];
                    count += 1;
                    skeleton.SkeletonPoints.Add(skeletonPoint);
                }
                
                ti.DataTransferer.Add(skeleton);
            }
        }

        /// <summary>
        /// Stop listening
        /// </summary>
        override protected void StopListening()
        {
            Console.WriteLine("Stop listening on UdpSkeletonListener thread");
            _udp.Close();
        }
    }
}
