using System.Collections.Generic;
using ServeurFusion.ReceptionUDP.Datas.Cloud;

namespace ServeurFusion.ReceptionUDP.Datas.PointCloud
{
    /// <summary>
    /// Class who represenst a cloud point
    /// </summary>
    public class Cloud
    {
        public long Timestamp { get; set; }

        /// <summary>
        /// Point list of the cloud
        /// </summary>
        public List<CloudPoint> Points { get; set; }
    }
}
