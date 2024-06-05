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

            IsAdvertise_100BASE_TX_FD = true;
            IsAdvertise_100BASE_TX_HD = true;
            IsAdvertise_10BASE_T_FD = true;
            IsAdvertise_10BASE_T_HD = true;

            IsAdvertise_EEE_1000BASE_T = false;
            IsAdvertise_EEE_100BASE_TX = false;

            ForcedSpeeds = new List<string>()
            {
                "SPEED_10BASE_T_HD",
                "SPEED_10BASE_T_FD",
                "SPEED_100BASE_TX_HD",
                "SPEED_100BASE_TX_FD"
            };
            ForcedSpeed = ForcedSpeeds[0];

            IsDownSpeed_10BASE_T_HD = false;
            DownSpeedRetries = 4;

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

            AdvertisedSpeeds = new List<string>()
            {
                "SPEED_100BASE_TX_FD_SPEED",
                "SPEED_100BASE_TX_HD_SPEED",
                "SPEED_10BASE_T_FD_SPEED",
                "SPEED_10BASE_T_HD_SPEED"
            };
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

        public string ForcedSpeed { get; set; }

        public List<string> ForcedSpeeds { get; set; }

        public List<string> AdvertisedSpeeds { get; set; }


        public List<string> MasterSlaveAdvertises { get; set; }
        public string MasterSlaveAdvertise { get; set; }
        public string TxAdvertise { get; set; }
        public List<string> TxAdvertises { get; set; }
    }
}
