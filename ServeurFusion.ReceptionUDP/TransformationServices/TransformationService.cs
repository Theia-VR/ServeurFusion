using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ServeurFusion.ReceptionUDP.TransformationServices
{
    /// <summary>
    /// Middleware class for future data processing
    /// </summary>
    /// <typeparam name="T">Generic type of transfered data</typeparam>
    public abstract class TransformationService<T>
    {
        private Thread _transformationServiceThread;
        protected MiddleThreadInfos<T> _middleThreadInfos{ get; set; }

        /// <summary>
        /// Starting thread
        /// </summary>
        public void Start()
        {
            _transformationServiceThread = new Thread(new ParameterizedThreadStart(Launch));
            _transformationServiceThread.Start(_middleThreadInfos);
        }

        // Stopping thread
        public void Stop()
        {
            Console.WriteLine("TransformationSkeleton thread stopped");
            _transformationServiceThread.Abort();
        }

        protected abstract void Launch(object obj);
    }

    /// <summary>
    /// Class use to set thread params
    /// </summary>
    /// <typeparam name="T">Generic type used in the listener</typeparam>
    public class MiddleThreadInfos<T>
    {
        public BlockingCollection<T> _udpToMiddle { get; set; }
        public BlockingCollection<T> _middleToWebRtc { get; set; }

        public MiddleThreadInfos(BlockingCollection<T> udpToMiddle, BlockingCollection<T> middleToWebRtc)
        {
            _udpToMiddle = udpToMiddle;
            _middleToWebRtc = middleToWebRtc;
        }
    }
}
