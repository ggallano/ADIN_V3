namespace ADIN.Device.Models
{
    public class PeakModel
    {
        public string Type { get; set; }
        public int idx { get; set; }
        public double Amplitude { get; set; }
        public int StartEdge { get; set; }
        public int EndEdge { get; set; }
    }
}
