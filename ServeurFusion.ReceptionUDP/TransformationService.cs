using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServeurFusion.ReceptionUDP
{
    public class MiddleThreadInfos
    {
        public DataTransferer _udpToMiddle { get; set; }
        public DataTransferer _middleToWebRtc { get; set; }

        public MiddleThreadInfos(DataTransferer udpToMiddle, DataTransferer middleToWebRtc)
        {
            _udpToMiddle = udpToMiddle;
            _middleToWebRtc = middleToWebRtc;
        }
    }

    public class TransformationService
    {
        private MiddleThreadInfos _middleThreadInfos { get; set; }

        public TransformationService(DataTransferer udpToMiddle, DataTransferer middleToWebRtc)
        {
            _middleThreadInfos = new MiddleThreadInfos(udpToMiddle, middleToWebRtc);
        }

        private void StartProsecute(object threadInfos)
        {
            MiddleThreadInfos ti = (MiddleThreadInfos)threadInfos;
            Console.WriteLine("Thread middle démarrée");
            
            while (true)
            {
                if (ti._udpToMiddle.IsEmpty())
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    ti._middleToWebRtc.AddData(ti._udpToMiddle.ConsumeData());
                    Console.WriteLine("Transfert");

                }

            }
        }

        public void Prosecute()
        {
            Thread th = new Thread(new ParameterizedThreadStart(StartProsecute));

            th.Start(_middleThreadInfos);
        }
    }
}
