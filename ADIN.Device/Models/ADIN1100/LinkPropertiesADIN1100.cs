using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1100
{
    public class LinkPropertiesADIN1100 : ILinkProperties
    {
        public List<string> AdvertisedSpeeds { get; set; }

        public uint DownSpeedRetries { get; set; }

        public string EnergyDetectPowerDownMode { get; set; }

        public List<string> EnergyDetectPowerDownModes { get; set; }

        public string ForcedSpeed { get; set; }

        public List<string> ForcedSpeeds { get; set; }

        public bool IsAdvertise_1000BASE_T_FD { get; set; }

        public bool IsAdvertise_1000BASE_T_HD { get; set; }

        public bool IsAdvertise_100BASE_TX_FD { get; set; }

        public bool IsAdvertise_100BASE_TX_HD { get; set; }

        public bool IsAdvertise_10BASE_T_FD { get; set; }

        public bool IsAdvertise_10BASE_T_HD { get; set; }

        public bool IsAdvertise_EEE_1000BASE_T { get; set; }

        public bool IsAdvertise_EEE_100BASE_TX { get; set; }

        public bool IsDownSpeed_100BASE_TX_HD { get; set; }

        public bool IsDownSpeed_10BASE_T_HD { get; set; }

        public bool IsSpeedCapable1G { get; set; }

        public string MasterSlave { get; set; }

        public string MasterSlaveAdvertise { get; set; }

        public string MDIX { get; set; }

        public List<string> MDIXs { get; set; }

        public string SpeedMode { get; set; }

        public List<string> SpeedModes { get; set; }


        public List<string> MasterSlaveAdvertises { get; set; }

        public List<string> MasterSlaves { get; set; }

        public string TxAdvertise { get; set; }

        public List<string> TxAdvertises { get; set; }
    }
}
