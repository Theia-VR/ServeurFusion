using ServeurFusion.ReceptionUDP.Datas.PointCloud;
using System;
using System.Collections.Concurrent;

namespace ServeurFusion.ReceptionUDP.TransformationServices
{
    /// <summary>
    /// Class who transfert the cloud between two BlockingCollection
    /// </summary>
    public class TransformationCloudService : TransformationService<Cloud>
    {

        public TransformationCloudService(BlockingCollection<Cloud> udpToMiddle, BlockingCollection<Cloud> middleToWebRtc)
        {
            _middleThreadInfos = new MiddleThreadInfos<Cloud>(udpToMiddle, middleToWebRtc);
        }

        /// <summary>
        /// Transport data between two BlockingCollection
        /// </summary>
        /// <param name="threadInfos">Object who contains the data to be tranfered </param>
        override protected void Launch(object threadInfos)
        {
            MiddleThreadInfos<Cloud> ti = (MiddleThreadInfos<Cloud>)threadInfos;
            Console.WriteLine("TransformationCloud thread started");

            while (true)
            {
                var data = ti._udpToMiddle.Take(); ;
                ti._middleToWebRtc.Add(data);
            }
        }
    }
}
