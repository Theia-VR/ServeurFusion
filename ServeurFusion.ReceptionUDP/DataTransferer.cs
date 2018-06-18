using ServeurFusion.EnvoiRTC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServeurFusion.ReceptionUDP
{
    public class DataTransferer
    {

        private static Queue fileInfos = new Queue();
        private int size = 0;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddData(Skeleton data)
        {
            size++;
            fileInfos.Enqueue(data);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Skeleton ConsumeData()
        {
            size--;
            return (Skeleton)fileInfos.Dequeue();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Queue ReadData()
        {
            return fileInfos;
        }
        
        public bool IsEmpty()
        {
            return size == 0;
        }
    }
}
