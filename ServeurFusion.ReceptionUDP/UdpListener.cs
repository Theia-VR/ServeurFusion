using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServeurFusion.ReceptionUDP
{
    public class UdpThreadInfos
    {
        public DataTransferer _dataTransferer { get; set; }
        public int _port = 9877;

        public UdpThreadInfos(DataTransferer dataTransferer, int port)
        {
            _dataTransferer = dataTransferer;
            _port = port;
        }
    }
    public class UdpListener
    {
        private UdpThreadInfos _threadInfos;

        public UdpListener(DataTransferer dataTransferer, int port)
        {
            _threadInfos = new UdpThreadInfos(dataTransferer, port);
        }

        private void StartListening(object threadInfos)
        {
            UdpThreadInfos ti = (UdpThreadInfos)threadInfos;
            Console.WriteLine("Thread udp démarrée");

            UdpClient udp = new UdpClient(ti._port);

            while (true)
            {
                var remoteEP = new IPEndPoint(IPAddress.Any, ti._port);
                var data = udp.Receive(ref remoteEP);
                ti._dataTransferer.AddData(data);
                //Console.WriteLine("receive data from " + remoteEP.ToString() + " ; Lenght = " + data.Length + " ; content = " + data);
                //Console.WriteLine("dataSize : " + ti._dataTransferer.ReadData().Count);
            }
        }

        public void Listen()
        {
            Thread th = new Thread(new ParameterizedThreadStart(StartListening));
            
            th.Start(_threadInfos);
        }
    }
}
