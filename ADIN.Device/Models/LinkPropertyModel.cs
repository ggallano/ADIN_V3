using ADIN.Device.Models.ADIN1300;
using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class LinkPropertyModel : ILinkProperties
    {
        public List<string> ANAdvertisedSpeeds { get; set; }
        public List<string> EEEAdvertiseSpeeds { get; set; }
        public List<string> EnergyDetectPowerDownModes { get; set; }
        public List<string> MasterSlave { get; set; }
        public List<string> MDIXs { get; set; }
        public List<string> SpeedMode { get; set; }
    }
}