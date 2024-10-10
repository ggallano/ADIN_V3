using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1300
{
    public class LinkPropertiesADIN1300 : ILinkProperties
    {
        public LinkPropertiesADIN1300()
        {
            IsSpeedCapable1G = true;

            SpeedModes = new List<string>()
            {
                "Advertised",
                "Forced"
            };

            ForcedSpeeds = new List<string>()
            {
                "SPEED_10BASE_T_HD",
                "SPEED_10BASE_T_FD",
                "SPEED_100BASE_TX_HD",
                "SPEED_100BASE_TX_FD"
                //"SPEED_1000BASE_T_FD"
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

            MasterSlaveAdvertises = new List<string>()
            {
                "Master",
                "Slave"
            };

            AdvertisedSpeeds = new List<string>() { };
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
