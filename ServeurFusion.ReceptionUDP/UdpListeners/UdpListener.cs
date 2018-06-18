using System.Collections.Concurrent;
using System.Threading;

namespace ServeurFusion.ReceptionUDP.UdpListeners
{
    /// <summary>
    /// Generic class for UdpListener threads
    /// </summary>
    /// <typeparam name="T">Object created by the listener</typeparam>
    public abstract class UdpListener<T>
    {
        /// <summary>
        /// Listening thread
        /// </summary>
        private Thread _udpListenerThread;

        /// <summary>
        /// Informations used in the method started by the thread
        /// The thread object allows only one parameter : put all the infomations in this object
        /// </summary>
        protected UdpThreadInfos<T> _udpThreadInfos;

        /// <summary>
        ///  Start the listening thread
        /// </summary>
        public void Listen()
        {
            _udpListenerThread = new Thread(new ParameterizedThreadStart(StartListening));
            _udpListenerThread.Start(_udpThreadInfos);
        }

        /// <summary>
        /// Stop the listening thread
        /// </summary>
        public void Stop()
        {
            StopListening();
            _udpListenerThread.Abort();
        }

        /// <summary>
        /// Method to override in children of this class, start listening on the thread
        /// </summary>
        /// <param name="obj">UdpThreadInfo, thread params</param>
        protected abstract void StartListening(object obj);

        /// <summary>
        /// Method to override in children of this class, stop listening on the thread
        /// </summary>
        protected abstract void StopListening();
    }

    /// <summary>
    /// Class use to set thread params
    /// </summary>
    /// <typeparam name="T">Generic type used in the listener</typeparam>
    public class UdpThreadInfos<T>
    {
        /// <summary>
        /// BlockingCollection in which adding received data
        /// </summary>
        public BlockingCollection<T> DataTransferer { get; set; }

        /// <summary>
        /// Listening port
        /// </summary>
        public int Port { get; set; }

        public UdpThreadInfos(BlockingCollection<T> dataTransferer, int port)
        {
            DataTransferer = dataTransferer;
            Port = port;
        }
    }
}
