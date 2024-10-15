using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public interface ILinkProperties
    {
        #region ADIN1200 ADIN1300
        List<string> EnergyDetectPowerDownModes { get; set; }
        List<string> MDIXs { get; set; }
        List<string> SpeedModes { get; set; }
        List<string> ForcedSpeeds { get; set; }
        List<string> AdvertisedSpeeds { get; set; }

        string EnergyDetectPowerDownMode { get; set; }
        string MDIX { get; set; }
        string SpeedMode { get; set; }
        string ForcedSpeed { get; set; }

        uint DownSpeedRetries { get; set; }
        bool IsDownSpeed_10BASE_T_HD { get; set; }
        bool IsDownSpeed_100BASE_TX_HD { get; set; }
        bool IsAdvertise_1000BASE_T_FD { get; set; }
        bool IsAdvertise_1000BASE_T_HD { get; set; }
        bool IsSpeedCapable1G { get; set; }
        bool IsAdvertise_100BASE_TX_FD { get; set; }
        bool IsAdvertise_100BASE_TX_HD { get; set; }
        bool IsAdvertise_10BASE_T_FD { get; set; }
        bool IsAdvertise_10BASE_T_HD { get; set; }

        bool IsAdvertise_EEE_1000BASE_T { get; set; }
        bool IsAdvertise_EEE_100BASE_TX { get; set; }
        #endregion

        #region ADIN1100
        string TxAdvertise { get; set; }
        List<string> TxAdvertises { get; set; }
        #endregion

        string MasterSlaveAdvertise { get; set; }
        List<string> MasterSlaveAdvertises { get; set; }
    }
}
