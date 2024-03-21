using System.Windows.Media;

namespace ADIN.Device.Models
{
    public class FaultDetectorModel
    {
        public FaultDetectorModel()
        {
            CableDiagnostics = new TDRModel();
        }

        public Brush CableBackgroundBrush { get; set; }
        //public string Checker { get; set; }
        public TDRModel CableDiagnostics { get; set; }
        public string CableFileName { get; set; }
        public string DistToFault { get; set; }
        public Brush FaultBackgroundBrush { get; set; }
        public string FaultState { get; set; }
        public bool IsFaultVisibility { get; set; } = false;
        public bool IsOngoingCalibration { get; set; }
        public Brush OffsetBackgroundBrush { get; set; }
        public string OffsetFileName { get; set; }
    }
}