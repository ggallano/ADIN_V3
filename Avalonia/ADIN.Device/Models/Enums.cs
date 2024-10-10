using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public enum AutoNegMasterSlaveAdvertisementItem
    {
        Prefer_Slave,
        Prefer_Master,
        Forced_Slave,
        Forced_Master
    }

    public enum BoardRevision
    {
        Rev0,
        Rev1
    }

    public enum BoardType
    {
        ADIN1100_S1,
        ADIN1100,
        ADIN1110,
        ADIN1200,
        ADIN1300,
        ADIN2111,
    }

    public enum CalibrateType
    {
        Offset,
        Cable
    }

    public enum CalibrationMode
    {
        Optimized = 0,
        AutoRange
    }

    /// <summary>
    /// All possible Ethernet Speeds, values 0 - 7 much match HCD tech
    /// </summary>
    public enum EthernetSpeeds
    {
        /// <summary>
        /// 10BASE-T HD
        /// </summary>
        SPEED_10BASE_T_HD = 0x0,

        /// <summary>
        /// 10BASE-T FD
        /// </summary>
        SPEED_10BASE_T_1L = 0x01,// SPEED_10BASE_T_FD = 0x1,

        /// <summary>
        /// 100BASE-TX HD
        /// </summary>
        SPEED_100BASE_TX_HD = 0x2,

        /// <summary>
        /// 100BASE-TX FD
        /// </summary>
        SPEED_100BASE_TX_FD = 0x3,

        /// <summary>
        /// Not specified
        /// </summary>
        SPEED_1000BASE_T_HD = 0x4,

        /// <summary>
        /// 1000BASE-T
        /// </summary>
        SPEED_1000BASE_T_FD = 0x5,

        /// <summary>
        /// Not specified
        /// </summary>
        SPEED_UNDEFINED_6 = 0x6,

        /// <summary>
        /// Not specified
        /// </summary>
        SPEED_UNDEFINED_7 = 0x7,

        /// <summary>
        /// 10GBASE-KR EEE ability
        /// </summary>
        SPEED_10GBASE_KR_EEE,

        /// <summary>
        /// 10GBASE-KX4 EEE
        /// </summary>
        SPEED_10GBASE_KX4_EEE,

        /// <summary>
        /// 1000BASE-KX EEE
        /// </summary>
        SPEED_1000BASE_KX_EEE,

        /// <summary>
        /// 10GBASE-T EEE
        /// </summary>
        SPEED_10GBASE_T_EEE,

        /// <summary>
        /// 1000BASE-T EEE
        /// </summary>
        SPEED_1000BASE_T_EEE,

        /// <summary>
        /// 100BASE-TX EEE
        /// </summary>
        SPEED_100BASE_TX_EEE
    }

    /// <summary>
    /// EthernetPhyState
    /// </summary>
    public enum EthPhyState
    {
        /// <summary>
        /// Powerdown. No Linking Possible
        /// </summary>
        Powerdown,

        /// <summary>
        /// Powered Up. Linking NOT enabled.
        /// </summary>
        Standby,

        /// <summary>
        /// Linking Enabled
        /// </summary>
        LinkDown,

        /// <summary>
        /// Link Established
        /// </summary>
        LinkUp
    }

    public enum FaultType
    {
        None,
        Open,
        Short
    }

    /// <summary>
    /// Frame Type
    /// </summary>
    public enum FrameType
    {
        /// <summary>
        /// Random
        /// </summary>
        Random,

        /// <summary>
        /// All0s
        /// </summary>
        All0s,

        /// <summary>
        /// A111s
        /// </summary>
        All1s,

        /// <summary>
        /// Alt10
        /// </summary>
        Alt10s,

        /// <summary>
        /// Decrement
        /// </summary>
        Decrement
    }

    /// <summary>
    /// Loopback Mode
    /// </summary>
    public enum LoopBackMode
    {
        /// <summary>
        /// Digital / PCS
        /// </summary>
        Digital,

        /// <summary>
        /// LineDriver / PMA
        /// </summary>
        LineDriver,

        /// <summary>
        /// ExtCable / ExtMII/RMII
        /// </summary>
        ExtCable,

        /// <summary>
        /// MAC I/F Remote
        /// </summary>
        MacRemote,

        /// <summary>
        /// MAC I/F
        /// </summary>
        MAC,

        /// <summary>
        /// OFF
        /// </summary>
        OFF
    }

    public enum PeakVoltageAdvertisementItem
    {
        Capable2p4Volts_Requested2p4Volts,
        Capable2p4Volts_Requested1Volt,
        Capable1Volt,
    }

    public enum ResetType
    {
        SubSysPin,
        SubSys,
        Phy
    }

    public enum TestModeType
    {
        Normal,
        Test1,
        Test2,
        Test3,
        Transmit
    }

    public enum RegisterActionType
    {
        Export,
        Load,
        Save
    }
}
