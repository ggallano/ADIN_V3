//-----------------------------------------------------------------------
// <copyright file="FirmwareAPI.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

#define MASTER_SLAVE_NEGOTIATE

namespace TargetInterface
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using DeviceCommunication;
    using Utilities.Feedback;
    using Utilities.JSONParser;
    using Utilities.JSONParser.JSONClasses;
    using System.Xml;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Globalization;

    /// <summary>
    /// Handles the communication between PC and the device firmware API
    /// </summary>
    public class FirmwareAPI : FeedbackPropertyChange
    {
        private bool one1GCapabable = false;
        private bool cablediagnosticsRunning = false;
        private TargetSettings deviceSettingsUp = new TargetSettings();

        private Dictionary<uint, uint> regContents = new Dictionary<uint, uint>();

        private uint checkedFrames = 0;

        private uint checkedFramesErrors = 0;

        private uint mseA_Max = 0x0;

        private uint mseB_Max = 0x0;

        private uint mseC_Max = 0x0;

        private uint mseD_Max = 0x0;

        private bool localLpbk = false;
        /// <summary>
        /// A table of bit name which match to local advertised link speeds
        /// </summary>
        private List<RegisterLookUp<EthernetSpeeds>> localAdvSpeedBitLookUp = new List<RegisterLookUp<EthernetSpeeds>>();

        /// <summary>
        /// A table of bit name which match to remote advertised link speeds
        /// </summary>
        private List<RegisterLookUp<EthernetSpeeds>> remoteAdvSpeedBitLookUp = new List<RegisterLookUp<EthernetSpeeds>>();

        /// <summary>
        /// Stores the device connection to the part
        /// </summary>
        private DeviceConnection deviceConnection;

        /// <summary>
        /// Stores the instance of JSON parser engine
        /// </summary>
        private JSONParserEngine jsonParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareAPI"/> class
        /// </summary>
        public FirmwareAPI()
        {
            this.jsonParser = new JSONParserEngine();
            this.deviceConnection = null;

            this.UpdatefromRegisterJSON(DeviceType.ADIN1300); // Assume ADIN1300 until we connect and find out differently
        }

        public ObservableCollection<RegisterDetails> Registers { get; set; }

        public ObservableCollection<string> Scripts { get; set; }

        /// <summary>
        /// Identifies which device type we are connected to
        /// </summary>
        public enum DeviceType
        {
            /// <summary>
            /// ADIN1300
            /// </summary>
            ADIN1300,

            /// <summary>
            /// ADIN1301
            /// </summary>
            ADIN1301,

            /// <summary>
            /// ADIN1200
            /// </summary>
            ADIN1200,

            /// <summary>
            /// ADIN1100 (10SPE)
            /// </summary>
            ADIN1100,
        }

        /// <summary>
        /// Clock selection for outputting on GP_CLK pin
        /// </summary>
        public enum GPClockSel
        {
            /// <summary>
            /// Digital 25MHz Clock (irrespective of what the reference clock input is)
            /// </summary>
            Digital25MHz,

            /// <summary>
            /// 125 Mhz recovered clock output
            /// </summary>
            RecoveredReceiver125MHz,

            /// <summary>
            /// GeClkFree125
            /// </summary>
            GeClkFree125,

            /// <summary>
            /// GeClkHrtRcvr
            /// </summary>
            GeClkHrtRcvr,

            /// <summary>
            /// GeClkHrtFree
            /// </summary>
            GeClkHrtFree,

            /// <summary>
            /// OFF
            /// </summary>
            OFF
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

        /// <summary>
        /// FrameChecker
        /// </summary>
        public enum FrameChecker
        {
            /// <summary>
            /// Rx Side
            /// </summary>
            RxSide,

            /// <summary>
            /// Tx Side
            /// </summary>
            TxSide
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

        /// <summary>
        /// These are the subset of speeds which can be forced
        /// </summary>
        public enum EthernetSpeedsForced
        {
            /// <summary>
            /// 10BASE-T HD
            /// </summary>
            SPEED_10BASE_T_HD = 0x0,

            /// <summary>
            /// 10BASE-T FD
            /// </summary>
            SPEED_10BASE_T_1L = 0x1,

            /// <summary>
            /// 100BASE-TX HD
            /// </summary>
            SPEED_100BASE_TX_HD = 0x2,

            /// <summary>
            /// 100BASE-TX FD
            /// </summary>
            SPEED_100BASE_TX_FD = 0x3
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
        /// Available Forced Speeds
        /// </summary>
        public enum AutoMdixMode
        {
            /// <summary>
            /// Auto
            /// </summary>
            Auto,

            /// <summary>
            /// FixedMdi
            /// </summary>
            FixedMdi,

            /// <summary>
            /// FixedMdix
            /// </summary>
            FixedMdix
        }

        /// <summary>
        /// Master / Slave Setting
        /// </summary>
        public enum MasterSlaveFixed
        {
            /// <summary>
            /// Configure PHY as MASTER during MASTER-SLAVE negotiation
            /// </summary>
            Master,

            /// <summary>
            /// Configure PHY as SLAVE during MASTER-SLAVE negotiation
            /// </summary>
            Slave
        }

        /// <summary>
        /// Master / Slave Setting
        /// </summary>
        public enum MasterSlavePreference
        {
            /// <summary>
            /// Prefer master (or port type) advertisement
            /// </summary>
            Master,

            /// <summary>
            /// Prefer Slave (or port type) advertisement
            /// </summary>
            Slave
        }

        /// <summary>
        /// SignalPeakToPeakVoltage Setting
        /// </summary>
        public enum SignalPeakToPeakVoltage
        {
            /// <summary>
            /// 2.4 Volts Pk-Pk Capable, Requested 2.4V
            /// </summary>
            //CapableTwoPointFourVolts_RequestedTwoPointFourVolts,
            Capable2p4Volts_Requested2p4Volts,

            /// <summary>
            /// 2.4 Volts Pk-Pk Capable, Requested 1.0V
            /// </summary>
            //CapableTwoPointFourVolts_RequestedOneVolt,
            Capable2p4Volts_Requested1Volt,


            /// <summary>
            /// One Volt Pk-Pk
            /// </summary>
            //CapableOneVolt
            Capable1Volt
        }
        /// <summary>
        /// AN Status Setting
        /// </summary>
        public enum ANStatus
        {
            /// <summary>
            /// Configuration Fault
            /// </summary>
            Config_Fault,

            /// <summary>
            /// AN Done
            /// </summary>
            AN_Done,

            /// <summary>
            /// An Link Good
            /// </summary>
            AN_Link_Good
        }

        /// <summary>
        /// Master / Slave / Negotiate Setting
        /// Brian Murray Recommendation is only to use the PHY in Auto-Neg enabled mode. Quite complicated to bring up a link with ANeg disabled. 
        /// </summary>
        public enum MasterSlaveNegotiate
        {
#if MASTER_SLAVE_NEGOTIATE
            /// <summary>
            /// Fixed Master
            /// </summary>
            Prefer_Master,

            /// <summary>
            /// Fixed Slave
            /// </summary>
            Prefer_Slave,
#endif

            /// <summary>
            /// Negotiate Master or Slave
            /// </summary>
            //Negotiate

            /// <summary>
            ///
            /// </summary>
            Forced_Master,

            /// <summary>
            /// Forced Slave
            /// </summary>
            Forced_Slave
        }

        /// <summary>
        /// Speed Selection Protocol
        /// </summary>
        public enum EthSpeedMode
        {
            /// <summary>
            /// Advertise a number of different speeds
            /// </summary>
            Advertised,

            /// <summary>
            /// Force a particular speed
            /// </summary>
            Forced
        }

        /// <summary>
        /// Energy Powerdown Mode Options
        /// </summary>
        public enum EnergyPowerDownMode
        {
            /// <summary>
            /// Energy Powerdown Mode : OFF
            /// </summary>
            Disabled,

            /// <summary>
            /// Energy Powerdown Mode : ON
            /// </summary>
            Enabled,

            /// <summary>
            /// Energy Powerdown Mode : ON with occasional pulse TX
            /// </summary>
            EnabledWithPeriodicPulseTX
        }

        /// <summary>
        /// Gets or sets device settings
        /// </summary>
        public TargetSettings DeviceSettings
        {
            get
            {
                return this.deviceSettingsUp;
            }

            set
            {
                string listproperty;

                listproperty = "Advertise10HD";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.Speed10HdAdvertisement(value.Negotiate.Advertise10HD);
                }

                listproperty = "Advertise10FD";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.Speed10FdAdvertisement(value.Negotiate.Advertise10FD);
                }

                listproperty = "Advertise100HD";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.Speed100HdAdvertisement(value.Negotiate.Advertise100HD);
                }

                listproperty = "Advertise100FD";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.Speed100FdAdvertisement(value.Negotiate.Advertise100FD);
                }

                listproperty = "Advertise1000HD";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.Speed1000HdAdvertisement(value.Negotiate.Advertise1000HD);
                }

                listproperty = "Advertise1000FD";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.Speed1000FdAdvertisement(value.Negotiate.Advertise1000FD);
                }

                listproperty = "AdvertiseEEE100";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.EEE100Advertisement(value.Negotiate.AdvertiseEEE100);
                }

                listproperty = "AdvertiseEEE1000";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.EEE1000Advertisement(value.Negotiate.AdvertiseEEE1000);
                }

                listproperty = "EPDMode";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.EdPdConfig(value.EPDMode);
                }

                listproperty = "MdixMode";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.AutoMdixConfig(value.MdixMode);
                }

                listproperty = "EthSpeedSelection";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    if (value.EthSpeedSelection == EthSpeedMode.Forced)
                    {
                        this.ANegEnable(false);
                    }
                    else
                    {
                        this.ANegEnable(true);
                    }
                }

                listproperty = "FixedMasterSlave";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.ManualMasterSlaveConfig(value.Fixed.FixedMasterSlave);
                }

                listproperty = "ForcedSpeed";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.ForcedSpeedSelect(value.Fixed.ForcedSpeed);
                }

                listproperty = "InSoftwarePowerDown";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                }

                listproperty = "LinkingEnabled";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                }

                listproperty = "AutoNegLocalPhyEnabled";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);

                    // this.Negotiate.AutoNegLocalPhyEnabled = value.Negotiate.AutoNegLocalPhyEnabled;
                }

                listproperty = "PreferMasterSlave";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.PreferMasterSlaveAdvertisement(value.Negotiate.PreferMasterSlave);
                }

                listproperty = "PkPkVoltage";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.PeakToPeakVoltageSetting(value.Negotiate.PkPkVoltage);
                }

                listproperty = "NegotiateMasterSlave";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.NegotiateMasterSlaveSetting(value.Negotiate.NegotiateMasterSlave);
                }

                /* Downspeed settings */
                listproperty = "DownSpeed10Enabled";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.DnSpeed10Enable(value.Negotiate.DownSpeed10Enabled);
                }

                listproperty = "DownSpeed100Enabled";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.DnSpeed10Enable(value.Negotiate.DownSpeed100Enabled);
                }

                listproperty = "DownSpeedRetries";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                    this.DnSpeedRetries(value.Negotiate.DownSpeedRetries);
                }

                /* Dump these if they exist, they are not interesting */
                //                listproperty = "Negotiate";
                //               if (value.PropertiesChangedList.Contains(listproperty))
                //{
                //                    value.PropertiesChangedList.Remove(listproperty);
                //               }

                listproperty = "Fixed";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                }

                listproperty = "Link";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                }

                listproperty = "PropertiesChangedList";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                }

                listproperty = "DownspeedTo100Possible";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                }

                listproperty = "DownspeedTo10Possible";
                if (value.PropertiesChangedList.Contains(listproperty))
                {
                    value.PropertiesChangedList.Remove(listproperty);
                }

                /* Anything not handled? */
                foreach (var property in value.PropertiesChangedList)
                {
                    Trace.WriteLine(property);
                }

                value.ClearPropertiesChangedList();
            }
        }

        private bool TenSPEDevice()
        {
            return this.deviceConnection.DeviceDescription == DeviceConnection.DeviceDescriptionEVALADIN11xx;
        }

        private void UpdatefromRegisterJSON(DeviceType deviceType)
        {
            string requiredjsonfile = "registers_adin1300.json";
            switch (deviceType)
            {
                case DeviceType.ADIN1100:
                    requiredjsonfile = "registers_adin1100.json";
                    break;
                case DeviceType.ADIN1200:
                    requiredjsonfile = "registers_adin1200.json";
                    break;
                case DeviceType.ADIN1300:
                    requiredjsonfile = "registers_adin1300.json";
                    break;
                case DeviceType.ADIN1301:
                    requiredjsonfile = "registers_adin1301.json";
                    break;
                default:
                    break;
            }

            this.Info(string.Format("Loading registers from {0}", requiredjsonfile));
            this.jsonParser.ParseJSONData(Path.Combine(@"registers", requiredjsonfile));

            // Transfer of jsonParser.RegisterFieldMapping.Registers to Register
            Array.Sort(this.jsonParser.RegisterFieldMapping.Registers, delegate (RegisterDetails x, RegisterDetails y) { return x.Address.CompareTo(y.Address); });
            this.Registers = new ObservableCollection<RegisterDetails>();
            foreach (var register in this.jsonParser.RegisterFieldMapping.Registers)
            {
                // Removing the Fields that has RESERVED
                var fields = register.Fields.ToList();
                fields.RemoveAll(x => x.Name == "RESERVED");
                register.Fields = fields.ToArray();
                this.Registers.Add(register);
            }

            this.localAdvSpeedBitLookUp.Clear();

            this.localAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("Hd10Adv", EthernetSpeeds.SPEED_10BASE_T_HD));
            this.localAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("Fd10Adv", EthernetSpeeds.SPEED_10BASE_T_1L));//dani 20april SPEED_10BASE_T_FD));
            this.localAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("Hd100Adv", EthernetSpeeds.SPEED_100BASE_TX_HD));
            this.localAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("Fd100Adv", EthernetSpeeds.SPEED_100BASE_TX_FD));
            this.localAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("Eee100Adv", EthernetSpeeds.SPEED_100BASE_TX_EEE));
            this.localAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("Hd1000Adv", EthernetSpeeds.SPEED_1000BASE_T_HD));
            this.localAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("Fd1000Adv", EthernetSpeeds.SPEED_1000BASE_T_FD));
            this.localAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("Eee1000Adv", EthernetSpeeds.SPEED_1000BASE_T_EEE));

            this.remoteAdvSpeedBitLookUp.Clear();
            this.remoteAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("LpHd10Able", EthernetSpeeds.SPEED_10BASE_T_HD));
            this.remoteAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("LpFd10Able", EthernetSpeeds.SPEED_10BASE_T_1L));//dani 20aprilSPEED_10BASE_T_FD));
            this.remoteAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("LpHd100Able", EthernetSpeeds.SPEED_100BASE_TX_HD));
            this.remoteAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("LpFd100Able", EthernetSpeeds.SPEED_100BASE_TX_FD));
            this.remoteAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("LpEee100Able", EthernetSpeeds.SPEED_100BASE_TX_EEE));
            this.remoteAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("LpHd1000Able", EthernetSpeeds.SPEED_1000BASE_T_HD));
            this.remoteAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("LpFd1000Able", EthernetSpeeds.SPEED_1000BASE_T_FD));
            this.remoteAdvSpeedBitLookUp.Add(new RegisterLookUp<EthernetSpeeds>("LpEee1000Able", EthernetSpeeds.SPEED_1000BASE_T_EEE));
        }

        /// <summary>
        /// Reset FC statisctics
        /// </summary>
        public void ResetFrameCheckerStatistics()
        {
            this.checkedFrames = 0;
            this.checkedFramesErrors = 0;

            this.mseA_Max = 0x0;
            this.mseB_Max = 0x0;
            this.mseC_Max = 0x0;
            this.mseD_Max = 0x0;

            TargetInfoItem frameCheckerStatus = new TargetInfoItem(this.deviceSettingsUp.FrameCheckerStatus.ItemName);
            frameCheckerStatus.ItemContent = string.Format("{0:d} frames, {1:d} errors", this.checkedFrames, this.checkedFramesErrors);

            frameCheckerStatus.IsAvailable = this.deviceSettingsUp.FrameCheckerStatus.IsAvailable;

            this.deviceSettingsUp.FrameCheckerStatus = frameCheckerStatus;

            TargetInfoItem pairMeanSquareErrorStats = new TargetInfoItem(this.deviceSettingsUp.Link.PairMeanSquareErrorStats.ItemName);
            if ((this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_HD) || (this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_FD))
            {
                pairMeanSquareErrorStats.ItemContent = string.Format(" {0:d}, {1:d}, {2:d}, {3:d}", this.mseA_Max, this.mseB_Max, this.mseC_Max, this.mseD_Max);
            }
            else
            {
                pairMeanSquareErrorStats.ItemContent = string.Format(" {0:d}", this.mseA_Max);
            }

            pairMeanSquareErrorStats.IsAvailable = this.deviceSettingsUp.Link.PairMeanSquareErrorStats.IsAvailable;

            this.deviceSettingsUp.Link.PairMeanSquareErrorStats = pairMeanSquareErrorStats;
        }

        /// <summary>
        /// Execute the specified script
        /// </summary>
        /// <param name="scripttorun">Name of the script to run</param>
        /// <returns>Contents of register</returns>
        public uint RunScript(ScriptJSONStructure scripttorun)
        {
            bool ok = false;
            this.VerboseInfo("Running script : " + scripttorun.Script.Name);

            foreach (var regacc in scripttorun.Script.RegisterAccesses)
            {
                if (regacc.Description != null && regacc.Description != string.Empty)
                {
                    this.Info(regacc.Description);
                }

                if (regacc.MMap != null && regacc.Name != string.Empty)
                {
                    try
                    {
                        uint value = this.getValue(regacc.Value);
                        this.WriteYodaRg(regacc.MMap, regacc.Name, value);
                    }
                    catch (Exception ex)
                    {
                        this.Error($"{ex.Message} MemoryMap:{regacc.MMap}, RegisterName:{regacc.Name}, Value:{regacc.Value}");
                    }
                }
                else
                {
                    try
                    {
                        string resultString = string.Empty;
                        uint value = this.getValue(regacc.Value);
                        uint address = this.getAddress(regacc.RegisterAddress);
                        this.VerboseInfo(string.Format("Writing address 0x{0:X} with 0x{1:X}", address, value));
                        this.deviceConnection.WriteMDIORegister(address, value);
                    }
                    catch (Exception ex)
                    {
                        this.Error($"{ex.Message} RegisterAddress:{regacc.RegisterAddress}, Value:{regacc.Value}");
                    }
                }
            }

            return 0x0;
        }

        /// <summary>
        /// gets the address in the script
        /// </summary>
        /// <param name="inputValue">input address value</param>
        /// <returns>returns the address value</returns>
        private uint getAddress(string inputValue)
        {
            uint value = 0;
            string resultString = string.Empty;

            if (this.getPrefix(inputValue, out resultString))
            {
                if ((resultString.Substring(1) == "x")
                 || (resultString.Substring(1) == "X"))
                {
                    value = this.getHexValue(inputValue);
                }
                else
                {
                    value = this.getDecimalValue(inputValue);
                }
            }
            else
            {
                throw new Exception("Invalid Syntax.");
            }

            return value;
        }

        /// <summary>
        /// gets the value from script
        /// </summary>
        /// <param name="inputValue">value input</param>
        /// <returns>returns uint value</returns>
        private uint getValue(string inputValue)
        {
            uint value = 0;
            string resultString = string.Empty;

            if (this.getPrefix(inputValue, out resultString))
            {
                if ((resultString.Substring(1) == "x")
                 || (resultString.Substring(1) == "X"))
                {
                    value = this.getHexValue(inputValue);
                }
                else if ((resultString.Substring(1) == "d")
                      || (resultString.Substring(1) == "D"))
                {
                    value = this.getDecimalValue(inputValue);
                }
                else
                {
                    value = uint.Parse(resultString);
                }
            }
            else
            {
                uint resultParse = 0;
                if (uint.TryParse(resultString, out resultParse))
                {
                    value = resultParse;
                }
                else
                {
                    throw new Exception("Invalid Syntax.");
                }
            }

            return value;
        }

        /// <summary>
        /// gets the prefix value
        /// </summary>
        /// <param name="value">input string value</param>
        /// <param name="stringResult">string result</param>
        /// <returns>returns if the prefix is valid or invalid</returns>
        private bool getPrefix(string value, out string stringResult)
        {
            string pattern = @"^0[xXdD]";
            Regex rg = new Regex(pattern);

            var result = rg.Match(value);

            if (result.Success)
            {
                stringResult = result.Value;
                return true;
            }
            else
            {
                stringResult = value;
                return false;
            }
        }

        /// <summary>
        /// gets the Decimal value
        /// </summary>
        /// <param name="value">input string value</param>
        /// <returns>returns decimal value</returns>
        private uint getDecimalValue(string value)
        {
            string pattern = @"(?<=0[xXdD])\d*";
            Regex rg = new Regex(pattern);

            var readValue = rg.Match(value);
            uint resultValue = 0;

            var parseResult = uint.TryParse(readValue.Value, out resultValue);

            if (parseResult)
            {
                return resultValue;
            }
            else
            {
                throw new Exception("Invalid value.");
            }
        }

        /// <summary>
        /// gets the Hex value
        /// </summary>
        /// <param name="value">input string value</param>
        /// <returns>returns hex value</returns>
        private uint getHexValue(string value)
        {
            string pattern = @"(?<=0[xXdD])[0-9a-zA-Z]+";
            Regex rg = new Regex(pattern);

            var readValue = rg.Match(value);
            uint resultValue = 0;

            var parseResult = uint.TryParse(readValue.Value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out resultValue);

            if (parseResult)
            {
                return resultValue;
            }
            else
            {
                throw new Exception("Invalid value.");
            }
        }

        /// <summary>
        /// Return the contents of the specified register or bitfield.
        /// </summary>
        /// <param name="mMap">Name of memory map that the register is in</param>
        /// <param name="name">Register or Bifield name to access in the memory map</param>
        /// <returns>Contents of register</returns>
        public uint ReadYodaRg(string mMap, string name)
        {
            uint value;
            RegisterInfo registerInfo = this.LookUpAccessDefinition(mMap, name);

            if (!this.regContents.TryGetValue(registerInfo.Address, out value))
            {
                value = this.deviceConnection.ReadMDIORegister(registerInfo.Address);
                this.regContents.Add(registerInfo.Address, value);
            }

            return registerInfo.ExtractFieldValue(value);
        }

        /// <summary>
        /// Read value from specified register address
        /// </summary>
        /// <param name="regAddr">Register address</param>
        /// <returns>Value in register</returns>
        public uint ReadValueInRegisterAddress(uint regAddr)
        {
            return this.ReadCheckYodaRg(regAddr);
        }

        /// <summary>
        /// Write value to register
        /// </summary>
        /// <param name="regAddr">Register address</param>
        /// <param name="regVal">Value to write to register</param>
        public void WriteValueInRegisterAddress(uint regAddr, uint regVal)
        {
            this.WriteYodaRg(regAddr, regVal);
        }

        /// <summary>
        /// Update our field contents for the rgeister view
        /// </summary>
        private void UpdateFieldsFromRegContents()
        {
            uint regContent;

            foreach (var registerDetail in this.Registers)
            {
                if (this.regContents.TryGetValue(registerDetail.Address, out regContent))
                {
                    foreach (var fieldDetail in registerDetail.Fields)
                    {
                        var registerInfo = new RegisterInfo(registerDetail, fieldDetail);
                        fieldDetail.Value = registerInfo.ExtractFieldValue(regContent);
                    }

                    registerDetail.Value = regContent;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FirmwareAPI"/> class
        /// </summary>
        /// <param name="deviceConnection">Parameter Description</param>
        public void AttachDevice(DeviceConnection deviceConnection)
        {
            this.deviceConnection = deviceConnection;
            this.deviceConnection.PropertyChanged += this.DeviceConnection_PropertyChanged;
        }

        /// <summary>
        /// Dump out all register information to window
        /// </summary>
        public void DumpRegisterContents()
        {
            uint value;
            RegisterInfo regInfo;

            foreach (RegisterDetails registerDetail in this.jsonParser.RegisterFieldMapping.Registers)
            {
                if (registerDetail.IncludeInDump)
                {
                    value = this.deviceConnection.ReadMDIORegister(registerDetail.Address);
                    regInfo = new RegisterInfo(registerDetail);
                    this.VerboseInfo(registerDetail.Name + string.Format(" = 0x{0:X} [ ", value) + registerDetail.Desc.Replace("\n", string.Empty).Replace("\r", string.Empty) + " ]");
                    foreach (FieldDetails fieldDetail in registerDetail.Fields)
                    {
                        if ((fieldDetail.Name != "RESERVED") && fieldDetail.IncludeInDump)
                        {
                            regInfo = new RegisterInfo(registerDetail, fieldDetail);
                            this.VerboseInfo("  --> " + fieldDetail.Name + string.Format(" = 0x{0:X} [ ", regInfo.ExtractFieldValue(value)) + fieldDetail.Documentation.Replace("\n", string.Empty).Replace("\r", string.Empty) + " ]");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Save Register contents to file
        /// </summary>
        /// <param name="filename">Name of file</param>
        public void SaveRegisterContents(string filename)
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.jsonParser.RegisterFieldMapping.Registers.GetType());
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            this.Info(string.Format("Refreshing register contents before saving to {0}", filename));
            this.RefreshRegisterContents();
            this.Info(string.Format("Refreshing register contents done."));
            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                x.Serialize(writer, this.jsonParser.RegisterFieldMapping.Registers);
                this.Info(string.Format("Registers saved to '{0}'", filename));
            }
        }

        /// <summary>
        /// Load Register contents from file
        /// </summary>
        /// <param name="filename">Name of file</param>
        public void LoadRegisterContents(string filename)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                RegisterDetails[] registers;
                uint regwritten = 0;

                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.jsonParser.RegisterFieldMapping.Registers.GetType());

                registers = (RegisterDetails[])x.Deserialize(reader);

                foreach (RegisterDetails registerDetailsrc in registers)
                {
                    // If the value is different to the current value
                    foreach (RegisterDetails registerDetaildst in this.jsonParser.RegisterFieldMapping.Registers)
                    {
                        if (registerDetailsrc.Address == registerDetaildst.Address)
                        {
                            if ((registerDetailsrc.Address != 0x10) && (registerDetailsrc.Address != 0x11) && (registerDetaildst.Access != "R"))
                            {
                                if (registerDetailsrc.Value != registerDetaildst.Value)
                                {
                                    regwritten += 1;
                                    this.WriteYodaRg(registerDetailsrc.MMap, registerDetailsrc.Name, registerDetailsrc.Value);
                                    this.VerboseInfo(string.Format("Register '{0}' is 0x{2:X} and will be written with 0x{1:X}", registerDetailsrc.Name, registerDetailsrc.Value, registerDetaildst.Value));
                                }
                            }

                            break;
                        }
                    }
                }

                this.Info(string.Format("Registers loaded from '{0}'. {1} register(s) written.", filename, regwritten));
            }
        }

        /// <summary>
        /// Dump out all register information to window
        /// </summary>
        public void RefreshRegisterContents()
        {
            uint regContent;
            RegisterInfo fieldInfo;

            foreach (RegisterDetails registerDetail in this.jsonParser.RegisterFieldMapping.Registers)
            {
                regContent = this.ReadYodaRg(registerDetail.MMap, registerDetail.Name);
                foreach (FieldDetails fieldDetail in registerDetail.Fields)
                {
                    fieldInfo = new RegisterInfo(registerDetail, fieldDetail);
                    fieldDetail.Value = fieldInfo.ExtractFieldValue(regContent);
                }

                registerDetail.Value = regContent;
            }
        }

        /// <summary>
        /// Report PHY Status.
        /// </summary>
        public void ReportPhyStatus()
        {
            this.regContents.Clear();

            this.RefreshConnectedDevice();

            if (this.TenSPEDevice())
            {
                bool inSwPowerDown = this.ReadYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD") == 1;

                this.deviceSettingsUp.Link.LinkEstablished = this.ReadYodaRg("IndirectAccessAddressMap", "AN_LINK_STATUS") == 1;
                if (inSwPowerDown)
                {
                    this.deviceSettingsUp.PhyState = EthPhyState.Powerdown;
                }
                else
                {
                    //if (!this.deviceSettingsUp.Link.LinkingEnabled || inStndbyMode)
                    //{
                    //    this.deviceSettingsUp.PhyState = EthPhyState.Standby;
                    // }
                    //else
                    {
                        if (!this.deviceSettingsUp.Link.LinkEstablished)
                        {
                            this.deviceSettingsUp.PhyState = EthPhyState.LinkDown;
                        }
                        else
                        {
                            this.deviceSettingsUp.PhyState = EthPhyState.LinkUp;
                        }
                    }
                }

                this.deviceSettingsUp.Link.LinkingEnabled = true;
                this.deviceSettingsUp.EthSpeedSelection = EthSpeedMode.Forced;

                this.deviceSettingsUp.Fixed.FixedMasterSlave = MasterSlaveFixed.Master;
                this.deviceSettingsUp.Negotiate.PreferMasterSlave = MasterSlavePreference.Master;
                this.deviceSettingsUp.Negotiate.AutoNegCompleted = true;
                this.deviceSettingsUp.Link.ResolvedHCD = EthernetSpeeds.SPEED_10BASE_T_1L;//dani 20april SPEED_10BASE_T_FD; // 10SPE uses a fixed speed
                this.deviceSettingsUp.Link.FrameGenRunning = false;
                this.deviceSettingsUp.Link.FrameGenRunning = false;
                if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_B10L_TX_LVL_HI_ABL") == 1)
                {
                    this.deviceSettingsUp.Negotiate.PkPkVoltage = SignalPeakToPeakVoltage.Capable2p4Volts_Requested2p4Volts;//CapableTwoPointFourVolts_RequestedTwoPointFourVolts;
                }
                else
                {
                    this.deviceSettingsUp.Negotiate.PkPkVoltage = SignalPeakToPeakVoltage.Capable1Volt;//CapableOneVolt;
                }

                this.RefreshNegotiateMasterSlaveSetting(); //dani Do we need to refresh the settings?              
                this.RefreshTenSPEStatusItem();
                this.FrameGeneratorStatus();
                this.FrameCheckerStatus();

                /* The remainder of these might not be relevant for 10SPE */
                this.deviceSettingsUp.Link.InEnergyPowerDown = false;
                this.deviceSettingsUp.EPDMode = EnergyPowerDownMode.Disabled;
                this.deviceSettingsUp.Negotiate.DownSpeed10Enabled = false;
                this.deviceSettingsUp.Negotiate.DownSpeed100Enabled = false;
                this.deviceSettingsUp.Negotiate.DownSpeed100Enabled = false;
                this.deviceSettingsUp.Negotiate.DownSpeedRetries = 0x0;
                this.deviceSettingsUp.MdixMode = AutoMdixMode.FixedMdi;
                this.deviceSettingsUp.Negotiate.Advertise10HD = false;
                this.deviceSettingsUp.Negotiate.Advertise10FD = false;
                this.deviceSettingsUp.Negotiate.Advertise100HD = false;
                this.deviceSettingsUp.Negotiate.Advertise100FD = false;
                this.deviceSettingsUp.Negotiate.AdvertiseEEE100 = false;
                this.deviceSettingsUp.Negotiate.Advertise1000HD = false;
                this.deviceSettingsUp.Negotiate.Advertise1000FD = false;
                this.deviceSettingsUp.Negotiate.AdvertiseEEE1000 = false;
                this.deviceSettingsUp.Link.FreqOffsetPpm = 0.0;
                this.deviceSettingsUp.Link.LocalRcvrOk = true;
                this.deviceSettingsUp.Link.RemoteRcvrOk = true;
            }
            else
            {
                bool inStndbyMode = this.ReadYodaRg("GEPhy", "PhyInStndby") == 1;
                bool inSwPowerDown = this.ReadYodaRg("GEPhy", "SftPd") == 1;

                this.deviceSettingsUp.Link.InEnergyPowerDown = this.ReadYodaRg("GEPhy", "PhyInNrgPd") == 1;
                this.deviceSettingsUp.Link.LinkEstablished = this.ReadYodaRg("GEPhy", "LinkStatLat") == 1;
                this.deviceSettingsUp.Link.LinkingEnabled = this.ReadYodaRg("GEPhy", "LinkEn") == 1;

                if (this.ReadYodaRg("GEPhy", "AutonegEn") == 1)
                {
                    this.deviceSettingsUp.EthSpeedSelection = EthSpeedMode.Advertised;
                }
                else
                {
                    this.deviceSettingsUp.EthSpeedSelection = EthSpeedMode.Forced;
                }

                this.deviceSettingsUp.Negotiate.AutoNegCompleted = this.ReadYodaRg("GEPhy", "AutonegStat") == 1;

                this.RefreshAutoMdixMode_st();
                this.RefreshEdPdConfig();
                this.RefreshDnSpeed10Enable();
                this.RefreshDnSpeed100Enable();
                this.RefreshDnSRtrs_st();
                this.RefreshLocallyAdvertisedSpeeds();
                this.RefreshRemoteAdvertisedSpeeds();
                this.RefreshMasterSlave();
                this.RefreshForcedSpeeds();
                this.RefreshTenSPEStatusItem();

                if (inSwPowerDown)
                {
                    this.deviceSettingsUp.PhyState = EthPhyState.Powerdown;
                }
                else
                {
                    if (!this.deviceSettingsUp.Link.LinkingEnabled || inStndbyMode)
                    {
                        this.deviceSettingsUp.PhyState = EthPhyState.Standby;
                    }
                    else
                    {
                        if (!this.deviceSettingsUp.Link.LinkEstablished)
                        {
                            this.deviceSettingsUp.PhyState = EthPhyState.LinkDown;
                        }
                        else
                        {
                            this.deviceSettingsUp.PhyState = EthPhyState.LinkUp;
                        }
                    }
                }

                switch (this.deviceSettingsUp.PhyState)
                {
                    case EthPhyState.Powerdown:
                    case EthPhyState.Standby:
                        this.RefreshNonLinkedStatus();
                        break;
                    case EthPhyState.LinkDown:
                        this.RefreshNonLinkedStatus();
                        /* Linking is enabled...but currently no link established */
                        if (this.deviceSettingsUp.Link.InEnergyPowerDown)
                        {
                            // this.VerboseInfo("    PHY in Energy Detect PD:");
                        }

                        break;
                    case EthPhyState.LinkUp:
                        this.RefreshLinkedStatus();
                        break;
                    default:
                        break;
                }

                if (this.deviceSettingsUp.PhyState >= EthPhyState.LinkDown)
                {
                    if (this.deviceSettingsUp.EthSpeedSelection == EthSpeedMode.Advertised)
                    {
                        // this.VerboseInfo(string.Format("Local PHY advertised: {0:s}", string.Join(" ", this.deviceSettingsUp.Negotiate.LocalAdvSpeeds)));
                        bool autoNegRemotePhyEnabled = this.ReadYodaRg("GEPhy", "AutonegSup") == 1;
                        if (autoNegRemotePhyEnabled)
                        {
                            // Auto-Neg enabled on both Sides
                            // this.VerboseInfo(string.Format("Remote PHY advertised: {0:s}", string.Join(" ", this.deviceSettingsUp.Negotiate.RemoteAdvSpeeds)));
                        }
                        else
                        {
                            // Auto-Neg Enabled just on Local PHY
                        }

                        if (this.deviceSettingsUp.Negotiate.AutoNegCompleted)
                        {
                            // this.VerboseInfo("Auto-Negotiation complete");
                        }
                        else
                        {
                            // this.VerboseInfo("Auto-Negotiation not complete");
                        }
                    }
                    else
                    {
                        // this.VerboseInfo("Auto-Neg Disabled:");
                    }
                }

                if (this.deviceSettingsUp.PhyState == EthPhyState.LinkUp)
                {
                }

                this.FrameGeneratorStatus();
                this.FrameCheckerStatus();
                this.CableDiagnosticsStatus();
            }

            if (this.deviceSettingsUp.PropertiesChangedList.Count != 0)
            {
                this.RaisePropertyChanged("DeviceSettings");
            }

            this.UpdateFieldsFromRegContents();
        }

        /// <summary>
        /// Configure Forced Speed Selection
        /// </summary>
        /// <param name="frcSpdSel_st">Parameter Description</param>
        public void ForcedSpeedSelect(EthernetSpeeds frcSpdSel_st)
        {
            switch (frcSpdSel_st)
            {
                case EthernetSpeeds.SPEED_10BASE_T_HD:
                    this.Info("    10BASE-T half duplex forced speed selected ");
                    this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
                    this.WriteYodaRg("GEPhy", "SpeedSelLsb", 0);
                    this.WriteYodaRg("GEPhy", "DplxMode", 0);
                    break;
                case EthernetSpeeds.SPEED_10BASE_T_1L://dani 20aprilSPEED_10BASE_T_FD:
                    this.Info("    10BASE-T full duplex forced speed selected ");
                    this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
                    this.WriteYodaRg("GEPhy", "SpeedSelLsb", 0);
                    this.WriteYodaRg("GEPhy", "DplxMode", 1);
                    break;
                case EthernetSpeeds.SPEED_100BASE_TX_HD:
                    this.Info("    100BASE-TX half duplex forced speed selected ");
                    this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
                    this.WriteYodaRg("GEPhy", "SpeedSelLsb", 1);
                    this.WriteYodaRg("GEPhy", "DplxMode", 0);
                    break;
                case EthernetSpeeds.SPEED_100BASE_TX_FD:
                    this.Info("    100BASE-TX full duplex forced speed selected ");
                    this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
                    this.WriteYodaRg("GEPhy", "SpeedSelLsb", 1);
                    this.WriteYodaRg("GEPhy", "DplxMode", 1);
                    break;
                case EthernetSpeeds.SPEED_1000BASE_T_FD:
                    this.Info("    1000BASE-T forced speed selected ");
                    this.WriteYodaRg("GEPhy", "SpeedSelLsb", 0);
                    this.WriteYodaRg("GEPhy", "SpeedSelMsb", 1);
                    this.WriteYodaRg("GEPhy", "DplxMode", 1);
                    break;
                default:
                    this.Error("Forced Speed Not Configured - Use one of  10BASE-T HD / 10BASE-T FD / 100BASE-TX HD / 100BASE-TX FD / 1000BASE-T");
                    break;
            }
        }

        /// <summary>
        /// Open the connection.
        /// </summary>
        public void Open()
        {
            this.deviceConnection.Open();
            //dani
            // Decide if an immediate switch of the loaded yoda file might make sense now
            if ((this.deviceConnection.IsDevRecognised() != true) && (this.deviceConnection.DeviceDescription == DeviceConnection.DeviceDescriptionEVALADIN11xx) && (this.deviceSettingsUp.ConnectedDeviceType != DeviceType.ADIN1100))
            {
                // Assume ADIN1100 until we connect and find out differently
                this.deviceSettingsUp.ConnectedDeviceType = DeviceType.ADIN1100;
                this.UpdatefromRegisterJSON(this.deviceSettingsUp.ConnectedDeviceType);
                this.ScanMDIOHwAddress();
                this.deviceConnection.IsDevRecognised(true);
            }
            else
            if ((this.deviceConnection.IsDevRecognised() != true) && (this.deviceConnection.DeviceDescription != DeviceConnection.DeviceDescriptionEVALADIN11xx) && (this.deviceSettingsUp.ConnectedDeviceType == DeviceType.ADIN1100))
            {
                // Assume ADIN1300 until we connect and find out differently
                this.deviceSettingsUp.ConnectedDeviceType = DeviceType.ADIN1100;
                this.UpdatefromRegisterJSON(this.deviceSettingsUp.ConnectedDeviceType);
                this.ScanMDIOHwAddress();
                this.deviceConnection.IsDevRecognised(true);
            }

            if ((this.deviceConnection.IsDevRecognised() != true) && (this.deviceConnection.DeviceDescription == DeviceConnection.DeviceDescriptionEVALADIN11xx) && (this.deviceSettingsUp.ConnectedDeviceType == DeviceType.ADIN1100))
            {
                // Assume ADIN1300 until we connect and find out differently
                this.deviceSettingsUp.ConnectedDeviceType = DeviceType.ADIN1100;
                this.UpdatefromRegisterJSON(this.deviceSettingsUp.ConnectedDeviceType);
                this.ScanMDIOHwAddress();
                this.deviceConnection.IsDevRecognised(true);
            }
        }

        /// <summary>
        /// Open the connection.
        /// </summary>
        public void Close()
        {
            this.deviceConnection.Close();
        }

        /// <summary>
        /// This method is called when the feedback of Device Connection is set
        /// </summary>
        /// <param name="sender">The firmware wrapper instance</param>
        /// <param name="e">Property Changes arguments</param>
        public void DeviceConnection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DeviceConnection deviceConnection = (DeviceConnection)sender;
            switch (e.PropertyName)
            {
                case "FeedbackOfActions":
                    /* Send it along */
                    this.FeedbackOfActions = deviceConnection.FeedbackOfActions;
                    break;
            }
        }

        /// <summary>
        /// Enable downspeed to 100BASE-TX
        /// </summary>
        /// <param name="dnS10En_st">Parameter Description</param>
        private void DnSpeed10Enable(bool dnS10En_st = true)
        {
            if (dnS10En_st)
            {
                this.Info("    enable downspeed to 10BASE-T");
                this.WriteYodaRg("GEPhy", "DnSpeedTo10En", 1);
            }
            else
            {
                this.Info("    disable downspeed to 10BASE-T");
                this.WriteYodaRg("GEPhy", "DnSpeedTo10En", 0);
            }
        }

        /// <summary>
        /// Enable downspeed to 100BASE-TX
        /// </summary>
        /// <param name="dnS100En_st">Parameter Description</param>
        private void DnSpeed100Enable(bool dnS100En_st = true)
        {
            if (dnS100En_st)
            {
                this.Info("    enable downspeed to 100BASE-TX");
                this.WriteYodaRg("GEPhy", "DnSpeedTo100En", 1);
            }
            else
            {
                this.Info("    disable downspeed to 100BASE-TX");
                this.WriteYodaRg("GEPhy", "DnSpeedTo100En", 0);
            }
        }

        /// <summary>
        /// Configure Energy Detect Power Down mode
        /// </summary>
        /// <param name="edPdMode_st">Requested Energy detect powerdown mode</param>
        private void EdPdConfig(EnergyPowerDownMode edPdMode_st)
        {
            switch (edPdMode_st)
            {
                case EnergyPowerDownMode.EnabledWithPeriodicPulseTX:
                    this.Info("    enable EDPD - with periodic transmission of pulse");
                    this.WriteYodaRg("GEPhy", "NrgPdEn", 1);
                    this.WriteYodaRg("GEPhy", "NrgPdTxEn", 1);
                    break;
                case EnergyPowerDownMode.Enabled:
                    this.Info("    enable EDPD - no transmission of pulse");
                    this.WriteYodaRg("GEPhy", "NrgPdEn", 1);
                    this.WriteYodaRg("GEPhy", "NrgPdTxEn", 0);
                    break;
                case EnergyPowerDownMode.Disabled:
                    this.Info("    disable EDPD");
                    this.WriteYodaRg("GEPhy", "NrgPdEn", 0);
                    break;
                default:
                    this.Info("    EDPD Not Configured - Use one of EDPD_w_Tx / EDPD_no_Tx / EDPD_Off");
                    break;
            }
        }

        /// <summary>
        /// Refresh Energy Detect Power Down mode
        /// </summary>
        private void RefreshEdPdConfig()
        {
            EnergyPowerDownMode edPdMode_st = EnergyPowerDownMode.Disabled;
            uint edpdE_val = this.ReadYodaRg("GEPhy", "NrgPdEn");
            uint edpdTx_val = this.ReadYodaRg("GEPhy", "NrgPdTxEn");

            if (edpdE_val != 0)
            {
                if (edpdTx_val != 0)
                {
                    edPdMode_st = EnergyPowerDownMode.EnabledWithPeriodicPulseTX;
                }
                else
                {
                    edPdMode_st = EnergyPowerDownMode.Enabled;
                }
            }

            this.deviceSettingsUp.EPDMode = edPdMode_st;
        }

        /// <summary>
        /// Refresh DnSpeed10Enable
        /// </summary>
        private void RefreshDnSpeed10Enable()
        {
            this.deviceSettingsUp.Negotiate.DownSpeed10Enabled = this.ReadYodaRg("GEPhy", "DnSpeedTo10En") == 1;
        }

        /// <summary>
        /// Refresh DnSpeed100Enable
        /// </summary>
        private void RefreshDnSpeed100Enable()
        {
            if (this.one1GCapabable)
            {
                this.deviceSettingsUp.Negotiate.DownSpeed100Enabled = this.ReadYodaRg("GEPhy", "DnSpeedTo100En") == 1;
            }
            else
            {
                this.deviceSettingsUp.Negotiate.DownSpeed100Enabled = false;
            }
        }

        /// <summary>
        /// Configure downspeed retries
        /// </summary>
        /// <param name="dnSRtrs_st">Parameter Description</param>
        private void DnSpeedRetries(uint dnSRtrs_st = 4)
        {
            if ((dnSRtrs_st >= 0) && (dnSRtrs_st <= 7))
            {
                this.WriteYodaRg("GEPhy", "NumSpeedRetry", dnSRtrs_st);
            }
            else
            {
                this.Error("downspeed retries Not Configured - must be a value between 0 and 7");
            }
        }

        /// <summary>
        /// Refresh Downspeed retries
        /// </summary>
        private void RefreshDnSRtrs_st()
        {
            this.deviceSettingsUp.Negotiate.DownSpeedRetries = this.ReadYodaRg("GEPhy", "NumSpeedRetry");
        }

        /// <summary>
        /// Configure Auto MDIX; used in Auto CrossOver Testing
        /// </summary>
        /// <param name="autoMdixMode_st">Parameter Description</param>
        private void AutoMdixConfig(AutoMdixMode autoMdixMode_st)
        {
            switch (autoMdixMode_st)
            {
                case AutoMdixMode.Auto:
                    this.WriteYodaRg("GEPhy", "AutoMdiEn", 1);
                    break;
                case AutoMdixMode.FixedMdi:
                    this.WriteYodaRg("GEPhy", "AutoMdiEn", 0);
                    this.WriteYodaRg("GEPhy", "ManMdix", 0);
                    break;
                case AutoMdixMode.FixedMdix:
                    this.WriteYodaRg("GEPhy", "AutoMdiEn", 0);
                    this.WriteYodaRg("GEPhy", "ManMdix", 1);
                    break;
                default:
                    this.Error("    AutoMdixMode Not Set - Use one of Auto / FixedMdi / FixedMdix");
                    break;
            }
        }

        /// <summary>
        /// Refresh Auto MDIX
        /// </summary>
        private void RefreshAutoMdixMode_st()
        {
            uint aMdix_val = this.ReadYodaRg("GEPhy", "AutoMdiEn");
            uint manMdix_val = this.ReadYodaRg("GEPhy", "ManMdix");

            AutoMdixMode autoMdixMode_st = AutoMdixMode.Auto;

            if (aMdix_val != 0)
            {
                autoMdixMode_st = AutoMdixMode.Auto;
            }
            else
            {
                if (manMdix_val != 0)
                {
                    autoMdixMode_st = AutoMdixMode.FixedMdix;
                }
                else
                {
                    autoMdixMode_st = AutoMdixMode.FixedMdi;
                }
            }

            this.deviceSettingsUp.MdixMode = autoMdixMode_st;
        }

        // Changed Func

        /// <summary>
        /// Configure as Manual Master or Manual Slave
        /// </summary>
        /// <param name="msMasCfg_st">Parameter Description</param>
        private void ManualMasterSlaveConfig(MasterSlaveFixed msMasCfg_st)
        {
            if (msMasCfg_st == MasterSlaveFixed.Master)
            {
                this.WriteYodaRg("GEPhy", "ManMstrSlvEnAdv", 1);
                this.WriteYodaRg("GEPhy", "ManMstrAdv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "ManMstrSlvEnAdv", 1);
                this.WriteYodaRg("GEPhy", "ManMstrAdv", 0);
            }
        }

        /// <summary>
        /// Advertise Prefer Master or Prefer Slave
        /// </summary>
        /// <param name="msPrefMas_st">Parameter Description</param>
        private void PreferMasterSlaveAdvertisement(MasterSlavePreference msPrefMas_st)
        {
            if (msPrefMas_st == MasterSlavePreference.Master)
            {
                this.WriteYodaRg("GEPhy", "ManMstrSlvEnAdv", 0);
                this.WriteYodaRg("GEPhy", "PrefMstrAdv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "ManMstrSlvEnAdv", 0);
                this.WriteYodaRg("GEPhy", "PrefMstrAdv", 0);
            }
        }

        /// <summary>
        /// Pk to pk Voltage Settings
        /// </summary>
        /// <param name="pkpkVoltage">Parameter Description</param>
        private void PeakToPeakVoltageSetting(SignalPeakToPeakVoltage pkpkVoltage)
        {
            if (pkpkVoltage == SignalPeakToPeakVoltage.Capable2p4Volts_Requested2p4Volts)//CapableTwoPointFourVolts_RequestedTwoPointFourVolts)
            {
                // Configuring for high voltage transmit levels 2.4VPkpk
                this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_B10L_TX_LVL_HI_ABL", 1);
                this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_B10L_TX_LVL_HI_REQ", 1);
            }
            else if (pkpkVoltage == SignalPeakToPeakVoltage.Capable2p4Volts_Requested1Volt)//CapableTwoPointFourVolts_RequestedOneVolt)
            {
                // Configuring for high voltage transmit levels 2.4VPkpk
                this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_B10L_TX_LVL_HI_ABL", 1);
                this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_B10L_TX_LVL_HI_REQ", 0);
            }

            else
            {
                // Configuring for low voltage transmit levels 1.0VPk-pk AnAdvB10lTxLvlHiAbl
                this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_B10L_TX_LVL_HI_ABL", 0);
            }

            // Renegotiate immediately
            this.RestartANeg();
        }

        private void RefreshNegotiateMasterSlaveSetting()
        {
#if MASTER_SLAVE_NEGOTIATE
            if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_EN") == 1)
            {
                ////////dani 20April  this.deviceSettingsUp.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Negotiate;
                //////  uint masterSlave = this.ReadYodaRg("IndirectAccessAddressMap", "AN_MS_CONFIG_RSLTN");
                //////  switch (masterSlave)
                //////  {
                //////      case 0:
                //////          {
                //////              //Not Run
                //////          }

                //////          break;
                //////      case 1:
                //////          {
                //////              //Configuration Fault
                //////              //Configuration Fault
                //////              // Configuration Fault

                //////              // FYI: the function call for this method was comment out.
                //////              // Initializing the AN Status
                //////            //  TargetInfoItem anStatus = new TargetInfoItem(this.deviceSettingsUp.Link.AnStatus.ItemName);
                //////            //  anStatus.IsAvailable = this.TenSPEDevice();//&& this.deviceSettingsUp.PhyState != EthPhyState.LinkUp; // this is the condition for the visibility in the UI

                //////              // start the code here

                //////              // this how to set the AN Status
                //////             // anStatus.ItemContent = "Configuration Fault";// ANStatus.Config_Fault.ToString().Replace("_", " ");  // this converts from enum type to string type
                //////             // anStatus.ItemContent = ANStatus.AN_Done.ToString().Replace("_", " ");       // this converts from enum type to string type
                //////             // anStatus.ItemContent = ANStatus.AN_Link_Good.ToString().Replace("_", " ");  // this converts from enum type to string type

                //////              // end the code here

                //////             // this.deviceSettingsUp.Link.AnStatus = anStatus;
                //////          }

                //////          break;
                //////      case 2:
                //////          {
                //////              if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS") == 1)
                //////              {
                //////                  this.DeviceSettings.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Forced_Slave;
                //////              }
                //////              else
                //////              {
                //////                  this.DeviceSettings.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Prefer_Slave;
                //////              }
                //////          }

                //////          break;
                //////      case 3:
                //////          {
                //////              if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS") == 1)
                //////              {
                //////                  this.DeviceSettings.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Forced_Master;
                //////              }
                //////              else
                //////              {
                //////                  this.DeviceSettings.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Prefer_Master;
                //////              }
                //////          }

                //////          break;
                //////  }
                //// }
                ////else
                ////{
                ////    uint masterSlave = this.ReadYodaRg("IndirectAccessAddressMap", "AN_MS_CONFIG_RSLTN");	AN_ADV_MST
                if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_MST") == 1)
                {
                    if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS") == 1)
                    {
                        this.DeviceSettings.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Forced_Master;
                    }
                    else
                    {
                        this.DeviceSettings.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Prefer_Master;
                    }
                }
                else
                {
                    if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS") == 1)
                    {
                        this.DeviceSettings.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Forced_Slave;
                    }
                    else
                    {
                        this.DeviceSettings.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Prefer_Slave;
                    }
                }
            }
#else
            this.deviceSettingsUp.Negotiate.NegotiateMasterSlave = MasterSlaveNegotiate.Negotiate;
#endif
        }

        /// <summary>
        /// Master Slave Negotiate Settings
        /// </summary>
        /// <param name="negotiateMasterSlave">Parameter Description</param>
        private void NegotiateMasterSlaveSetting(MasterSlaveNegotiate negotiateMasterSlave)
        {
            // CfgMst is used only when Auto-Negotiation is disabled; otherwise this value is determined by the Auto-Negotiation process itself.
            // CFG_MST
            // MASTER-SLAVE config value
            //    1 = Configure as MASTER
            //    0 = Configure as SLAVE

            // AnEn
            // AN_EN
            // Auto-Negotiation enable.
            //    1 = enable Auto-Negotiation
            //    0 = disable Auto-Negotiation (use AnFrcModeEn register to enable forced link configuration mode)

            // AnFrcModeEn
            // AN_ADV_FORCE_MS
            // Autonegotiation Forced Mode, Applies selection in CFG_MST as a forced during AN
            // Enables forced mode functionality
            switch (negotiateMasterSlave)
            {
                //                case MasterSlaveNegotiate.Negotiate:
                // Allow it to negotiate
                // this.WriteYodaRg("IndirectAccessAddressMap", "CFG_MST", 0);
                //                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 1);

                // this.WriteYodaRg("IndirectAccessAddressMap", "AN_FRC_MODE_EN", 0);
                //break;
#if MASTER_SLAVE_NEGOTIATE
                case MasterSlaveNegotiate.Prefer_Master:
                    // Configure loc as forced Master
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_MST", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
                    //this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 0);

                    //  this.WriteYodaRg("IndirectAccessAddressMap", "AN_FRC_MODE_EN", 1);
                    break;
                case MasterSlaveNegotiate.Prefer_Slave:
                    // Configure rem as forced Slave"
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_MST", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
                    // this.WriteYodaRg("IndirectAccessAddressMap", "AN_FRC_MODE_EN", 1);
                    break;
                case MasterSlaveNegotiate.Forced_Master:
                    // Configure loc as forced Master
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_MST", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
                    //this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 0);

                    //  this.WriteYodaRg("IndirectAccessAddressMap", "AN_FRC_MODE_EN", 1);
                    break;
                case MasterSlaveNegotiate.Forced_Slave:
                    // Configure rem as forced Slave"
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_MST", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
                    // this.WriteYodaRg("IndirectAccessAddressMap", "AN_FRC_MODE_EN", 1);
                    break;
#endif
                default:
                    // Allow it to negotiate
                    // this.WriteYodaRg("IndirectAccessAddressMap", "CFG_MST", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 1);

                    // this.WriteYodaRg("IndirectAccessAddressMap", "AN_FRC_MODE_EN", 0);
                    break;
            }
        }

        /// <summary>
        /// Refresh Master Slave
        /// </summary>
        private void RefreshMasterSlave()
        {
            if (this.deviceSettingsUp.Negotiate.LocalAdvSpeeds.Contains(EthernetSpeeds.SPEED_1000BASE_T_FD))
            {
                if (this.ReadYodaRg("GEPhy", "ManMstrAdv") != 0)
                {
                    this.deviceSettingsUp.Fixed.FixedMasterSlave = MasterSlaveFixed.Master;
                }
                else
                {
                    this.deviceSettingsUp.Fixed.FixedMasterSlave = MasterSlaveFixed.Slave;
                }

                if (this.ReadYodaRg("GEPhy", "PrefMstrAdv") != 0)
                {
                    this.deviceSettingsUp.Negotiate.PreferMasterSlave = MasterSlavePreference.Master;
                }
                else
                {
                    this.deviceSettingsUp.Negotiate.PreferMasterSlave = MasterSlavePreference.Slave;
                }

                /* This is not conveying any EXTRA information */
                if (this.ReadYodaRg("GEPhy", "ManMstrSlvEnAdv") != 0)
                {
                }
                else
                {
                }
            }
        }

        /// <summary>
        /// Configure 10BASE-T Half-Duplex advertisement
        /// </summary>
        /// <param name="advertise">Parameter Description</param>
        private void Speed10HdAdvertisement(bool advertise)
        {
            if (advertise)
            {
                this.WriteYodaRg("GEPhy", "Hd10Adv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "Hd10Adv", 0);
            }
        }

        /// <summary>
        /// Configure 10BASE-T Full-Duplex advertisement
        /// </summary>
        /// <param name="spd10FdAdv_st">Parameter Description</param>
        private void Speed10FdAdvertisement(bool spd10FdAdv_st = true)
        {
            if (spd10FdAdv_st)
            {
                this.WriteYodaRg("GEPhy", "Fd10Adv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "Fd10Adv", 0);
            }
        }

        /// <summary>
        /// Configure 100BASE-TX Half-Duplex advertisement
        /// </summary>
        /// <param name="spd100HdAdv_st">Parameter Description</param>
        private void Speed100HdAdvertisement(bool spd100HdAdv_st = true)
        {
            if (spd100HdAdv_st)
            {
                this.WriteYodaRg("GEPhy", "Hd100Adv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "Hd100Adv", 0);
            }
        }

        /// <summary>
        /// Configure 100BASE-TX Full-Duplex advertisement
        /// </summary>
        /// <param name="spd100FdAdv_st">Parameter Description</param>
        private void Speed100FdAdvertisement(bool spd100FdAdv_st = true)
        {
            if (spd100FdAdv_st)
            {
                this.WriteYodaRg("GEPhy", "Fd100Adv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "Fd100Adv", 0);
            }
        }

        /// <summary>
        /// Configure 1000BASE-T Half-Duplex advertisement
        /// </summary>
        /// <param name="spd1000HdAdv_st">Parameter Description</param>
        private void Speed1000HdAdvertisement(bool spd1000HdAdv_st = true)
        {
            if (spd1000HdAdv_st)
            {
                this.WriteYodaRg("GEPhy", "Hd1000Adv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "Hd1000Adv", 0);
            }
        }

        /// <summary>
        /// Configure 1000BASE-T Full-Duplex advertisement
        /// </summary>
        /// <param name="spd1000FdAdv_st">Parameter Description</param>
        private void Speed1000FdAdvertisement(bool spd1000FdAdv_st = true)
        {
            if (spd1000FdAdv_st)
            {
                this.WriteYodaRg("GEPhy", "Fd1000Adv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "Fd1000Adv", 0);
            }
        }

        /// <summary>
        /// Configure EEE 100BASE-TX advertisement
        /// </summary>
        /// <param name="eEE100Adv_st">Parameter Description</param>
        private void EEE100Advertisement(bool eEE100Adv_st = true)
        {
            if (eEE100Adv_st)
            {
                this.Info("    enable EEE 100BASE-TX advertisement");
                this.WriteYodaRg("GEPhy", "Eee100Adv", 1);
            }
            else
            {
                this.Info("    disable EEE 100BASE-TX advertisement");
                this.WriteYodaRg("GEPhy", "Eee100Adv", 0);
            }
        }

        /// <summary>
        /// Advertise EEE 1000BASE-T.
        /// </summary>
        /// <param name="eEE1000Adv_st">Parameter Description</param>
        private void EEE1000Advertisement(bool eEE1000Adv_st = true)
        {
            if (eEE1000Adv_st)
            {
                this.WriteYodaRg("GEPhy", "Eee1000Adv", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "Eee1000Adv", 0);
            }
        }

        /// <summary>
        /// Refresh locally advertised speeds
        /// </summary>
        private void RefreshLocallyAdvertisedSpeeds()
        {
            uint value;
            List<EthernetSpeeds> speeds = new List<EthernetSpeeds>();

            /* Create a list of the locally advertised speeds */
            speeds.Clear();
            foreach (var localSpLut in this.localAdvSpeedBitLookUp)
            {
                try
                {
                    value = this.ReadYodaRg("GEPhy", localSpLut.Name);
                }
                catch (ArgumentException e)
                {
                    /* This register does not exist in this device */
                    value = 0;
                }

                if (value != 0)
                {
                    speeds.Add(localSpLut.Value);
                }

                switch (localSpLut.Value)
                {
                    case EthernetSpeeds.SPEED_10BASE_T_HD:
                        this.deviceSettingsUp.Negotiate.Advertise10HD = value == 1;
                        break;
                    case EthernetSpeeds.SPEED_10BASE_T_1L://dani 20april:
                        this.deviceSettingsUp.Negotiate.Advertise10FD = value == 1;
                        break;
                    case EthernetSpeeds.SPEED_100BASE_TX_HD:
                        this.deviceSettingsUp.Negotiate.Advertise100HD = value == 1;
                        break;
                    case EthernetSpeeds.SPEED_100BASE_TX_FD:
                        this.deviceSettingsUp.Negotiate.Advertise100FD = value == 1;
                        break;
                    case EthernetSpeeds.SPEED_100BASE_TX_EEE:
                        this.deviceSettingsUp.Negotiate.AdvertiseEEE100 = value == 1;
                        break;
                    case EthernetSpeeds.SPEED_1000BASE_T_HD:
                        this.deviceSettingsUp.Negotiate.Advertise1000HD = value == 1;
                        break;
                    case EthernetSpeeds.SPEED_1000BASE_T_FD:
                        this.deviceSettingsUp.Negotiate.Advertise1000FD = value == 1;
                        break;
                    case EthernetSpeeds.SPEED_1000BASE_T_EEE:
                        this.deviceSettingsUp.Negotiate.AdvertiseEEE1000 = value == 1;
                        break;
                    default:
                        break;
                }
            }

            this.deviceSettingsUp.Negotiate.LocalAdvSpeeds = speeds;
        }

        // Enumeration?

        // Changed Func

        /// <summary>
        /// Refresh locally advertised speeds
        /// </summary>
        private void RefreshRemoteAdvertisedSpeeds()
        {
            List<EthernetSpeeds> speeds = new List<EthernetSpeeds>();

            /* Create a list of the Remotely advertised speeds */
            foreach (var remSpLut in this.remoteAdvSpeedBitLookUp)
            {
                if (this.ReadYodaRg("GEPhy", remSpLut.Name) != 0)
                {
                    speeds.Add(remSpLut.Value);
                }
            }

            this.deviceSettingsUp.Negotiate.RemoteAdvSpeeds = speeds;
        }

        /// <summary>
        /// Refresh forced speeds
        /// </summary>
        private void RefreshForcedSpeeds()
        {
            uint forced_speed_row = 2 * this.ReadYodaRg("GEPhy", "SpeedSelMsb");
            forced_speed_row += this.ReadYodaRg("GEPhy", "SpeedSelLsb");
            bool fullDuplex = this.ReadYodaRg("GEPhy", "DplxMode") == 1;

            EthernetSpeeds localForcedSpeed = EthernetSpeeds.SPEED_10BASE_T_1L;//dani 20aprilSPEED_10BASE_T_FD;

            switch (forced_speed_row)
            {
                case 0:
                    /* 10-BASE T */
                    if (fullDuplex)
                    {
                        localForcedSpeed = EthernetSpeeds.SPEED_10BASE_T_1L;//dani 20april;
                    }
                    else
                    {
                        localForcedSpeed = EthernetSpeeds.SPEED_10BASE_T_HD;
                    }

                    break;
                case 1:
                    /* 100-BASE T */
                    if (fullDuplex)
                    {
                        localForcedSpeed = EthernetSpeeds.SPEED_100BASE_TX_FD;
                    }
                    else
                    {
                        localForcedSpeed = EthernetSpeeds.SPEED_100BASE_TX_FD;
                    }

                    break;
                case 2:
                    if (fullDuplex)
                    {
                        localForcedSpeed = EthernetSpeeds.SPEED_1000BASE_T_FD;
                    }
                    else
                    {
                        localForcedSpeed = EthernetSpeeds.SPEED_1000BASE_T_HD;
                    }

                    break;
                case 3:
                    // this.Error("Forced speed selection is invalid.");
                    break;
            }

            this.deviceSettingsUp.Fixed.ForcedSpeed = localForcedSpeed;
        }

        /// <summary>
        /// We have a number of different models that we might be connected to
        /// </summary>
        private void RefreshConnectedDevice()//dani
        {
            uint modelNum;

            if (this.TenSPEDevice())
            {
                modelNum = this.ReadYodaRg("IndirectAccessAddressMap", "MMD1_MODEL_NUM");
            }
            else
            {
                modelNum = this.ReadYodaRg("GEPhy", "ModelNum");
            }

            DeviceType deviceType = DeviceType.ADIN1100;

            switch (modelNum)
            {
                case 0x2:
                    deviceType = DeviceType.ADIN1200;
                    this.one1GCapabable = false;
                    break;
                case 0x8: // ADIN1100/ADIN1101
                case 0x9: // ADIN1110/ADIN1111
                case 0xA: // ADIN2111
                    deviceType = DeviceType.ADIN1100;
                    this.one1GCapabable = false;
                    break;
                default:
                    {
                        this.ScanMDIOHwAddress();
                        modelNum = this.ReadYodaRg("IndirectAccessAddressMap", "MMD1_MODEL_NUM");
                        //uint gePkg = this.ReadYodaRg("GESubSys", "GePkg");
                        //if (gePkg == 3)
                        //{
                        //    deviceType = DeviceType.ADIN1300;
                        //    this.one1GCapabable = true;
                        //}
                        //else
                        //{
                        //    deviceType = DeviceType.ADIN1301;
                        //    this.one1GCapabable = true;
                        //}
                    }

                    break;
            }

            TargetInfoItem connectedDevice = new TargetInfoItem(this.deviceSettingsUp.DetectedDevice.ItemName);
            connectedDevice.IsAvailable = true;
            connectedDevice.ItemContent = deviceType.ToString() + "   \n" + "PHY Addr:" + this.deviceConnection.GetMDIOAddress().ToString();

            this.deviceSettingsUp.DetectedDevice = connectedDevice;

            if (this.deviceSettingsUp.ConnectedDeviceType != deviceType)
            {
                this.UpdatefromRegisterJSON(deviceType);
                this.deviceSettingsUp.ConnectedDeviceType = deviceType;
            }
        }

        /// <summary>
        /// Get information about the specified register or bit
        /// </summary>
        /// <param name="mMap">Name of memory map that the register is in</param>
        /// <param name="name">Register or Bifield name to access in the memory map</param>
        /// <returns>Access definition</returns>
        private RegisterInfo LookUpAccessDefinition(string mMap, string name)
        {
            foreach (RegisterDetails registerDetail in this.jsonParser.RegisterFieldMapping.Registers)
            {
                if (registerDetail.MMap != mMap)
                {
                    continue;
                }

                if (registerDetail.Name == name)
                {
                    /* Name matches with a full register */
                    registerDetail.IncludeInDump = true;
                    return new RegisterInfo(registerDetail);
                }

                foreach (FieldDetails fieldDetail in registerDetail.Fields)
                {
                    if (fieldDetail.Name == name)
                    {
                        registerDetail.IncludeInDump = true;
                        fieldDetail.IncludeInDump = true;
                        /* Name matches with a register field */
                        return new RegisterInfo(registerDetail, fieldDetail);
                    }
                }
            }

            if (this.TenSPEDevice()) //dani frame
            {
                // There are a few registers that we don't want to expose in the JSON file
                // Brian Murray says these frame generator ones will be added eventually
                RegisterDetails registerDetailCRSM_FRM_GEN_DIAG_CLK_EN = new RegisterDetails();
                registerDetailCRSM_FRM_GEN_DIAG_CLK_EN.Address = 0x1e882c;
                registerDetailCRSM_FRM_GEN_DIAG_CLK_EN.Name = "CRSM_FRM_GEN_DIAG_CLK_EN";
                FieldDetails fieldDetailCRSM_FRM_GEN_DIAG_CLK_EN = new FieldDetails();
                fieldDetailCRSM_FRM_GEN_DIAG_CLK_EN.Start = 1;
                fieldDetailCRSM_FRM_GEN_DIAG_CLK_EN.Width = 1;

                switch (name)
                {
                    case "CRSM_FRM_GEN_DIAG_CLK_EN":
                        return new RegisterInfo(registerDetailCRSM_FRM_GEN_DIAG_CLK_EN, fieldDetailCRSM_FRM_GEN_DIAG_CLK_EN);
                    default:
                        throw new ArgumentException(string.Format("Information on register or field \"{0:s}\" is not available", name), name);
                }
            }
            else
            {
                // There are a few registers that we don't want to expose in the JSON file
                RegisterDetails registerDetailFgNoFcsNoHdr = new RegisterDetails();
                FieldDetails fieldDetailFgNoFcs = new FieldDetails();
                FieldDetails fieldDetailFgNoHdr = new FieldDetails();

                registerDetailFgNoFcsNoHdr.Address = 2003993;
                registerDetailFgNoFcsNoHdr.Name = "FgNoFcsNoHdr";

                fieldDetailFgNoFcs.Start = 0;
                fieldDetailFgNoFcs.Width = 1;

                fieldDetailFgNoHdr.Start = 1;
                fieldDetailFgNoHdr.Width = 1;

                RegisterDetails registerDetailLnkWdEn = new RegisterDetails();
                registerDetailLnkWdEn.Address = 2004992;
                registerDetailLnkWdEn.Name = "LnkWdEn";
                FieldDetails fieldDetailLnkWdEn = new FieldDetails();
                fieldDetailLnkWdEn.Start = 0;
                fieldDetailLnkWdEn.Width = 1;

                RegisterDetails registerDetailArbWdEn = new RegisterDetails();
                registerDetailArbWdEn.Address = 1998852;
                registerDetailArbWdEn.Name = "ArbWdEn";
                FieldDetails fieldDetailArbWdEn = new FieldDetails();
                fieldDetailArbWdEn.Start = 0;
                fieldDetailArbWdEn.Width = 1;

                RegisterDetails registerDetailB10LpTxEn = new RegisterDetails();
                registerDetailB10LpTxEn.Address = 2011660;
                registerDetailB10LpTxEn.Name = "B10LpTxEn";
                FieldDetails fieldDetailB10LpTxEn = new FieldDetails();
                fieldDetailB10LpTxEn.Start = 0;
                fieldDetailB10LpTxEn.Width = 1;

                RegisterDetails registerDetailGePhyIfCfg = new RegisterDetails();
                registerDetailGePhyIfCfg.Address = 2031399;
                registerDetailGePhyIfCfg.Name = "GePhyIfCfg";
                FieldDetails fieldDetailGeFifoDpth = new FieldDetails();
                fieldDetailGeFifoDpth.Start = 2;
                fieldDetailGeFifoDpth.Width = 3;

                RegisterDetails registerDetailDpthMiiByte = new RegisterDetails();
                registerDetailDpthMiiByte.Address = 2004482;
                registerDetailDpthMiiByte.Name = "DpthMiiByte";
                FieldDetails fieldDetailDpthMiiByte = new FieldDetails();
                fieldDetailDpthMiiByte.Start = 0;
                fieldDetailDpthMiiByte.Width = 1;

                RegisterDetails registerDetailGePkg = new RegisterDetails();
                registerDetailGePkg.Address = 2031362;
                registerDetailGePkg.Name = "GePkg";
                FieldDetails fieldDetailGePkg = new FieldDetails();
                fieldDetailGePkg.Start = 0;
                fieldDetailGePkg.Width = 4;

                RegisterDetails registerDetailGePhyBaseCfg = new RegisterDetails();
                registerDetailGePhyBaseCfg.Address = 2031380;
                registerDetailGePhyBaseCfg.Name = "GePhyBaseCfg";
                FieldDetails fieldDetailGePhySftPdCfg = new FieldDetails();
                fieldDetailGePhySftPdCfg.Start = 3;
                fieldDetailGePhySftPdCfg.Width = 1;

                RegisterDetails registerDetailGePhyRst = new RegisterDetails();
                registerDetailGePhyRst.Address = 2031373;
                registerDetailGePhyRst.Name = "GePhyRst";
                FieldDetails fieldDetailGePhyRst = new FieldDetails();
                fieldDetailGePhyRst.Start = 0;
                fieldDetailGePhyRst.Width = 1;

                //RegisterDetails registerDetail = new RegisterDetails();
                //.Address = ;
                //.Name = "";
                //FieldDetails fieldDetail = new FieldDetails();
                //.Start = ;
                //.Width = ;

                switch (name)
                {
                    case "GePhyRst":
                        return new RegisterInfo(registerDetailGePhyRst, fieldDetailGePhyRst);
                    case "GePhySftPdCfg":
                        return new RegisterInfo(registerDetailGePhyBaseCfg, fieldDetailGePhySftPdCfg);
                    case "GePkg":
                        return new RegisterInfo(registerDetailGePkg, fieldDetailGePkg);
                    case "DpthMiiByte":
                        return new RegisterInfo(registerDetailDpthMiiByte, fieldDetailDpthMiiByte);
                    case "FgNoHdr":
                        return new RegisterInfo(registerDetailFgNoFcsNoHdr, fieldDetailFgNoHdr);
                    case "FgNoFcs":
                        return new RegisterInfo(registerDetailFgNoFcsNoHdr, fieldDetailFgNoFcs);
                    case "LnkWdEn":
                        return new RegisterInfo(registerDetailLnkWdEn, fieldDetailLnkWdEn);
                    case "ArbWdEn":
                        return new RegisterInfo(registerDetailArbWdEn, fieldDetailArbWdEn);
                    case "B10LpTxEn":
                        return new RegisterInfo(registerDetailB10LpTxEn, fieldDetailB10LpTxEn);
                    case "GeFifoDpth":
                        return new RegisterInfo(registerDetailGePhyIfCfg, fieldDetailGeFifoDpth);
                    default:
                        throw new ArgumentException(string.Format("Information on register or field \"{0:s}\" is not available", name), name);
                }
            }
        }

        /// <summary>
        /// Poll with a timeout for a register or bitfield to contain a particular value.
        /// </summary>
        /// <param name="mMap">Name of memory map that the register is in</param>
        /// <param name="name">Register or Bifield name to access in the memory map</param>
        /// <param name="expected">Expected value</param>
        /// <param name="timeout">Timeout value</param>
        private void PollEqYodaRg(string mMap, string name, uint expected, double timeout)
        {
            uint regContent;
            RegisterInfo registerInfo = this.LookUpAccessDefinition(mMap, name);
            long timeout_ms = (long)(timeout * 1000);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            do
            {
                regContent = this.deviceConnection.ReadMDIORegister(registerInfo.Address);
                if (sw.ElapsedMilliseconds > timeout_ms)
                {
                    throw new TimeoutException(string.Format("Gave up waiting for \"{0}\" to contain \"{1}\" within {2} seconds.", name, expected, timeout));
                }
            }
            while (registerInfo.ExtractFieldValue(regContent) != expected);

            return;
        }

        /// <summary>
        /// Check if Register or Bifield name in the memory map contains the specified value
        /// </summary>
        /// <param name="mMap">Name of memory map that the register is in</param>
        /// <param name="name">Register or Bifield name to access in the memory map</param>
        /// <param name="expected">Expected value</param>
        private void ReadCheckYodaRg(string mMap, string name, uint expected)
        {
            RegisterInfo registerInfo = this.LookUpAccessDefinition(mMap, name);

            Debug.Assert(expected != registerInfo.ExtractFieldValue(this.deviceConnection.ReadMDIORegister(registerInfo.Address)), "Value is not what was expeted.");
        }

        /// <summary>
        /// Check if Register name in the memory map contains the specified value
        /// </summary>
        /// <param name="address">Register name to access in the memory map</param>
        /// <param name="expected">Expected value</param>
        /// <returns></returns>
        private uint ReadCheckYodaRg(uint address)
        {
            return this.deviceConnection.ReadMDIORegister(address);
        }

        /// <summary>
        /// Write value to register or bitfield
        /// </summary>
        /// <param name="mMap">Name of memory map that the register is in</param>
        /// <param name="name">Register or Bifield name to access in the memory map</param>
        /// <param name="value">Value to write to register or bitfield</param>
        private void WriteYodaRg(string mMap, string name, uint value)
        {
            uint regContent = value;
            RegisterInfo registerInfo = this.LookUpAccessDefinition(mMap, name);

            if (registerInfo.IsSubField)
            {
                this.VerboseInfo(string.Format("BitField \"{1:s}\" = {2:d}", mMap, name, value));
                regContent = this.deviceConnection.ReadMDIORegister(registerInfo.Address);
                regContent = registerInfo.InsertFieldValue(regContent, value);
            }
            else
            {
                this.VerboseInfo(string.Format("Register \"{1:s}\" = {2:d}", mMap, name, value));
            }

            this.deviceConnection.WriteMDIORegister(registerInfo.Address, regContent);
        }

        /// <summary>
        /// Write value to register
        /// </summary>
        /// <param name="address">Register address</param>
        /// <param name="value">Value to write to register</param>
        private void WriteYodaRg(uint address, uint value)
        {
            this.deviceConnection.WriteMDIORegister(address, value);
        }

        /// <summary>
        /// Some information
        /// </summary>
        /// <param name="seconds">Parameter Description 1</param>
        private void Sleep(double seconds)
        {
            Thread.Sleep((int)(seconds * 1000));
        }

        /// <summary>
        /// Write a value
        /// </summary>
        /// <param name="mMap">Memory map of element to write to</param>
        /// <param name="name">Name of element to write to</param>
        /// <param name="value">Value of element</param>
        public void WriteValue(string mMap, string name, uint value)
        {
            this.WriteYodaRg(mMap, name, value);
            this.Info($"{mMap} -> {name}:{value}");
        }

        /// <summary>
        /// Software reset of GE PHY, without applying settings for UNH-IOL testing.
        /// </summary>
        private void SftRstGEPhy()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  GEPhy exits software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
        }

        /// <summary>
        /// Put PHY in Software Power Down.
        /// </summary>
        /// <param name="swPd_st">Power Down Enable</param>
        public void SoftwarePdEnable(bool swPd_st = true)
        {
            if (swPd_st)
            {
                if (this.TenSPEDevice())
                {
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
                }
                else
                {
                    this.WriteYodaRg("GEPhy", "SftPd", 1);
                }

                this.Info("    put PHY in Software Power Down");
            }
            else
            {
                if (this.TenSPEDevice())
                {
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
                }
                else
                {
                    this.WriteYodaRg("GEPhy", "SftPd", 0);
                }

                this.Info("    PHY NOT in Software Power Down");
            }
        }

        /// <summary>
        /// Put PHY in Software Power Down.
        /// </summary>
        /// <param name="resettype">Power Down Enable</param>
        public void SoftwareReset(string resettype)
        {
            if (this.TenSPEDevice())
            {
                switch (resettype)
                {
                    case "Reset: SubSys (Pin)":
                        this.Error("TODO_10SPE : What registers are needed to do this reset : " + resettype);
                        break;
                    case "Reset: SubSys":
                        this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_PHY_SUBSYS_RST", 1);
                        break;
                    case "Reset: PHY":
                        this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_RST", 1);
                        //Fixed this.Error("TODO_10SPE : What registers are needed to do this reset : " + resettype);
                        break;
                }
            }
            else
            {
                switch (resettype)
                {
                    case "Reset: SubSys (Pin)":
                        this.WriteYodaRg("GESubsys", "GeSftRstCfgEn", 1);
                        this.WriteYodaRg("GESubsys", "GeSftRst", 1);
                        break;
                    case "Reset: SubSys":
                        /* The sub-system can be reset by setting GeSftRst (sub - system GeSftRst
                         * register 0xFF0C bit 0) to ‘1’. This bit is self - clearing. */
                        this.WriteYodaRg("GESubsys", "GeSftRstCfgEn", 0);
                        this.WriteYodaRg("GESubsys", "GeSftRst", 1);
                        break;
                    case "Reset: PHY":
                        /* The PHY core registers can be reset by setting SftRst
                            (PHY core MiiControl register 0x0000 bit 15) to ‘1’.
                            This bit is self - clearing.*/
                        this.WriteYodaRg("GEPhy", "SftRst", 1);
                        break;
                }
            }

            this.deviceSettingsUp.FlagAllPropertiesChanged();
        }

        /// <summary>
        /// Enable / Disable Auto-Negotiation.
        /// </summary>
        /// <param name="anEn_st">Parameter Description</param>
        public void ANegEnable(bool anEn_st = true)
        {
            if (anEn_st)
            {
                this.WriteYodaRg("GEPhy", "AutonegEn", 1);
                this.Info("    enable Auto-Negotiation");
            }
            else
            {
                this.WriteYodaRg("GEPhy", "AutonegEn", 0);
                this.Info("    disable Auto-Negotiation");
            }
        }

        /// <summary>
        /// Kick off cable diagnostics
        /// </summary>
        public void RunCableDiagnostics(bool enablecrosspairfaultchecking)
        {
            this.cablediagnosticsRunning = true;
            if (enablecrosspairfaultchecking)
            {
                this.Info("Cross Pair Checking enabled.");
                this.WriteYodaRg("GEPhy", "CdiagXpairDis", 0);
            }
            else
            {
                this.Info("Cross Pair Checking disabled.");
                this.WriteYodaRg("GEPhy", "CdiagXpairDis", 1);
            }

            this.WriteYodaRg("GEPhy", "CdiagRun", 1);
            this.Info("Running automated cable diagnostics");
        }

        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        /// <summary>
        /// Report cable diagnostics
        /// </summary>
        private void CableDiagnosticsStatus()
        {
            uint cdi_st = this.ReadYodaRg("GEPhy", "CdiagRun");
            if (this.cablediagnosticsRunning && (cdi_st == 0))
            {
                this.cablediagnosticsRunning = false;
                this.Info("Cable Diagnostics have completed");

                var diagInfoRegisters = new List<string>() { "CdiagDtldRslts0", "CdiagDtldRslts1", "CdiagDtldRslts2", "CdiagDtldRslts3" };
                var pairs = new List<string>() { "0", "1", "2", "3" };

                List<string> cableDiagnosticsStatus = new List<string>();

                foreach (var registerDetail in this.Registers)
                {
                    if (diagInfoRegisters.Contains(registerDetail.Name))
                    {
                        uint regContent = this.ReadYodaRg("GEPhy", registerDetail.Name);

                        foreach (var fieldDetail in registerDetail.Fields)
                        {
                            var registerInfo = new RegisterInfo(registerDetail, fieldDetail);
                            if (registerInfo.ExtractFieldValue(regContent) == 0x1)
                            {
                                string text = string.Format("'{0}' is set. {1}", fieldDetail.Name, fieldDetail.Documentation);
                                string searchtext = "When set, this register bit indicates that ";
                                if (text.Contains(searchtext))
                                {
                                    // Just use the text to the right of this as it is shorter
                                    text = this.FirstLetterToUpper(text.Substring(text.IndexOf(searchtext) + searchtext.Length));
                                }

                                cableDiagnosticsStatus.Add(text);
                            }
                        }
                    }
                }

                uint distance;
                foreach (var pair in pairs)
                {

                    try
                    {
                        distance = this.ReadYodaRg("GEPhy", string.Format("CdiagFltDist{0}", pair));
                        if (distance != 0xFF)
                        {
                            cableDiagnosticsStatus.Add(string.Format("Distance to fault on pair {0} is {1} m.", pair, distance));
                        }
                    }
                    catch (ArgumentException e)
                    {
                        /* This register does not exist in this device */
                    }
                }

                foreach (var item in cableDiagnosticsStatus)
                {
                    this.Info(item);
                }

                this.deviceSettingsUp.CableDiagnosticsStatus = cableDiagnosticsStatus;
            }
        }

        /// <summary>
        /// Enable / Disable Linking.
        /// </summary>
        /// <param name="linkEn_st">Parameter Description</param>
        public void EnableLinking(bool linkEn_st = true)
        {
            if (this.TenSPEDevice())
            {
                this.Error("TODO_10SPE : What register like \"LinkEn\" is needed to enable or disable linking?");
            }
            else
            {
                if (linkEn_st)
                {
                    this.WriteYodaRg("GEPhy", "LinkEn", 1);
                    this.Info("    enable Linking");
                }
                else
                {
                    this.WriteYodaRg("GEPhy", "LinkEn", 0);
                    this.Info("    disable Linking");
                }
            }

        }

        /// <summary>
        /// Restart Auto Negotiation.
        /// </summary>
        public void RestartANeg()
        {
            if (this.TenSPEDevice())
            {
                this.WriteYodaRg("IndirectAccessAddressMap", "AN_RESTART", 1);
            }
            else
            {
                this.WriteYodaRg("GEPhy", "RestartAneg", 1);
            }

            this.Info("    restart auto negotiation");
        }

        /// <summary>
        /// Refresh variables which need to be wiped when not linked
        /// </summary>
        private void RefreshNonLinkedStatus()
        {
            if (this.deviceSettingsUp.Link.CableLength.IsAvailable != false)
            {
                TargetInfoItem cableLength = new TargetInfoItem(this.deviceSettingsUp.Link.CableLength.ItemName);
                cableLength.IsAvailable = false;
                cableLength.ItemContent = this.deviceSettingsUp.Link.CableLength.ItemContent;
                this.deviceSettingsUp.Link.CableLength = cableLength;
            }

            if (this.deviceSettingsUp.Link.PairMeanSquareError.IsAvailable != false)
            {
                TargetInfoItem pairMeanSquareError = new TargetInfoItem(this.deviceSettingsUp.Link.PairMeanSquareError.ItemName);
                pairMeanSquareError.IsAvailable = false;
                pairMeanSquareError.ItemContent = this.deviceSettingsUp.Link.PairMeanSquareError.ItemContent;
                this.deviceSettingsUp.Link.PairMeanSquareError = pairMeanSquareError;
            }

            if (this.deviceSettingsUp.Link.PairMeanSquareErrorStats.IsAvailable != false)
            {
                TargetInfoItem pairMeanSquareErrorStats = new TargetInfoItem(this.deviceSettingsUp.Link.PairMeanSquareErrorStats.ItemName);
                pairMeanSquareErrorStats.IsAvailable = false;
                pairMeanSquareErrorStats.ItemContent = this.deviceSettingsUp.Link.PairMeanSquareErrorStats.ItemContent;
                this.deviceSettingsUp.Link.PairMeanSquareErrorStats = pairMeanSquareErrorStats;
            }
        }

        /// <summary>
        /// Refresh variables which need to be wiped when not linked
        /// </summary>
        private void RefreshTenSPEStatusItem()
        {
            TargetInfoItem masterSlaveStatus = new TargetInfoItem(this.deviceSettingsUp.Link.MasterSlaveStatus.ItemName);
            TargetInfoItem cableVoltage = new TargetInfoItem(this.deviceSettingsUp.Link.CableVoltage.ItemName);
            TargetInfoItem anStatus = new TargetInfoItem(this.deviceSettingsUp.Link.AnStatus.ItemName);

            masterSlaveStatus.IsAvailable = this.TenSPEDevice() && this.deviceSettingsUp.PhyState == EthPhyState.LinkUp;
            cableVoltage.IsAvailable = this.TenSPEDevice() && this.deviceSettingsUp.PhyState == EthPhyState.LinkUp;
            anStatus.IsAvailable = this.TenSPEDevice();// && this.deviceSettingsUp.PhyState == EthPhyState.LinkUp; // this is the condition for the visibility in the UI

            if (this.TenSPEDevice())
            {
                // AnMsConfigRsltn tells whether I resolved as a master or slave
                // AN_MS_CONFIG_RSLTN
                // MASTER/SLAVE resolution result, determined as per Table 98-4 - MASTER-SLAVE Configuration of IEEE Std 802.3.
                // This is encoded as follows:
                // 2'd0: Not run
                // 2'd1: Configuration fault
                // 2'd2: Success, PHY is configured as SLAVE
                // 2'd3: Success, PHY is configured as MASTER

                switch (this.ReadYodaRg("IndirectAccessAddressMap", "AN_MS_CONFIG_RSLTN"))
                {
                    //  default:
                    case 0x0:
                        //anStatus.ItemContent = "Not run";
                        break;
                    case 0x1:
                        anStatus.ItemContent = "Configuration fault";
                        break;
                    case 0x2:
                        masterSlaveStatus.ItemContent = "Slave";

                        //dani 20Ap
                        //if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_EN") == 1)
                        //{
                        //    masterSlaveStatus.ItemContent += " (Negotiated)";
                        //}

                        break;
                    case 0x3:
                        masterSlaveStatus.ItemContent = "Master";
                        // anStatus.ItemContent = "AN GOOD";
                        //dani 20Apr
                        //if (this.ReadYodaRg("IndirectAccessAddressMap", "AN_EN") == 1)
                        //{
                        //    masterSlaveStatus.ItemContent += " (Negotiated)";
                        //}

                        break;
                }

                uint forcedMasterSlave = this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_FORCE_MS");
                if (forcedMasterSlave == 1)
                {
                    anStatus.ItemContent = "Disabled";
                }
                else
                {
                    uint ancompleted = this.ReadYodaRg("IndirectAccessAddressMap", "AN_COMPLETE");
                    if (ancompleted == 1)
                    {
                        anStatus.ItemContent = "Completed";
                    }
                }

                uint hi_req = this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_B10L_TX_LVL_HI_REQ");
                uint hi_abl = this.ReadYodaRg("IndirectAccessAddressMap", "AN_ADV_B10L_TX_LVL_HI_ABL");

                uint lp_hi_req = this.ReadYodaRg("IndirectAccessAddressMap", "AN_LP_ADV_B10L_TX_LVL_HI_REQ");
                uint lp_hi_abl = this.ReadYodaRg("IndirectAccessAddressMap", "AN_LP_ADV_B10L_TX_LVL_HI_ABL");

                if ((hi_abl != 1) || (lp_hi_abl != 1))
                {
                    /* One or both sides cannot do HI, therfore must be low*/
                    cableVoltage.ItemContent = "1.0 Vpk-pk";
                }
                else
                    if ((hi_req == 0) && (lp_hi_req == 0x0))
                {
                    // Both can manage HI, but neither are requesting it
                    cableVoltage.ItemContent = "1.0 Vpk-pk";
                }
                else
                {
                    // Both can manage HI, and one or both are requesting it

                    cableVoltage.ItemContent = "2.4 Vpk-pk";
                }
            }

            this.deviceSettingsUp.Link.CableVoltage = cableVoltage;
            this.deviceSettingsUp.Link.MasterSlaveStatus = masterSlaveStatus;
            this.deviceSettingsUp.Link.AnStatus = anStatus;
        }

        /// <summary>
        /// Refresh linked status
        /// </summary>
        private void RefreshLinkedStatus()
        {
            uint mseA_st = 0x0;
            uint mseB_st = 0x0;
            uint mseC_st = 0x0;
            uint mseD_st = 0x0;

            mseA_st = this.ReadYodaRg("GEPhy", "MseA");
            this.deviceSettingsUp.Link.ResolvedHCD = (EthernetSpeeds)this.ReadYodaRg("GEPhy", "HcdTech");

            if ((this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_HD) || (this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_FD))
            {
                mseB_st = this.ReadYodaRg("GEPhy", "MseB");
                mseC_st = this.ReadYodaRg("GEPhy", "MseC");
                mseD_st = this.ReadYodaRg("GEPhy", "MseD");
            }

            uint dSP_len = this.ReadYodaRg("GEPhy", "CdiagCblLenEst");
            uint msRslvd_st = 0;
            uint msFlt_st = 0;
            if (this.one1GCapabable)
            {
                msRslvd_st = this.ReadYodaRg("GEPhy", "MstrRslvd");
                msFlt_st = this.ReadYodaRg("GEPhy", "MstrSlvFlt");
            }

            uint freqOffs_st = /* this.ReadYodaRg("GEPhy", "FreqOffsTrunc") */0;

            /* Calculate the frequency offset */
            if (freqOffs_st >= 32768)
            {
                freqOffs_st -= 65536;
            }

            this.deviceSettingsUp.Link.FreqOffsetPpm = (1000000.0 * Math.Pow(2, -25)) * freqOffs_st;

            this.deviceSettingsUp.Link.LocalRcvrOk = this.ReadYodaRg("GEPhy", "LocRcvrStatus") == 1;
            this.deviceSettingsUp.Link.RemoteRcvrOk = this.ReadYodaRg("GEPhy", "RemRcvrStatus") == 1;

            TargetInfoItem cableLength = new TargetInfoItem(this.deviceSettingsUp.Link.CableLength.ItemName);

            cableLength.IsAvailable = (dSP_len != 0xFF) && this.deviceSettingsUp.Link.LinkEstablished && ((this.deviceSettingsUp.Link.ResolvedHCD >= EthernetSpeeds.SPEED_100BASE_TX_HD) && (this.deviceSettingsUp.Link.ResolvedHCD <= EthernetSpeeds.SPEED_1000BASE_T_FD));
            cableLength.ItemContent = string.Format("{0:d} m", dSP_len);
            this.deviceSettingsUp.Link.CableLength = cableLength;

            if (this.mseA_Max < mseA_st)
            {
                this.mseA_Max = mseA_st;
            }

            if (this.mseB_Max < mseB_st)
            {
                this.mseB_Max = mseB_st;
            }

            if (this.mseC_Max < mseC_st)
            {
                this.mseC_Max = mseC_st;
            }

            if (this.mseD_Max < mseD_st)
            {
                this.mseD_Max = mseD_st;
            }

            TargetInfoItem pairMeanSquareError = new TargetInfoItem(this.deviceSettingsUp.Link.PairMeanSquareError.ItemName);
            pairMeanSquareError.IsAvailable = this.deviceSettingsUp.Link.LinkEstablished;
            if ((this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_HD) || (this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_FD))
            {
                pairMeanSquareError.ItemContent = string.Format(" {0:d}, {1:d}, {2:d}, {3:d}", mseA_st, mseB_st, mseC_st, mseD_st);
            }
            else
            {
                pairMeanSquareError.ItemContent = string.Format(" {0:d}", mseA_st);
            }

            this.deviceSettingsUp.Link.PairMeanSquareError = pairMeanSquareError;

            TargetInfoItem pairMeanSquareErrorStats = new TargetInfoItem(this.deviceSettingsUp.Link.PairMeanSquareErrorStats.ItemName);
            pairMeanSquareErrorStats.IsAvailable = this.deviceSettingsUp.Link.LinkEstablished;
            if ((this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_HD) || (this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_FD))
            {
                pairMeanSquareErrorStats.ItemContent = string.Format(" {0:d}, {1:d}, {2:d}, {3:d} ({4:d}, {5:d}, {6:d}, {7:d})", mseA_st, mseB_st, mseC_st, mseD_st, this.mseA_Max, this.mseB_Max, this.mseC_Max, this.mseD_Max);
            }
            else
            {
                pairMeanSquareErrorStats.ItemContent = string.Format(" {0:d} ({1:d})", mseA_st, this.mseA_Max);
            }

            this.deviceSettingsUp.Link.PairMeanSquareErrorStats = pairMeanSquareErrorStats;

#if false
             uint parFlt_st = this.ReadYodaRg("GEPhy", "ParDetFlt");
            uint pair01_st = this.ReadYodaRg("GEPhy", "Pair01Swap");
            uint pair23_st = this.ReadYodaRg("GEPhy", "Pair23Swap");
            uint b10PolInv_st = this.ReadYodaRg("GEPhy", "B10PolInv");

            uint pair0Pol_st = this.ReadYodaRg("GEPhy", "Pair0PolInv");
            uint pair1Pol_st = this.ReadYodaRg("GEPhy", "Pair1PolInv");
            uint pair2Pol_st = this.ReadYodaRg("GEPhy", "Pair2PolInv");
            uint pair3Pol_st = this.ReadYodaRg("GEPhy", "Pair3PolInv");

            uint txEn_st = this.ReadYodaRg("GEPhy", "TxEnStat");
            uint rxDv_st = this.ReadYodaRg("GEPhy", "RxDvStat");




            uint pgaLvlA_st = this.ReadYodaRg("GEPhy", "PgaLvlA");
            uint pgaLvlB_st = this.ReadYodaRg("GEPhy", "PgaLvlB");
            uint pgaLvlC_st = this.ReadYodaRg("GEPhy", "PgaLvlC");
            uint pgaLvlD_st = this.ReadYodaRg("GEPhy", "PgaLvlD");

            uint nrga_st = this.ReadYodaRg("GEPhy", "Nrga");
            uint nrgb_st = this.ReadYodaRg("GEPhy", "Nrgb");
            uint nrgc_st = this.ReadYodaRg("GEPhy", "Nrgc");
            uint nrgd_st = this.ReadYodaRg("GEPhy", "Nrgd");
            string msRslvd_name;
            if ((this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_HD) || (this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_FD))
            {
                if (msFlt_st != 0)
                {
                    msRslvd_name = "Master/Slave Fault";
                }
                else
                {
                    if (msRslvd_st != 0)
                    {
                        msRslvd_name = "Master";
                    }
                    else
                    {
                        msRslvd_name = "Slave";
                    }
                }
            }
            else
            {
                msRslvd_name = string.Empty;
            }



            if (pair01_st != 0)
            {
                // this.VerboseInfo("    Pair 01 Swapped");
            }
            else
            {
                this.Info("    Pair 01 Straight");
            }

            if ((this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_HD) || (this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_FD))
            {
                if (pair23_st != 0)
                {
                    // this.VerboseInfo("    Pair 23 Swapped");
                }
                else
                {
                    // this.VerboseInfo("    Pair 23 Straight");
                }
            }

            if (this.deviceSettingsUp.Negotiate.AutoNegLocalPhyEnabled)
            {

            }
            else
            {

            }

            if ((this.deviceSettingsUp.Negotiate.AutoNegLocalPhyEnabled && ((this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_10BASE_T_HD) || (this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_10BASE_T_FD))) || (!this.deviceSettingsUp.Negotiate.AutoNegLocalPhyEnabled && (this.deviceSettingsUp.Fixed.ForcedSpeed == EthernetSpeeds.SPEED_10BASE_T_HD || this.deviceSettingsUp.Fixed.ForcedSpeed == EthernetSpeeds.SPEED_10BASE_T_FD)))
            {
                if (b10PolInv_st != 0)
                {
                    // this.VerboseInfo("    Pair 0 Inverted");
                }
                else
                {
                    // this.VerboseInfo("    Pair 0 Not Inverted");
                }
            }

            if (this.deviceSettingsUp.Negotiate.AutoNegLocalPhyEnabled && ((this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_HD) || (this.deviceSettingsUp.Link.ResolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_FD)))
            {
                if (pair0Pol_st != 0)
                {
                    // this.VerboseInfo("    Pair 0 Inverted");
                }
                else
                {
                    // this.VerboseInfo("    Pair 0 Not Inverted");
                }

                if (pair1Pol_st != 0)
                {
                    // this.VerboseInfo("    Pair 1 Inverted");
                }
                else
                {
                    // this.VerboseInfo("    Pair 1 Not Inverted");
                }

                if (pair2Pol_st != 0)
                {
                    // this.VerboseInfo("    Pair 2 Inverted");
                }
                else
                {
                    // this.VerboseInfo("    Pair 2 Not Inverted");
                }

                if (pair3Pol_st != 0)
                {
                    // this.VerboseInfo("    Pair 3 Inverted");
                }
                else
                {
                    // this.VerboseInfo("    Pair 3 Not Inverted");
                }
            }


#endif
        }

        /// <summary>
        /// Send data using the Frame Generator
        /// </summary>
        /// <param name="numFrames">Parameter Description 1</param>
        /// <param name="frameLen">Parameter Description 2</param>
        /// <param name="frameType">Parameter Description 3</param>
        /// <param name="continuous">Continous mode enabled</param>
        public void SendData(uint numFrames, uint frameLen, FrameType frameType, bool continuous)
        {
            if (this.TenSPEDevice())
            {
                uint fgEn_st = this.ReadYodaRg("IndirectAccessAddressMap", "FG_EN");

                if (fgEn_st == 1)
                {
                    // Already running, therefore just terminate it and return
                    this.Info(" Frame Generator operation terminated.");
                    this.WriteYodaRg("IndirectAccessAddressMap", "FG_EN", 0);
                }
                else
                {
                    this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_FRM_GEN_DIAG_CLK_EN", 1);
                    if (frameLen <= 0xFFFF)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_FRM_LEN", frameLen);
                        this.Info(string.Format("    Frame Length set to {0:d}", frameLen));
                    }
                    else
                    {
                        this.Error("    Frame Length Not set - value must be less than 65535");
                    }

                    if (continuous)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_CONT_MODE_EN", 0x1);
                        this.Info("Frames will be sent continuously until terminated.");
                    }
                    else
                    {
                        uint numFramesH = numFrames / 65536;
                        uint numFramesL = numFrames - (numFramesH * 65536);
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_NFRM_L", numFramesL);
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_NFRM_H", numFramesH);
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_CONT_MODE_EN", 0x0);

                        this.Info(string.Format("    Num Frames set to {0:d}", numFrames));
                    }

                    if (frameType == FrameType.Random)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_CNTRL", 1);
                        this.Info("    Frame Type configured as random");
                    }
                    else if (frameType == FrameType.All0s)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_CNTRL", 2);
                        this.Info("    Frame Type configured as all zeros");
                    }
                    else if (frameType == FrameType.All1s)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_CNTRL", 3);
                        this.Info("    Frame Type configured as all ones");
                    }
                    else if (frameType == FrameType.Alt10s)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_CNTRL", 4);
                        this.Info("    Frame Type configured as alternating 1 0");
                    }
                    else if (frameType == FrameType.Decrement)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "FG_CNTRL", 5);
                        this.Info("    Frame Type configured as decrementing byte");
                    }
                    else
                    {
                        this.Info("    Frame Type Not Configured - Use one of  Random / A000s / A111s / Alt10 / Decrement");
                    }

                    this.WriteYodaRg("IndirectAccessAddressMap", "FG_EN", 1);
                    this.Info(string.Format(" - Started transmission of {0:d} frames - ", numFrames));
                }
            }
            else
            {
                uint fgEn_st = this.ReadYodaRg("GEPhy", "FgEn");

                if (fgEn_st == 1)
                {
                    // Already running, therefore just terminate it and return
                    this.Info(" Frame Generator operation terminated.");
                    this.WriteYodaRg("GEPhy", "FgEn", 0);
                }
                else
                {
                    this.WriteYodaRg("GEPhy", "DiagClkEn", 1);
                    if (frameLen <= 0xFFFF)
                    {
                        this.WriteYodaRg("GEPhy", "FgFrmLen", frameLen);
                        this.Info(string.Format("    Frame Length set to {0:d}", frameLen));
                    }
                    else
                    {
                        this.Error("    Frame Length Not set - value must be less than 65535");
                    }

                    if (continuous)
                    {
                        this.WriteYodaRg("GEPhy", "FgContModeEn", 0x1);
                        this.Info("Frames will be sent continuously until terminated.");
                    }
                    else
                    {
                        uint numFramesH = numFrames / 65536;
                        uint numFramesL = numFrames - (numFramesH * 65536);
                        this.WriteYodaRg("GEPhy", "FgNfrmL", numFramesL);
                        this.WriteYodaRg("GEPhy", "FgNfrmH", numFramesH);
                        this.WriteYodaRg("GEPhy", "FgContModeEn", 0x0);

                        this.Info(string.Format("    Num Frames set to {0:d}", numFrames));
                    }

                    if (frameType == FrameType.Random)
                    {
                        this.WriteYodaRg("GEPhy", "FgCntrl", 1);
                        this.Info("    Frame Type configured as random");
                    }
                    else if (frameType == FrameType.All0s)
                    {
                        this.WriteYodaRg("GEPhy", "FgCntrl", 2);
                        this.Info("    Frame Type configured as all zeros");
                    }
                    else if (frameType == FrameType.All1s)
                    {
                        this.WriteYodaRg("GEPhy", "FgCntrl", 3);
                        this.Info("    Frame Type configured as all ones");
                    }
                    else if (frameType == FrameType.Alt10s)
                    {
                        this.WriteYodaRg("GEPhy", "FgCntrl", 4);
                        this.Info("    Frame Type configured as alternating 1 0");
                    }
                    else if (frameType == FrameType.Decrement)
                    {
                        this.WriteYodaRg("GEPhy", "FgCntrl", 5);
                        this.Info("    Frame Type configured as decrementing byte");
                    }
                    else
                    {
                        this.Info("    Frame Type Not Configured - Use one of  Random / A000s / A111s / Alt10 / Decrement");
                    }

                    this.ReadYodaRg("GEPhy", "RxErrCnt");
                    this.WriteYodaRg("GEPhy", "FgEn", 1);
                    this.Info(string.Format(" - Started transmission of {0:d} frames - ", numFrames));
                }
            }
        }

        /// <summary>
        /// Configure Frame Checker
        /// </summary>
        /// <param name="fcTxSel_TxRx">Parameter Description 1</param>
        /// <param name="frameLen">Parameter Description 2</param>
        public void FrameCheckerConfig(FrameChecker fcTxSel_TxRx, uint frameLen)
        {
            if (this.TenSPEDevice())
            {
                /* Nothing here needed it seems */
            }
            else
            {
                this.WriteYodaRg("GEPhy", "FcMaxFrmSize", 0xFFFF);
                if (fcTxSel_TxRx == FrameChecker.RxSide)
                {
                    this.Info("    Frame Checker on Rx Side");

                    // Check frames received by the PHY from the remote end
                    this.WriteYodaRg("GEPhy", "FcTxSel", 0);
                }
                else if (fcTxSel_TxRx == FrameChecker.TxSide)
                {
                    this.Info("    Frame Checker on Tx Side");

                    // Check frames from the MAC I/F to be transmitted by the PHY
                    this.WriteYodaRg("GEPhy", "FcTxSel", 1);
                }
                else
                {
                    this.Info("    Frame Checker Not Configured - Use Tx Side / Rx Side");
                }
            }
        }

        /// <summary>
        /// Check Frame Generator Status
        /// </summary>
        private void FrameGeneratorStatus()
        {
            string mmap = "GEPhy";
            string FgEn = "FgEn";
            string FcTxSel = "FcTxSel";
            string FgContModeEn = "FgContModeEn";
            string FgDone = "FgDone";

            if (this.TenSPEDevice())
            {
                mmap = "IndirectAccessAddressMap";
                FgEn = "FG_EN";
                FcTxSel = "FC_TX_SEL";
                FgContModeEn = "FG_CONT_MODE_EN";
                FgDone = "FG_DONE";
            }

            uint fgEn_st = this.ReadYodaRg(mmap, FgEn);
            uint fcTxSel_st = this.ReadYodaRg(mmap, FcTxSel);
            uint fgContModeEn_st = this.ReadYodaRg(mmap, FgContModeEn);

            TargetInfoItem frameGeneratorStatus = new TargetInfoItem(this.deviceSettingsUp.FrameGeneratorStatus.ItemName);
            frameGeneratorStatus.IsAvailable = true;
            TargetInfoItem remLoopbackStatus = new TargetInfoItem(this.deviceSettingsUp.FrameGeneratorStatus.ItemName);
            remLoopbackStatus.IsAvailable = true;

            if (this.localLpbk == true)
            {
                frameGeneratorStatus.ItemContent = "Remote Loopback Enabled";
                this.deviceSettingsUp.Link.FrameGenRunning = false;
            }
            else if (fgEn_st == 0)
            {
                this.deviceSettingsUp.Link.FrameGenRunning = false;
                //dani if (this.localLpbk == true)
                //{
                //    frameGeneratorStatus.ItemContent = "Not Enabled" + "\n" + "RemoteLoopback Mode";
                //}
                //else
                //{
                frameGeneratorStatus.ItemContent = "Not Enabled";
                //}
            }
            else
            {
                if (fgContModeEn_st == 1)
                {
                    frameGeneratorStatus.ItemContent = "Frame Transmission in progress";
                    this.Info("Frame Transmission in progress");
                    this.deviceSettingsUp.Link.FrameGenRunning = true;
                }
                else
                {
                    uint fgDone_st = this.ReadYodaRg(mmap, FgDone);
                    if (fgDone_st != 0)
                    {
                        frameGeneratorStatus.ItemContent = "Frame Transmission completed";
                        this.WriteYodaRg(mmap, FgEn, 0);
                        this.Info("    Frame Generator disabled");
                        this.deviceSettingsUp.Link.FrameGenRunning = false;
                    }
                    else
                    {
                        frameGeneratorStatus.ItemContent = "Frame Transmission in progress";
                        this.Info("Frame Transmission in progress");
                        this.deviceSettingsUp.Link.FrameGenRunning = true;
                    }
                }
            }

            this.deviceSettingsUp.FrameGeneratorStatus = frameGeneratorStatus;
        }

        /// <summary>
        /// Report Frame Checker Status
        /// </summary>
        private void FrameCheckerStatus()
        {
            string mmap = "GEPhy";
            string FcEn = "FcEn";
            string FcTxSel = "FcTxSel";
            string RxErrCnt = "RxErrCnt";
            string FcFrmCntL = "FcFrmCntL";
            string FcFrmCntH = "FcFrmCntH";

            if (this.TenSPEDevice())
            {
                mmap = "IndirectAccessAddressMap";
                FcEn = "FC_EN";
                FcTxSel = "FC_TX_SEL";
                RxErrCnt = "RX_ERR_CNT";
                FcFrmCntL = "FC_FRM_CNT_L";
                FcFrmCntH = "FC_FRM_CNT_H";
            }

            uint fcEn_st = this.ReadYodaRg(mmap, FcEn);
            uint fcTxSel_st = this.ReadYodaRg(mmap, FcTxSel);

            TargetInfoItem frameCheckerStatus = new TargetInfoItem(this.deviceSettingsUp.FrameCheckerStatus.ItemName);
            frameCheckerStatus.IsAvailable = true;
            frameCheckerStatus.ItemContent = this.deviceSettingsUp.FrameCheckerStatus.ItemContent;
            if (fcEn_st != 0)
            {
                if (this.localLpbk == false)//we don't wanna update frames if device is in remote loopback
                {
                    uint errCnt = this.ReadYodaRg(mmap, RxErrCnt);
                    uint fCntL = this.ReadYodaRg(mmap, FcFrmCntL); // Latched by RxErrCnt
                    uint fCntH = this.ReadYodaRg(mmap, FcFrmCntH); // Latched by RxErrCnt
                    uint fCnt = (65536 * fCntH) + fCntL;

                    if (fCnt != 0)
                    {
                        this.checkedFrames += fCnt;
                        this.checkedFramesErrors += errCnt;

                        // We have received some frames
                        if (fcTxSel_st != 0)
                        {
                            frameCheckerStatus.ItemContent = string.Format("{0:d} Tx Side with {1:d} errors:", this.checkedFrames, this.checkedFramesErrors);
                        }
                        else
                        {
                            frameCheckerStatus.ItemContent = string.Format("{0:d} frames, {1:d} errors", this.checkedFrames, this.checkedFramesErrors);
                            this.Info(string.Format("FrameChecker Status: {0:d} frames, {1:d} errors", this.checkedFrames, this.checkedFramesErrors));
                        }
                    }
                }
            }
            else
            {
                frameCheckerStatus.ItemContent = "Disabled";
            }

            this.deviceSettingsUp.FrameCheckerStatus = frameCheckerStatus;
        }

        /// <summary>
        /// Enable / Disable Remote Loopback.
        /// </summary>
        /// <param name="lbRemoteEn_st">Parameter Description</param>
        private void RemoteLoopbackEnable(bool lbRemoteEn_st = true)
        {
            this.WriteYodaRg("GEPhy", "Loopback", 0);
            if (lbRemoteEn_st)
            {
                this.WriteYodaRg("GEPhy", "LbRemoteEn", 1);
                this.Info("    enable remote loopback");
            }
            else
            {
                this.WriteYodaRg("GEPhy", "LbRemoteEn", 0);
                this.Info("    disable remote loopback");
            }
        }

        /// <summary>
        /// Configures the PHY Lopback Modes
        /// </summary>
        /// <param name="gePhyLb_sel">Parameter Description 1</param>
        /// <param name="isolateRx_st">Parameter Description 2</param>
        /// <param name="lbTxSup_st">Parameter Description 3</param>
        public void PhyLoopbackConfig(LoopBackMode phyLb_sel = LoopBackMode.Digital, bool isolateRx_st = true, bool lbTxSup_st = true)
        {
            if(this.TenSPEDevice())
            {
                this.SPEPhyLoopbackConfig(phyLb_sel, isolateRx_st, lbTxSup_st);
            }
            else
            {
                this.GePhyLoopbackConfig(phyLb_sel, isolateRx_st, lbTxSup_st);
            }
        }

        /// <summary>
        /// Configure the GE PHY Loopback.
        /// </summary>
        /// <param name="gePhyLb_sel">Parameter Description 1</param>
        /// <param name="isolateRx_st">Parameter Description 2</param>
        /// <param name="lbTxSup_st">Parameter Description 3</param>
        private void GePhyLoopbackConfig(LoopBackMode gePhyLb_sel = LoopBackMode.Digital, bool isolateRx_st = true, bool lbTxSup_st = true)
        {
            //if (gePhyLb_sel == LoopBackMode.MII)
            //{
            //    this.WriteYodaRg("GEPhy", "LbMiiSel", 1);
            //    this.WriteYodaRg("GEPhy", "LbAllDigSel", 0);
            //    this.WriteYodaRg("GEPhy", "LbLdSel", 0);
            //    this.WriteYodaRg("GEPhy", "LbExtEn", 0);
            //    this.WriteYodaRg("GEPhy", "Loopback", 1);
            //    this.Info("    GE PHY Loopback configured as MII loopback");
            //}
            //else
            if (gePhyLb_sel == LoopBackMode.Digital)
            {
                this.WriteYodaRg("GEPhy", "LbAllDigSel", 1);

                //this.WriteYodaRg("GEPhy", "LbMiiSel", 0);
                this.WriteYodaRg("GEPhy", "LbLdSel", 0);
                this.WriteYodaRg("GEPhy", "LbExtEn", 0);
                this.WriteYodaRg("GEPhy", "Loopback", 1);
                if (lbTxSup_st)
                {
                    this.WriteYodaRg("GEPhy", "LbTxSup", 1);
                    this.Info("    GE PHY Loopback configured as Digital loopback - Tx suppressed");
                }
                else
                {
                    this.WriteYodaRg("GEPhy", "LbTxSup", 0);
                    this.Info("    GE PHY Loopback configured as Digital loopback - Tx not suppressed");
                }
            }
            else
            if (gePhyLb_sel == LoopBackMode.LineDriver)
            {
                this.WriteYodaRg("GEPhy", "LbLdSel", 1);

                // this.WriteYodaRg("GEPhy", "LbMiiSel", 0);
                this.WriteYodaRg("GEPhy", "LbAllDigSel", 0);
                this.WriteYodaRg("GEPhy", "LbExtEn", 0);
                this.WriteYodaRg("GEPhy", "Loopback", 1);
                this.Info("    GE PHY Loopback configured as LineDriver loopback");
            }
            else if (gePhyLb_sel == LoopBackMode.ExtCable)
            {
                this.WriteYodaRg("GEPhy", "LbExtEn", 1);

                //this.WriteYodaRg("GEPhy", "LbMiiSel", 0);
                this.WriteYodaRg("GEPhy", "LbAllDigSel", 0);
                this.WriteYodaRg("GEPhy", "LbLdSel", 0);
                this.WriteYodaRg("GEPhy", "Loopback", 0);
                this.Info("    GE PHY Loopback configured as ExtCable loopback");
            }
            else if (gePhyLb_sel == LoopBackMode.OFF)
            {
                //this.WriteYodaRg("GEPhy", "LbMiiSel", 0);
                this.WriteYodaRg("GEPhy", "LbAllDigSel", 0);
                this.WriteYodaRg("GEPhy", "LbLdSel", 0);
                this.WriteYodaRg("GEPhy", "LbExtEn", 0);
                this.WriteYodaRg("GEPhy", "Loopback", 0);
                this.Info("    GE PHY Loopback disabled");
            }
            else
            {
                this.Info("    GE PHY Loopback NOT configured - use one of MII / Digital / LineDriver / ExtCable / OFF");
            }

            if (isolateRx_st)
            {
                this.WriteYodaRg("GEPhy", "IsolateRx", 1);
                this.Info("      - Rx data suppressed");
            }
            else
            {
                this.WriteYodaRg("GEPhy", "IsolateRx", 0);
                this.Info("      - Rx data forwarded to MAC IF");
            }
        }

        /// <summary>
        /// Configure the SPE PHY Loopback.
        /// </summary>
        /// <param name="gePhyLb_sel">Parameter Description 1</param>
        /// <param name="isolateRx_st">Parameter Description 2</param>
        /// <param name="lbTxSup_st">Parameter Description 3</param>
        private void SPEPhyLoopbackConfig(LoopBackMode spePhyLb_sel = LoopBackMode.Digital, bool isolateRx_st = true, bool lbTxSup_st = true)
        {
            switch (spePhyLb_sel)
            {
                // PCS
                case LoopBackMode.Digital:
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PMA_LOC_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PCS_EN", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "RMII_TXD_CHK_EN", 0);

                    this.Info("    SPE PHY Loopback configured as PCS loopback");
                    break;

                // PMA
                case LoopBackMode.LineDriver:
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PMA_LOC_EN", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PCS_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "RMII_TXD_CHK_EN", 0);

                    this.Info("    SPE PHY Loopback configured as PMA loopback");
                    break;

                // ExtMII,RMII
                case LoopBackMode.ExtCable:
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PMA_LOC_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PCS_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "RMII_TXD_CHK_EN", 1);

                    this.Info("    SPE PHY Loopback configured as External MII/RMII loopback");
                    break;

                // MAC IF Remote
                case LoopBackMode.MacRemote:
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PMA_LOC_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PCS_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_EN", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "RMII_TXD_CHK_EN", 0);

                    // Rx Suppression
                    if (isolateRx_st)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_RX_SUP_EN", 1);
                        this.Info("    SPE PHY Loopback configured as MAC Interface Remote loopback - Rx suppressed");
                    }
                    else
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_RX_SUP_EN", 0);
                        this.Info("    SPE PHY Loopback configured as MAC Interface Remote loopback - Rx not suppressed");
                    }

                    break;

                // MAC IF
                case LoopBackMode.MAC:
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PMA_LOC_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PCS_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_EN", 1);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "RMII_TXD_CHK_EN", 0);

                    // Tx Suppression
                    if (lbTxSup_st)
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_TX_SUP_EN", 1);
                        this.Info("    SPE PHY Loopback configured as MAC Interface loopback - Tx suppressed");
                    }
                    else
                    {
                        this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_TX_SUP_EN", 0);
                        this.Info("    SPE PHY Loopback configured as MAC Interface loopback - Tx not suppressed");
                    }

                    break;

                // OFF
                case LoopBackMode.OFF:
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PMA_LOC_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "B10L_LB_PCS_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_EN", 0);
                    this.WriteYodaRg("IndirectAccessAddressMap", "RMII_TXD_CHK_EN", 0);

                    this.Info("    SPE PHY Loopback disabled");
                    break;

                default:
                    this.Info("    SPE PHY Loopback NOT configured - use one of PMA / PCS / MAC Interface / MAC Interface Remote / External MII/RMII");
                    break;
            }
        }

        /// <summary>
        /// Loopback Tx Supression
        /// </summary>
        /// <param name="lbTxSup_st">Tx Supression</param>
        public void SPEPhyLoopbackTxSuppression(bool lbTxSup_st)
        {
            if (lbTxSup_st)
            {
                this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_TX_SUP_EN", 1);
                this.Info("    SPE PHY Loopback configured - Tx suppressed");
            }
            else
            {
                this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_LB_TX_SUP_EN", 0);
                this.Info("    SPE PHY Loopback configured - Tx not suppressed");
            }
        }

        /// <summary>
        /// Loopback Rx Supression
        /// </summary>
        /// <param name="isolateRx_st">Rx Supression</param>
        public void SPEPhyLoopbackRxSuppression(bool isolateRx_st)
        {
            if (isolateRx_st)
            {
                this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_RX_SUP_EN", 1);
                this.Info("    SPE PHY Loopback configured - Rx suppressed");
            }
            else
            {
                this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_RX_SUP_EN", 0);
                this.Info("    SPE PHY Loopback configured - Rx not suppressed");
            }
        }

        /// <summary>
        /// Report Loopback Status.
        /// </summary>
        private void GePhyLoopbackStatus()
        {
            if (this.ReadYodaRg("GEPhy", "LbMiiSel") != 0)
            {
                this.Info("    GE PHY MII Loopback selected:");
                if (this.ReadYodaRg("GEPhy", "LbAllDigSel") != 0)
                {
                    if (this.ReadYodaRg("GEPhy", "LbTxSup") != 0)
                    {
                        this.Info("    GE PHY Digital Loopback (Tx suppressed) selected:");
                    }
                    else
                    {
                        this.Info("    GE PHY Digital Loopback (Tx NOT suppressed) selected:");
                    }
                }

                if (this.ReadYodaRg("GEPhy", "LbLdSel") != 0)
                {
                    this.Info("    GE PHY LineDriver Loopback selected:");
                }

                if (this.ReadYodaRg("GEPhy", "Loopback") != 0)
                {
                    this.Info("      - Loopback Enabled:");
                }

                if (this.ReadYodaRg("GEPhy", "LbExtEn") != 0)
                {
                    this.Info("    GE PHY ExtCable Loopback Enabled");
                }

                if (this.ReadYodaRg("GEPhy", "IsolateRx") != 0)
                {
                    this.Info("      - Rx data suppressed");
                }
            }
            else
            {
                this.Info("      - Rx data forwarded to MAC IF");
            }
        }

        /// <summary>
        /// Configure a specific clock on the GP_CLK pin.
        /// </summary>
        /// <param name="gP_CLK_sel">Parameter Description</param>
        public void GP_CLKConfig(GPClockSel gP_CLK_sel = GPClockSel.RecoveredReceiver125MHz)
        {
            if (gP_CLK_sel == GPClockSel.Digital25MHz)
            {
                this.WriteYodaRg("GESubsys", "GeClkRcvr125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkFree125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtRcvrEn", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtFreeEn", 0);
                this.WriteYodaRg("GESubsys", "GeClk25En", 0);

                this.WriteYodaRg("GESubsys", "GeClk25En", 1);
                this.Info("    GE PHY 25 Mhz clock output on GP_CLK pin");
            }
            else if (gP_CLK_sel == GPClockSel.RecoveredReceiver125MHz)
            {
                this.WriteYodaRg("GESubsys", "GeClkRcvr125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkFree125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtRcvrEn", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtFreeEn", 0);
                this.WriteYodaRg("GESubsys", "GeClk25En", 0);

                this.WriteYodaRg("GESubsys", "GeClkRcvr125En", 1);
                this.Info("    GE PHY 125 Mhz recovered clock output on GP_CLK pin");
            }
            else if (gP_CLK_sel == GPClockSel.GeClkFree125)
            {
                this.WriteYodaRg("GESubsys", "GeClkRcvr125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkFree125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtRcvrEn", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtFreeEn", 0);
                this.WriteYodaRg("GESubsys", "GeClk25En", 0);

                this.WriteYodaRg("GESubsys", "GeClkFree125En", 1);
                this.Info("    GE PHY 125 Mhz free-running clock output on GP_CLK pin");
            }
            else if (gP_CLK_sel == GPClockSel.GeClkHrtRcvr)
            {
                this.WriteYodaRg("GESubsys", "GeClkRcvr125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkFree125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtRcvrEn", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtFreeEn", 0);
                this.WriteYodaRg("GESubsys", "GeClk25En", 0);

                this.WriteYodaRg("GESubsys", "GeClkHrtRcvrEn", 1);
                this.Info("    GE PHY recovered heartbeat clock output on GP_CLK pin");
            }
            else if (gP_CLK_sel == GPClockSel.GeClkHrtFree)
            {
                this.WriteYodaRg("GESubsys", "GeClkRcvr125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkFree125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtRcvrEn", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtFreeEn", 0);
                this.WriteYodaRg("GESubsys", "GeClk25En", 0);

                this.WriteYodaRg("GESubsys", "GeClkHrtFreeEn", 1);
                this.Info("    GE PHY free-running heartbeat clock output on GP_CLK pin");
            }
            else if (gP_CLK_sel == GPClockSel.OFF)
            {
                this.WriteYodaRg("GESubsys", "GeClkRcvr125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkFree125En", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtRcvrEn", 0);
                this.WriteYodaRg("GESubsys", "GeClkHrtFreeEn", 0);

                this.WriteYodaRg("GESubsys", "GeClk25En", 0);
                this.Info("    No clock output on GP_CLK pin");
            }
            else
            {
                this.Info("    GP_CLK NOT configured - use one of GeClk25 / GeClkRcvr125 / GeClkFree125 / GeClkHrtRcvr / GeClkHrtFree / GeTstClkRcvr125 / GeTstClkFree125 / OFF");
            }
        }

        /// <summary>
        /// Enable reference clock output on REF_CLK pin.
        /// </summary>
        /// <param name="geRefClkEn_st">Parameter Description</param>
        public void REF_CLKEnable(bool geRefClkEn_st = true)
        {
            if (geRefClkEn_st)
            {
                this.WriteYodaRg("GESubsys", "GeRefClkEn", 1);
                this.Info("    25 MHz reference clock output on REF_CLK pin");
            }
            else
            {
                this.WriteYodaRg("GESubsys", "GeRefClkEn", 0);
                this.Info("    clock output on REF_CLK disabled");
            }
        }

        /// <summary>
        /// Read latched Link Status bit.
        /// </summary>
        private void ReadLinkStatus()
        {
            this.Info(string.Format("    Read:-  Latched Link Status bit = {0:d}", this.ReadYodaRg("GEPhy", "LinkStatLat")));
        }

        /// <summary>
        /// Read resolved HCD technology
        /// </summary>
        private void ReadHcdResolved()
        {
            EthernetSpeeds resolvedHCD = (EthernetSpeeds)this.ReadYodaRg("GEPhy", "HcdTech");

            this.Info(string.Format("    Read:-  Resolved HCD Technology = ({1:s})", resolvedHCD.ToString()));
        }
        /// <summary>
        /// Reset PHY and Setup for remote loopback.
        /// </summary>
        /// <param name="enable"> Enable or Disable Loopback mode </param>
        public void ConfigureForRemoteLoopback(bool enable)//dani remote loop back
        {
            if (this.TenSPEDevice())
            {
                if (enable == true)
                {
                    this.Info("    enable remote loopback");
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_EN", 0x1);
                    this.localLpbk = true;
                }
                else
                {
                    this.Info("    disable remote loopback");
                    this.WriteYodaRg("IndirectAccessAddressMap", "MAC_IF_REM_LB_EN", 0x0);
                    this.localLpbk = false;
                }
            }
            else
            {
                this.Info("  GESubsys software reset");
                this.WriteYodaRg("GESubsys", "GeSftRst", 1);
                this.Sleep(0.1);
                this.Info("  GE PHY enters software reset, stays in software powerdown");
                this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
                this.WriteYodaRg("GESubsys", "GePhyRst", 1);
                this.Sleep(0.1);

                this.Info("  Apply base settings for UNH-IOL testing");
                this.ApplyIOLBaseSettings();

                this.Info("    configure for remote loopback,");
                this.Info("    enable remote loopback");
                this.WriteYodaRg("GEPhy", "LbRemoteEn", 1);
                this.Info("    exit software powerdown");
                this.WriteYodaRg("GEPhy", "SftPd", 0);
                this.Info("  Device configured for remote loopback");
            }
        }

        /// <summary>
        /// Setup for recovered clock output on GP_CLK pin.
        /// </summary>
        private void SetupClkRcvr125Out()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   configure operating mode");
            this.WriteYodaRg("GEPhy", "LinkEn", 0);
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.PollEqYodaRg("GEPhy", "PhyInStndby", 1, 0.05);
            this.Info("  GESubsys configure for clk_rcvr_125 output");
            this.WriteYodaRg("GESubsys", "GeClkCfg", 0);
            this.WriteYodaRg("GESubsys", "GeClkRcvr125En", 1);
            this.Info("  Device configured for GE PHY recovered clock measurement on GP_CLK pin");
        }

        /// <summary>
        /// Setup for free-running clock output on GP_CLK pin.
        /// </summary>
        private void SetupClkFree125Out()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   configure operating mode");
            this.WriteYodaRg("GEPhy", "LinkEn", 0);
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.PollEqYodaRg("GEPhy", "PhyInStndby", 1, 0.05);
            this.Info("  GESubsys configure for clk_free_125 output");
            this.WriteYodaRg("GESubsys", "GeClkCfg", 0);
            this.WriteYodaRg("GESubsys", "GeClkFree125En", 1);
            this.Info("  Device configured for GE PHY free-running clock measurement on GP_CLK pin");
        }

        /// <summary>
        /// Setup for PHY Test clock (recovered) output on REF_CLK pin.
        /// </summary>
        private void SetupTclkRcvrOut()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   configure operating mode");
            this.WriteYodaRg("GEPhy", "LinkEn", 0);
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.PollEqYodaRg("GEPhy", "PhyInStndby", 1, 0.05);
            this.Info("  GESubsys configure for clk_rcvr_125 output");
            this.WriteYodaRg("GEPhy", "TclkFreeSel", 0);
            this.WriteYodaRg("GEPhy", "TstClkEn", 1);
            this.WriteYodaRg("GESubsys", "GeClkCfg", 0);
            this.WriteYodaRg("GESubsys", "GeTclkEn", 1);
            this.Info("  Device configured for GE PHY test clock (recovered) measurement on REF_CLK pin");
        }

        /// <summary>
        /// Setup for PHY Test clock (free-running) output on REF_CLK pin.
        /// </summary>
        private void SetupTclkFreeOut()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   configure operating mode");
            this.WriteYodaRg("GEPhy", "LinkEn", 0);
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.PollEqYodaRg("GEPhy", "PhyInStndby", 1, 0.05);
            this.Info("  GESubsys configure for clk_free_125 output");
            this.WriteYodaRg("GEPhy", "TclkFreeSel", 1);
            this.WriteYodaRg("GEPhy", "TstClkEn", 1);
            this.WriteYodaRg("GESubsys", "GeClkCfg", 0);
            this.WriteYodaRg("GESubsys", "GeTclkEn", 1);
            this.Info("  Device configured for GE PHY test clock (free-running) measurement on REF_CLK pin");
        }

        /// <summary>
        /// Setup for reference clock output on REF_CLK pin.
        /// </summary>
        private void SetupRefClkOut()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Enable reference clock output");
            this.WriteYodaRg("GESubsys", "GeRefClkEn", 1);
            this.Info("  Device configured for 25 MHz reference clock measurement on REF_CLK pin");
        }

        /// <summary>
        /// Apply base settings for UNH-IOL testing.
        /// </summary>
        private void ApplyIOLBaseSettings()
        {
            // this.Info("    disable link watch-dog timer");
            this.WriteYodaRg("GEPhy", "LnkWdEn", 0);

            this.Info("    disable energy detect power-down");
            this.WriteYodaRg("GEPhy", "NrgPdEn", 0);

            // this.Info("    disable automatic MDI crossover resolution before forced speed");
            // this.WriteYodaRg("GEPhy", "RslvMdiManSpeed", 0);

            this.Info("    disable automatic speed down-shift");
            this.WriteYodaRg("GEPhy", "DnSpeedTo10En", 0);

            try
            {
                this.WriteYodaRg("GEPhy", "DnSpeedTo100En", 0);
            }
            catch (ArgumentException e)
            {
                /* This register does not exist in this device */
            }

            //this.Info("    disable arbitration watchdog timer");
            this.WriteYodaRg("GEPhy", "ArbWdEn", 0);

            // this.Info("    disable cable diagnostics during auto-negotiation");
            // this.WriteYodaRg("GEPhy", "CdiagOnAneg", 0);

            // this.Info("    disable 10BASE-T low power transmit");
            this.WriteYodaRg("GEPhy", "B10LpTxEn", 0);

            this.Info("    disable Energy Efficient Ethernet");
            this.WriteYodaRg("GEPhy", "EeeAdv", 0);

            this.Info("    disable extended next pages");
            this.WriteYodaRg("GEPhy", "ExtNextPageAdv", 0);

            // this.Info("    GESubsys, minimize TX elasticity buffers latency");
            this.WriteYodaRg("GESubsys", "GeFifoDpth", 0);

            this.WriteYodaRg("GEPhy", "DpthMiiByte", 0);
        }

        /// <summary>
        /// Software reset of all GEPhy ports, and apply settings for UNH-IOL testing.
        /// </summary>
        public void SftRstGEPhyIOLBase()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("  GEPhy exits software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
        }

        /// <summary>
        /// Setup for 100BASE-TX VOD measurements.
        /// </summary>
        public void SetupB100VOD()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("    configure for auto-negotiation disabled, 100BASE-TX, forced MDI, linking enabled");
            this.WriteYodaRg("GEPhy", "AutonegEn", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelLsb", 1);
            this.WriteYodaRg("GEPhy", "AutoMdiEn", 0);
            this.WriteYodaRg("GEPhy", "ManMdix", 0);
            this.WriteYodaRg("GEPhy", "LinkEn", 1);
            this.Info("    exit software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("  Device configured for 100BASE-TX VOD measurement");
        }

        /// <summary>
        /// Setup for 10BASE-T1L to exit from test modes and to proceed in normal operation
        /// </summary>
        public void SetupT1L_NormalMode()
        {
            this.Info("  10SPE Phy software reset");

            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_RST", 1);
            this.Sleep(0.1);
            this.Info("  10SPE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
            this.ReadYodaRg("IndirectAccessAddressMap", "CRSM_STAT");
            this.Sleep(0.1);
            //           this.Info("  Apply base settings for UNH-IOL testing");
            //           this.ApplyIOLBaseSettings();

            this.Info("   exit software powerdown, configure for 10BASE-T1L normal mode");
            this.WriteYodaRg("IndirectAccessAddressMap", "B10L_TX_TEST_MODE", 0);
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
            this.Info("  Device configured for 10BASE-T1Ls normal operation");
        }

        /// <summary>
        /// Setup for 10BASE-T1L test mode 1 measurements.
        /// </summary>
        public void SetupT1L_TestMode1()
        {
            this.Info("  10SPE Phy software reset");

            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_RST", 1);
            this.Sleep(0.1);
            this.Info("  10SPE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
            this.ReadYodaRg("IndirectAccessAddressMap", "CRSM_STAT");
            this.Sleep(0.1);

            //           this.Info("  Apply base settings for UNH-IOL testing");
            //           this.ApplyIOLBaseSettings();

            this.Info("   exit software powerdown, configure for 10BASE-T1L test mode 1");
            this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 0);
            this.WriteValueInRegisterAddress(0x078000, 1);
            this.WriteYodaRg("IndirectAccessAddressMap", "B10L_TX_TEST_MODE", 1);
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
            this.Info("  Device configured for 10BASE-T1Ls test mode 1 measurement");
        }

        /// <summary>
        /// Setup for 10BASE-T1L test mode 2 measurements.
        /// </summary>
        public void SetupT1L_TestMode2()
        {
            this.Info("  10SPE Phy software reset");

            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_RST", 1);
            this.Sleep(0.1);
            this.Info("  10SPE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
            this.ReadYodaRg("IndirectAccessAddressMap", "CRSM_STAT");
            this.Sleep(0.1);

            //           this.Info("  Apply base settings for UNH-IOL testing");
            //           this.ApplyIOLBaseSettings();

            this.Info("   exit software powerdown, configure for 10BASE-T1L test mode 2");
            this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 0);
            this.WriteValueInRegisterAddress(0x078000, 1);
            this.WriteYodaRg("IndirectAccessAddressMap", "B10L_TX_TEST_MODE", 2);
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
            this.Info("  Device configured for 10BASE-T1Ls test mode 2 measurement");
        }

        /// <summary>
        /// Setup for 10BASE-T1L test mode 3 measurements.
        /// </summary>
        public void SetupT1L_TestMode3()
        {
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_RST", 1);
            this.Sleep(0.1);
            this.Info("  10SPE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 1);
            this.ReadYodaRg("IndirectAccessAddressMap", "CRSM_STAT");
            this.Sleep(0.1);

            //           this.Info("  Apply base settings for UNH-IOL testing");
            //           this.ApplyIOLBaseSettings();

            this.Info("   exit software powerdown, configure for 10BASE-T1L test mode 3");
            this.WriteYodaRg("IndirectAccessAddressMap", "AN_EN", 0);
            this.WriteValueInRegisterAddress(0x078000, 1);
            this.WriteYodaRg("IndirectAccessAddressMap", "B10L_TX_TEST_MODE", 3);
            this.WriteYodaRg("IndirectAccessAddressMap", "CRSM_SFT_PD", 0);
            this.Info("  Device configured for 10BASE-T1Ls test mode 3 measurement");
        }

        /// <summary>
        /// Setup for 1000BASE-T test mode 1 measurements.
        /// </summary>
        public void SetupB1000TestMode1()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   exit software powerdown, configure for 1000BASE-T test mode 1");
            this.WriteYodaRg("GEPhy", "TstMode", 1);
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("  Device configured for 1000BASE-T test mode 1 measurement");
        }

        /// <summary>
        /// Setup for 1000BASE-T test mode 2 measurements.
        /// </summary>
        public void SetupB1000TestMode2()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   exit software powerdown, configure for 1000BASE-T test mode 2");
            this.WriteYodaRg("GEPhy", "TstMode", 2);
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("  Device configured for 1000BASE-T test mode 2 measurement");
        }

        /// <summary>
        /// Setup for 1000BASE-T test mode 3 measurements.
        /// </summary>
        public void SetupB1000TestMode3()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   exit software powerdown, configure for 1000BASE-T test mode 3");
            this.WriteYodaRg("GEPhy", "TstMode", 3);
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("  Device configured for 1000BASE-T test mode 3 measurement");
        }

        /// <summary>
        /// Setup for 1000BASE-T test mode 4 measurements.
        /// </summary>
        public void SetupB1000TestMode4()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   exit software powerdown, configure for 1000BASE-T test mode 4");
            this.WriteYodaRg("GEPhy", "TstMode", 4);
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("  Device configured for 1000BASE-T test mode 4 measurement");
        }

        /// <summary>
        /// Setup for 10BASE-T forced mode in loopback with Tx suppression disabled, for link pulse measurements.
        /// </summary>
        public void SetupB10LbTxEn()
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   configure for auto-negotiation disabled, 10BASE-T,");
            this.Info("    forced MDI, loopback enabled, Tx suppression disabled, linking enabled");
            this.WriteYodaRg("GEPhy", "AutonegEn", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelLsb", 0);
            this.WriteYodaRg("GEPhy", "AutoMdiEn", 0);
            this.WriteYodaRg("GEPhy", "ManMdix", 0);
            this.WriteYodaRg("GEPhy", "LbTxSup", 0);
            this.WriteYodaRg("GEPhy", "Loopback", 1);
            this.WriteYodaRg("GEPhy", "LinkEn", 1);
            this.Info("   exit software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("  Device configured for 10BASE-T forced mode link pulse measurement");
        }

        /// <summary>
        /// Setup for 10BASE-T forced mode in loopback with Tx suppression disabled,\nwith transmission of frames with random payloads using the frame generator.
        /// </summary>
        /// <param name="frm_len">Parameter Description</param>
        public void SetupB10LbTxEnFgRnd(uint frm_len = 1500)
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   configure for auto-negotiation disabled, 10BASE-T,");
            this.Info("    forced MDI, loopback enabled, Tx suppression disabled, linking enabled");
            this.WriteYodaRg("GEPhy", "AutonegEn", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelLsb", 0);
            this.WriteYodaRg("GEPhy", "AutoMdiEn", 0);
            this.WriteYodaRg("GEPhy", "ManMdix", 0);
            this.WriteYodaRg("GEPhy", "LbTxSup", 0);
            this.WriteYodaRg("GEPhy", "Loopback", 1);
            this.WriteYodaRg("GEPhy", "LinkEn", 1);
            this.Info("   exit software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("   poll for link up");
            this.PollEqYodaRg("GEPhy", "LinkStat", 1, 2.0);
            this.Info(string.Format("   configure for transmission of frames of length {0:d} bytes, random payload", frm_len));
            this.WriteYodaRg("GEPhy", "DiagClkEn", 1);
            this.WriteYodaRg("GEPhy", "FgFrmLen", frm_len);
            this.WriteYodaRg("GEPhy", "FgContModeEn", 1);
            this.WriteYodaRg("GEPhy", "FgEn", 1);
            this.Info("  Device configured for 10BASE-T forced mode, with random payload frame transmission");
        }

        /// <summary>
        /// Setup for 10BASE-T forced mode in loopback with Tx suppression disabled,\nwith transmission of frames with 0xFF repeating payloads using the frame generator.
        /// </summary>
        /// <param name="frm_len">Parameter Description</param>
        public void SetupB10LbTxEnFgAll1s(uint frm_len = 1500)
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   configure for auto-negotiation disabled, 10BASE-T,");
            this.Info("    forced MDI, loopback enabled, Tx suppression disabled, linking enabled");
            this.WriteYodaRg("GEPhy", "AutonegEn", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelLsb", 0);
            this.WriteYodaRg("GEPhy", "AutoMdiEn", 0);
            this.WriteYodaRg("GEPhy", "ManMdix", 0);
            this.WriteYodaRg("GEPhy", "LbTxSup", 0);
            this.WriteYodaRg("GEPhy", "Loopback", 1);
            this.WriteYodaRg("GEPhy", "LinkEn", 1);
            this.Info("   exit software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("   poll for link up");
            this.PollEqYodaRg("GEPhy", "LinkStat", 1, 2.0);
            this.Info(string.Format("   configure for transmission of frames of length {0:d} bytes, 0xFF repeating payload", frm_len));
            this.WriteYodaRg("GEPhy", "DiagClkEn", 1);
            this.WriteYodaRg("GEPhy", "FgFrmLen", frm_len);
            this.WriteYodaRg("GEPhy", "FgContModeEn", 1);
            this.WriteYodaRg("GEPhy", "FgCntrl", 3);
            this.WriteYodaRg("GEPhy", "FgNoHdr", 1);
            this.WriteYodaRg("GEPhy", "FgNoFcs", 1);
            this.WriteYodaRg("GEPhy", "FgEn", 1);
            this.Info("  Device configured for 10BASE-T forced mode, with 0xFF repeating payload frame transmission");
        }

        /// <summary>
        /// Setup for 10BASE-T forced mode in loopback with Tx suppression disabled,\nwith transmission of frames with 0x00 repeating payloads using the frame generator.
        /// </summary>
        /// <param name="frm_len">Parameter Description</param>
        public void SetupB10LbTxEnFgAll0s(uint frm_len = 1500)
        {
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info("   configure for auto-negotiation disabled, 10BASE-T,");
            this.Info("    forced MDI, loopback enabled, Tx suppression disabled, linking enabled");
            this.WriteYodaRg("GEPhy", "AutonegEn", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelMsb", 0);
            this.WriteYodaRg("GEPhy", "SpeedSelLsb", 0);
            this.WriteYodaRg("GEPhy", "AutoMdiEn", 0);
            this.WriteYodaRg("GEPhy", "ManMdix", 0);
            this.WriteYodaRg("GEPhy", "LbTxSup", 0);
            this.WriteYodaRg("GEPhy", "Loopback", 1);
            this.WriteYodaRg("GEPhy", "LinkEn", 1);
            this.Info("   exit software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("   poll for link up");
            this.PollEqYodaRg("GEPhy", "LinkStat", 1, 2.0);
            this.Info(string.Format("   configure for transmission of frames of length {0:d} bytes, 0x00 repeating payload", frm_len));
            this.WriteYodaRg("GEPhy", "DiagClkEn", 1);
            this.WriteYodaRg("GEPhy", "FgFrmLen", frm_len);
            this.WriteYodaRg("GEPhy", "FgContModeEn", 1);
            this.WriteYodaRg("GEPhy", "FgCntrl", 2);
            this.WriteYodaRg("GEPhy", "FgNoHdr", 1);
            this.WriteYodaRg("GEPhy", "FgNoFcs", 1);
            this.WriteYodaRg("GEPhy", "FgEn", 1);
            this.Info("  Device configured for 10BASE-T forced mode, with 0x00 repeating payload frame transmission");
        }

        /// <summary>
        /// Setup for 10BASE-T transmit test mode, consisting of 5 MHz square wave on desired dimesion.
        /// </summary>
        /// <param name="afe_dim">Parameter Description</param>
        public void SetupB10TxTst5MHz(uint afe_dim = 0)
        {
            // if (afe_dim not in [0, 1])
            // raise plec.plec_ex.InputOutOfRangeError("afe_dim", afe_dim)
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info(string.Format("   configure for 10BASE-T transmit 5 MHz square wave test mode transmission on dim {0:d}", afe_dim));
            this.WriteYodaRg("GEPhy", "B10TxTstMode", afe_dim + 3);
            this.Info("   exit software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("  Device configured for 10BASE-T test mode transmission (5 MHz)");
        }

        /// <summary>
        /// Setup for 10BASE-T transmit test mode, consisting of 10 MHz square wave on desired dimesion.
        /// </summary>
        /// <param name="afe_dim">Parameter Description</param>
        public void SetupB10TxTst10MHz(uint afe_dim = 0)
        {
            // if (afe_dim not in [0, 1])
            //    raise plec.plec_ex.InputOutOfRangeError("afe_dim", afe_dim)
            this.Info("  GESubsys software reset");
            this.WriteYodaRg("GESubsys", "GeSftRst", 1);
            this.Sleep(0.1);
            this.Info("  GE PHY enters software reset, stays in software powerdown");
            this.WriteYodaRg("GESubsys", "GePhySftPdCfg", 1);
            this.WriteYodaRg("GESubsys", "GePhyRst", 1);
            this.Sleep(0.1);

            this.Info("  Apply base settings for UNH-IOL testing");
            this.ApplyIOLBaseSettings();

            this.Info(string.Format("   configure for 10BASE-T transmit 10 MHz square wave test mode transmission on dim {0:d}", afe_dim));
            this.WriteYodaRg("GEPhy", "B10TxTstMode", afe_dim + 1);
            this.Info("   exit software powerdown");
            this.WriteYodaRg("GEPhy", "SftPd", 0);
            this.Info("  Device configured for 10BASE-T test mode transmission (10 MHz)");
        }
        /// <summary>
        /// Scans for PHY HW MDIO address
        /// </summary>
        public void ScanMDIOHwAddress()
        {
            if (this.TenSPEDevice())
            {
                uint i = 0;
                uint dev_ID = 0;
                for (; i < 8; i++)
                {
                    this.deviceConnection.ModifyMDIOAddress(i);
                    dev_ID = this.deviceConnection.ReadMDIORegister(0x1e0003);
                    if (dev_ID == 0xbc80)
                    {
                        break;
                    }
                }

                if (i > 7) //max number of adin1100 devices
                {
                    this.deviceConnection.ModifyMDIOAddress(0); //we didn't found any
                }

                // this.TenSpe2p4VCapableCheck();
            }
        }

        ///// <summary>
        ///// Gets capability for 2.4 V Linking and PMA Loopback
        ///// </summary>
        ///// <returns>True - 2.4V Capavle/ False 1 V capable</returns>
        //private void TenSpe2p4VCapableCheck()
        //{
        //    uint value = this.deviceConnection.ReadMDIORegister(0X0108F7);
        //    if ((value & 0X1000) == 0X1000) //BITM_B10L_PMA_STAT_B10L_TX_LVL_HI_ABLE
        //    {
        //        this.tenSpE2p4VoltCapable = true;
        //    }
        //    else
        //    {
        //        this.tenSpE2p4VoltCapable = false;
        //    }

        //    if ((value & 0X2000) == 0X2000) // BITM_B10L_PMA_STAT_B10L_LB_PMA_LOC_ABLE
        //    {
        //        this.tenSpEPMALoopBackCapable = true;
        //    }
        //    else
        //    {
        //        this.tenSpEPMALoopBackCapable = false;
        //    }
        //}

        /// <summary>
        /// Assert LPI request
        /// </summary>
        private void AssertLPI()
        {
            this.WriteYodaRg("GEPhy", "FgManLpiEn", 1);
            this.WriteYodaRg("GEPhy", "FgManLpi", 1);
            this.ReadCheckYodaRg("GEPhy", "TxLpi", 1);
            this.Info("   LPI request asserted");
        }

        /// <summary>
        /// Deassert LPI request.
        /// </summary>
        private void DeassertLPI()
        {
            this.WriteYodaRg("GEPhy", "FgManLpiEn", 1);
            this.WriteYodaRg("GEPhy", "FgManLpi", 0);
            this.ReadCheckYodaRg("GEPhy", "TxLpi", 0);
            this.Info("   LPI request deasserted");
        }

        /// <summary>
        /// Enable recovered clock output on GP_CLK pin
        /// </summary>
        private void TclkRcvrOutEnable()
        {
            this.WriteYodaRg("GEPhy", "TstClkEn", 0);
            this.Info("  GESubsys configure for Tclk output");
            this.WriteYodaRg("GEPhy", "TxTclkEnB1000", 255);
            this.WriteYodaRg("GEPhy", "RxTclkEnB100", 127);
            this.WriteYodaRg("GEPhy", "TclkFreeSel", 0);
            this.WriteYodaRg("GEPhy", "TstClkEn", 1);
            this.WriteYodaRg("GESubsys", "GeClkCfg", 0);
            this.WriteYodaRg("GESubsys", "GeTclkEn", 1);
            this.Info("   enabled test clock (recovered clock) output");
        }

        /// <summary>
        /// Enable free-running clock output on GP_CLK pin
        /// </summary>
        private void TclkFreeOutEnable()
        {
            this.WriteYodaRg("GEPhy", "TstClkEn", 0);
            this.Info("  GESubsys configure for Tclk output");
            this.WriteYodaRg("GEPhy", "TxTclkEnB1000", 255);
            this.WriteYodaRg("GEPhy", "RxTclkEnB100", 127);
            this.WriteYodaRg("GEPhy", "TclkFreeSel", 1);
            this.WriteYodaRg("GEPhy", "TstClkEn", 1);
            this.WriteYodaRg("GESubsys", "GeClkCfg", 0);
            this.WriteYodaRg("GESubsys", "GeTclkEn", 1);
            this.Info("   enabled test clock (free-running clock) output");
        }

        /// <summary>
        /// Disable all test clock output on TX_TCLK0/1 pin
        /// </summary>
        private void TclkOutDisable()
        {
            this.Info("  GESubsys configure for Tclk output disable");
            this.WriteYodaRg("GEPhy", "TstClkEn", 0);
            this.Info("   disabled test clock output");
        }

        /// <summary>
        /// Speed Lookup Struct
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        private class RegisterLookUp<T>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RegisterLookUp{T}"/> class.
            /// A look up for resolving
            /// </summary>
            /// <param name="name">Register Name</param>
            /// <param name="value">Register value</param>
            public RegisterLookUp(string name, T value)
            {
                this.Name = name;
                this.Value = value;
            }

            /// <summary>
            /// Gets or sets Name
            /// </summary>
            public string Name
            {
                get;

                set;
            }

            /// <summary>
            /// Gets or sets Value
            /// </summary>
            public T Value
            {
                get;

                set;
            }
        }
    }
}
