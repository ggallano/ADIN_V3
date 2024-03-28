using ADIN.Device.Models.ADIN1300;
using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class LinkPropertyModel : ILinkProperties
    {
        public List<string> EnergyDetectPowerDownModes { get; set; }
        public List<string> MasterSlaves { get; set; }
        public List<string> MDIXs { get; set; }
        public List<string> SpeedModes { get; set; }
        public string EnergyDetectPowerDownMode { get; set; }
        public string MasterSlave { get; set; }
        public string MDIX { get; set; }
        public string SpeedMode { get; set; }
        public bool IsAdvertise_1000BASE_T_FD { get; set; }
        public bool IsAdvertise_1000BASE_T_HD { get; set; }
        public bool IsAdvertise_100BASE_TX_FD { get; set; }
        public bool IsAdvertise_100BASE_TX_HD { get; set; }
        public bool IsAdvertise_10BASE_T_FD { get; set; }
        public bool IsAdvertise_10BASE_T_HD { get; set; }
        public bool IsAdvertise_EEE_1000BASE_T { get; set; }
        public bool IsAdvertise_EEE_100BASE_TX { get; set; }
    }
}