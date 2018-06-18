using System.Collections.Generic;


namespace ServeurFusion.ReceptionUDP.Datas
{
    /// <summary>
    /// Class who represent a skeleton
    /// </summary>
    public class Skeleton
    {
        // Timestamp - 8 bytes
        public long Timestamp { get; set; }
        // Tag - 1 byte
        public byte Tag { get; set; }

        /// <summary>
        /// Point list of the skeleton
        /// </summary>
        public List<SkeletonPoint> SkeletonPoints { get; set; }
    }
}