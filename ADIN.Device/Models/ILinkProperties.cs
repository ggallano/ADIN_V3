using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public interface ILinkProperties
    {
        List<string> ANAdvertisedSpeeds { get; set; }
        List<string> EEEAdvertiseSpeeds { get; set; }
        List<string> EnergyDetectPowerDownModes { get; set; }
        List<string> MasterSlave { get; set; }
        List<string> MDIXs { get; set; }
        List<string> SpeedMode { get; set; }
    }
}