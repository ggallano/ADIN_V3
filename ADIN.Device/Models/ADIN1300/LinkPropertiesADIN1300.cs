using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1300
{
    public class LinkPropertiesADIN1300 : ILinkProperties
    {
        //Advertised
        public List<string> SpeedMode { get; set; }
        public List<string> ANAdvertisedSpeeds { get; set; }
        public List<string> EEEAdvertiseSpeeds { get; set; }
        public List<string> MDIXs { get; set; }
        public List<string> EnergyDetectPowerDownModes { get; set; }
        public List<string> MasterSlave { get; set; }

        public LinkPropertiesADIN1300()
        {
            SpeedMode = new List<string>()
            {
                "Advertised",
                "Forced"
            };

            ANAdvertisedSpeeds = new List<string>()
            {
                "Advertise 1000BASE-T FD",
                "Advertise 1000BASE-T HD",
                "Advertise 1000BASE-TX FD",
                "Advertise 1000BASE-TX HD",
                "Advertise 10BASE-T FD",
                "Advertise 10BASE-T HD",
            };

            EEEAdvertiseSpeeds = new List<string>()
            {
                "Advertise EEE 1000BASE-T",
                "Advertise EEE 100BASE-TX",
            };

            MDIXs = new List<string>()
            {
                "Auto MDIX",
                "Fixed MDI",
                "Fixed MDIX"
            };

            EnergyDetectPowerDownModes = new List<string>()
            {
                "Disabled",
                "Enabled",
                "Enabled With Periodic Pulse TX"
            };

            MasterSlave = new List<string>()
            {
                "Master",
                "Slave"
            };
        }
    }
}
