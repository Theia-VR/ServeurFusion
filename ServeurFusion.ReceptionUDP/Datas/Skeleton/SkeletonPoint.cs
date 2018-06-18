namespace ServeurFusion.ReceptionUDP.Datas
{
    /// <summary>
    /// Class who represents a point of a skeleton
    /// </summary>
    public class SkeletonPoint
    {
        // X point - 4 bytes
        public float X { get; set; }
        // Y point - 4 bytes
        public float Y { get; set; }
        // Z point - 4 bytes
        public float Z { get; set; }

        // R color - 1 byte
        public byte R { get; set; }
        // G color - 1 byte
        public byte G { get; set; }
        // B color - 1 byte
        public byte B { get; set; }

        // Tag vector - 1 byte
        public byte Tag { get; set; }
    }
}
