using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ADIN.Device.Models.ADIN1200
{
    public class LinkPropertiesADIN1200 : ILinkProperties
    {
        public LinkPropertiesADIN1200()
        {
            IsSpeedCapable1G = false;

            SpeedModes = new List<string>()
            {
                "Advertised",
                "Forced"
            };
            SpeedMode = SpeedModes[0];


            MDIXs = new List<string>()
            {
                "Auto MDIX",
                "Fixed MDI",
                "Fixed MDIX"
            };
            MDIX = MDIXs[0];

            EnergyDetectPowerDownModes = new List<string>()
            {
                "Disabled",
                "Enabled",
                "Enabled With Periodic Pulse TX"
            };
            EnergyDetectPowerDownMode = EnergyDetectPowerDownModes[0];

            MasterSlaves = new List<string>()
            {
                "Master",
                "Slave"
            };
            MasterSlave = MasterSlaves[0];
        }

        public bool IsAdvertise_1000BASE_T_FD { get; set; }
        public bool IsAdvertise_1000BASE_T_HD { get; set; }
        public bool IsSpeedCapable1G { get; set; }
        public bool IsAdvertise_100BASE_TX_FD { get; set; }
        public bool IsAdvertise_100BASE_TX_HD { get; set; }
        public bool IsAdvertise_10BASE_T_FD { get; set; }
        public bool IsAdvertise_10BASE_T_HD { get; set; }

        public bool IsAdvertise_EEE_1000BASE_T { get; set; }
        public bool IsAdvertise_EEE_100BASE_TX { get; set; }

        public uint DownSpeedRetries { get; set; }
        public bool IsDownSpeed_10BASE_T_HD { get; set; }
        public bool IsDownSpeed_100BASE_TX_HD { get; set; }

        public string EnergyDetectPowerDownMode { get; set; }

        public List<string> EnergyDetectPowerDownModes { get; set; }

        public string MasterSlave { get; set; }

        public List<string> MasterSlaves { get; set; }

        public string MDIX { get; set; }

        public List<string> MDIXs { get; set; }

        public string SpeedMode { get; set; }

        public List<string> SpeedModes { get; set; }
    }
}
