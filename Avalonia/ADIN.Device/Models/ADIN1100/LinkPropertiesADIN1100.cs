using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models.ADIN1100
{
    public class LinkPropertiesADIN1100 : ILinkProperties
    {
        public LinkPropertiesADIN1100()
        {
            MasterSlaveAdvertises = new List<string>();
            MasterSlaveAdvertises.Add("Prefer_Master");
            MasterSlaveAdvertises.Add("Prefer_Slave");
            MasterSlaveAdvertises.Add("Forced_Master");
            MasterSlaveAdvertises.Add("Forced_Slave");
            MasterSlaveAdvertise = MasterSlaveAdvertises[0];

            TxAdvertises = new List<string>();
            TxAdvertises.Add("Capable2p4Volts_Requested2p4Volts");
            TxAdvertises.Add("Capable2p4Volts_Requested1Volt");
            TxAdvertises.Add("Capable1Volt");
            TxAdvertise = TxAdvertises[0];
        }
        #region ADIN1300_ADIN1200 Properties
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

        public List<string> MasterSlaves { get; set; }

        public string MDIX { get; set; }

        public List<string> MDIXs { get; set; }

        public string SpeedMode { get; set; }

        public List<string> SpeedModes { get; set; }
        #endregion

        public string MasterSlaveAdvertise { get; set; }
        public List<string> MasterSlaveAdvertises { get; set; }
        public string TxAdvertise { get; set; }
        public List<string> TxAdvertises { get; set; }

        public string ActivePhyMode { get; set; }
        public string MacInterface { get; set; }
    }
}
