using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface ITimeDomainReflectometry
    {
        //Brush CableBackgroundBrush { get; set; }
        string CableFileName { get; set; }
        string DistToFault { get; set; }
        //Brush FaultBackgroundBrush { get; set; }
        string FaultState { get; set; }
        bool IsFaultVisibility { get; set; }
        bool IsOngoingCalibration { get; set; }
        //Brush OffsetBackgroundBrush { get; set; }
        string OffsetFileName { get; set; }
        TDRModel TimeDomainReflectometry { get; set; }
        bool IsVisibleCableCalibration { get; set; }
        bool IsVisibleOffsetCalibration { get; set; }
        string OffsetCalibrationMessage { get; set; }
        string CableCalibrationMessage { get; set; }
    }
}
