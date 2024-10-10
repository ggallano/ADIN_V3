using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1100
{
    public class TimeDomainReflectometryADIN1100 : ITimeDomainReflectometry
    {
        public TimeDomainReflectometryADIN1100()
        {
            TimeDomainReflectometry = new TDRModel();
        }

        //public Brush CableBackgroundBrush { get; set; }
        public string CableFileName { get; set; }
        public string DistToFault { get; set; }
        //public Brush FaultBackgroundBrush { get; set; }
        public string FaultState { get; set; }
        public bool IsFaultVisibility { get; set; }
        public bool IsOngoingCalibration { get; set; }
        //public Brush OffsetBackgroundBrush { get; set; }
        public string OffsetFileName { get; set; }
        public TDRModel TimeDomainReflectometry { get; set; }
        public bool IsVisibleCableCalibration { get; set; }
        public bool IsVisibleOffsetCalibration { get; set; }
        public string OffsetCalibrationMessage { get; set; }
        public string CableCalibrationMessage { get; set; }
    }
}
