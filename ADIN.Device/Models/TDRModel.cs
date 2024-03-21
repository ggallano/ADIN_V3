namespace ADIN.Device.Models
{
    public class TDRModel
    {
        public decimal NVP { get; set; } = 0.67M;
        public decimal CableOffset { get; set; } = 80.00M;
        public decimal Coeff0 { get; set; } = 0.754M;
        public decimal Coeff1 { get; set; } = 1.003M;
        public FaultType Fault { get; set; } = FaultType.None;
        public CalibrationMode Mode { get; set; } = CalibrationMode.AutoRange;
    }
}
