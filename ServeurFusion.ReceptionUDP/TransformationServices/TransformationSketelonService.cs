using ServeurFusion.ReceptionUDP.Datas;
using ServeurFusion.ReceptionUDP.TransformationServices;
using System;
using System.Collections.Concurrent;

namespace ServeurFusion.ReceptionUDP
{
    /// <summary>
    /// Class who transfert the skeleton between two BlockingCollection
    /// </summary>
    public class TransformationSkeletonService : TransformationService<Skeleton>
    {
        public TransformationSkeletonService(BlockingCollection<Skeleton> udpToMiddle, BlockingCollection<Skeleton> middleToWebRtc)
        {
            _middleThreadInfos = new MiddleThreadInfos<Skeleton>(udpToMiddle, middleToWebRtc);
        }

        /// <summary>
        /// Transport data between two BlockingCollection
        /// </summary>
        /// <param name="threadInfos">Object who contains the data to be tranfered </param>
        override protected void Launch(object threadInfos)
        {
            MiddleThreadInfos<Skeleton> ti = (MiddleThreadInfos<Skeleton>)threadInfos;
            Console.WriteLine("TransformationSkeleton thread started");
            
            while (true)
            {
                var data = ti._udpToMiddle.Take();
                ti._middleToWebRtc.Add(data);
            }
        }
    }
}
