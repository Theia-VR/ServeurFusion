using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServeurFusion.EnvoiRTC
{
    public class Skeleton
    {
        // Timestamp - 8 bytes
        public long Timestamp { get; set; }
        // Tag - 1 byte
        public byte Tag { get; set; }

        public List<SkeletonPoint> SkeletonPoints { get; set; }
    }
}
