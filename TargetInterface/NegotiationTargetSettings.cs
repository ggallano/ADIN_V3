// <copyright file="NegotiationTargetSettings.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace TargetInterface
{
    using System.Collections.Generic;
    using System.Reflection;
    using static FirmwareAPI;

    /// <summary>
    /// Negotiation Target Settings
    /// </summary>
    public class NegotiationTargetSettings : Settings
    {
        private MasterSlavePreference preferMasterSlave = MasterSlavePreference.Master;

        private MasterSlaveNegotiate negotiateMasterSlave = MasterSlaveNegotiate.Prefer_Master;

        private SignalPeakToPeakVoltage pkpkVoltage = SignalPeakToPeakVoltage.Capable2p4Volts_Requested2p4Volts;//.CapableTwoPointFourVolts_RequestedTwoPointFourVolts;//.CapableTwoPointFourVolts_RequestedOneVolt;

        private Dictionary<EthernetSpeeds, string> ethernetSpeedStrings = new Dictionary<EthernetSpeeds, string>();

        private List<EthernetSpeeds> remoteAdvSpeeds = new List<EthernetSpeeds>();

        private List<EthernetSpeeds> localAdvSpeeds = new List<EthernetSpeeds>();

        private bool advertise10HD = false;

        private bool advertise10FD = false;

        private bool advertise100HD = false;

        private bool advertise100FD = false;

        private bool advertise1000HD = false;

        private bool advertise1000FD = false;

        private bool advertiseEEE100 = false;

        private bool advertiseEEE1000 = false;

        private bool downSpeed10Enabled = false;

        private bool downSpeed100Enabled = false;

        private uint downSpeedRetries = 0;

        private bool autoNegCompleted = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="NegotiationTargetSettings"/> class.
        /// </summary>
        public NegotiationTargetSettings()
        {
            this.ethernetSpeedStrings.Add(EthernetSpeeds.SPEED_1000BASE_T_FD, "1000BASE-T FD");
            this.ethernetSpeedStrings.Add(EthernetSpeeds.SPEED_1000BASE_T_HD, "1000BASE-T HD");
            this.ethernetSpeedStrings.Add(EthernetSpeeds.SPEED_1000BASE_T_EEE, "EEE 1000BASE-T");
            this.ethernetSpeedStrings.Add(EthernetSpeeds.SPEED_100BASE_TX_FD, "100BASE-TX FD");
            this.ethernetSpeedStrings.Add(EthernetSpeeds.SPEED_100BASE_TX_HD, "100BASE-TX HD");
            this.ethernetSpeedStrings.Add(EthernetSpeeds.SPEED_100BASE_TX_EEE, "EEE 100BASE-TX");
            this.ethernetSpeedStrings.Add(EthernetSpeeds.SPEED_10BASE_T_1L, "10BASE-T1L");//SPEED_10BASE_T_FD
            this.ethernetSpeedStrings.Add(EthernetSpeeds.SPEED_10BASE_T_HD, "10BASE-T HD");
        }

        /// <summary>
        /// Gets or sets a value indicating whether local auto negotiation is completed
        /// </summary>
        public bool AutoNegCompleted
        {
            get
            {
                return this.autoNegCompleted;
            }

            set
            {
                this.HandledChangedProperty("AutoNegCompleted", ref this.autoNegCompleted, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating
        /// </summary>
        public MasterSlavePreference PreferMasterSlave
        {
            get
            {
                return this.preferMasterSlave;
            }

            set
            {
                if (this.preferMasterSlave != value)
                {
                    string propertyName = "PreferMasterSlave";
                    this.preferMasterSlave = value;

                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating
        /// </summary>
        public SignalPeakToPeakVoltage PkPkVoltage
        {
            get
            {
                return this.pkpkVoltage;
            }

            set
            {
                if (this.pkpkVoltage != value)
                {
                    string propertyName = "PkPkVoltage";
                    this.pkpkVoltage = value;

                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating
        /// </summary>
        public MasterSlaveNegotiate NegotiateMasterSlave
        {
            get
            {
                return this.negotiateMasterSlave;
            }

            set
            {
                if (this.negotiateMasterSlave != value)
                {
                    string propertyName = "NegotiateMasterSlave";
                    this.negotiateMasterSlave = value;

                    this.RaisePropertyChanged(propertyName);
                }
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether advertised options would allow a downspeed to 10
        /// Need a 10 option advertised AND a higher speed to allow a downspeed to 100
        /// </summary>
        public bool DownspeedTo10Possible
        {
            get
            {
                return (this.Advertise10HD || this.Advertise10FD) &&
                    (this.Advertise1000HD ||
                    this.Advertise1000FD ||
                    this.Advertise100FD ||
                    this.Advertise100HD ||
                    this.AdvertiseEEE100 ||
                    this.AdvertiseEEE1000);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how many retries at a higher speed before downspeed
        /// </summary>
        public uint DownSpeedRetries
        {
            get
            {
                return this.downSpeedRetries;
            }

            set
            {
                if (this.downSpeedRetries != value)
                {
                    string propertyName = "DownSpeedRetries";
                    this.downSpeedRetries = value;
                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 10-BASE-T is enabled.
        /// </summary>
        public List<EthernetSpeeds> LocalAdvSpeeds
        {
            get
            {
                return this.localAdvSpeeds;
            }

            set
            {
                if (!this.IdenticalList(this.localAdvSpeeds, value))
                {
                    this.localAdvSpeeds = value;
                    this.RaisePropertyChanged("LocalAdvSpeeds");
                    this.RaisePropertyChanged("LocalAdvSpeedsStrings");
                }
            }
        }

        /// <summary>
        /// Gets locally advertised speeds in aligned string format
        /// </summary>
        public List<string> LocalAdvSpeedsStrings
        {
            get
            {
                return this.AdvertisedSpeedList(this.localAdvSpeeds);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 10-BASE-T is enabled.
        /// </summary>
        public bool DownSpeed10Enabled
        {
            get
            {
                return this.downSpeed10Enabled;
            }

            set
            {
                this.HandledChangedProperty("DownSpeed10Enabled", ref this.downSpeed10Enabled, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 100-BASE-T is enabled.
        /// </summary>
        public bool DownSpeed100Enabled
        {
            get
            {
                return this.downSpeed100Enabled;
            }

            set
            {
                this.HandledChangedProperty("DownSpeed100Enabled", ref this.downSpeed100Enabled, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 10-BASE-T is enabled.
        /// </summary>
        public List<EthernetSpeeds> RemoteAdvSpeeds
        {
            get
            {
                return this.remoteAdvSpeeds;
            }

            set
            {
                if (!this.IdenticalList(this.remoteAdvSpeeds, value))
                {
                    this.remoteAdvSpeeds = value;
                    this.RaisePropertyChanged("RemoteAdvSpeeds");
                    this.RaisePropertyChanged("RemoteAdvSpeedsStrings");
                }
            }
        }

        /// <summary>
        /// Gets remotely advertised speeds in aligned string format
        /// </summary>
        public List<string> RemoteAdvSpeedsStrings
        {
            get
            {
                return this.AdvertisedSpeedList(this.remoteAdvSpeeds);
            }
        }

        /// <summary>
        /// Gets a value indicating whether advertised options would allow a downspeed to 100
        /// Need a 100 option advertised AND a higher speed to allow a downspeed to 100
        /// </summary>
        public bool DownspeedTo100Possible
        {
            get
            {
                return (this.AdvertiseEEE100 || this.Advertise100FD || this.Advertise100HD) && (
                    this.Advertise1000HD ||
                    this.Advertise1000FD ||
                    this.AdvertiseEEE100);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether 10HD is advertised
        /// </summary>
        public bool Advertise10HD
        {
            get
            {
                return this.advertise10HD;
            }

            set
            {
                this.HandledChangedProperty("Advertise10HD", ref this.advertise10HD, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether 10FD is advertised
        /// </summary>
        public bool Advertise10FD
        {
            get
            {
                return this.advertise10FD;
            }

            set
            {
                this.HandledChangedProperty("Advertise10FD", ref this.advertise10FD, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether 100HD  is advertised
        /// </summary>
        public bool Advertise100HD
        {
            get
            {
                return this.advertise100HD;
            }

            set
            {
                this.HandledChangedProperty("Advertise100HD", ref this.advertise100HD, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether 100FD  is advertised
        /// </summary>
        public bool Advertise100FD
        {
            get
            {
                return this.advertise100FD;
            }

            set
            {
                this.HandledChangedProperty("Advertise100FD", ref this.advertise100FD, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether 1000 HD  is advertised
        /// </summary>
        public bool Advertise1000HD
        {
            get
            {
                return this.advertise1000HD;
            }

            set
            {
                this.HandledChangedProperty("Advertise1000HD", ref this.advertise1000HD, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether 1000FD  is advertised
        /// </summary>
        public bool Advertise1000FD
        {
            get
            {
                return this.advertise1000FD;
            }

            set
            {
                this.HandledChangedProperty("Advertise1000FD", ref this.advertise1000FD, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether EEE 100  is advertised
        /// </summary>
        public bool AdvertiseEEE100
        {
            get
            {
                return this.advertiseEEE100;
            }

            set
            {
                this.HandledChangedProperty("AdvertiseEEE100", ref this.advertiseEEE100, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether EEE1000 is advertised
        /// </summary>
        public bool AdvertiseEEE1000
        {
            get
            {
                return this.advertiseEEE1000;
            }

            set
            {
                this.HandledChangedProperty("AdvertiseEEE1000", ref this.advertiseEEE1000, value);
            }
        }

        /// <summary>
        /// Clear the property changed list.
        /// </summary>
        public override void FlagAllPropertiesChanged()
        {
            var classType = typeof(NegotiationTargetSettings);
            PropertyInfo[] myPropertyInfo;

            myPropertyInfo = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < myPropertyInfo.Length; i++)
            {
                this.NotifyPropertyChange(myPropertyInfo[i].Name);
            }
        }

        /// <summary>
        /// String for speed
        /// </summary>
        /// <param name="speed">Name of the proprty</param>
        /// <returns>Value of field</returns>
        public string SpeedString(EthernetSpeeds speed)
        {
            if (this.ethernetSpeedStrings.ContainsKey(speed))
            {
                return this.ethernetSpeedStrings[speed];
            }
            else
            {
                return speed.ToString();
            }
        }

        /// <summary>
        /// Get the value of a private boolean field by name
        /// </summary>
        /// <param name="field">Name of the proprty</param>
        /// <returns>Value of field</returns>
        protected FieldInfo GetFieldInfo(string field)
        {
            var classType = typeof(NegotiationTargetSettings);

            return classType.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private List<string> AdvertisedSpeedList(List<EthernetSpeeds> speedenums)
        {
            List<EthernetSpeeds> orderedAdvertisedSpeeds = new List<EthernetSpeeds>
            {
                EthernetSpeeds.SPEED_1000BASE_T_FD,
            EthernetSpeeds.SPEED_1000BASE_T_HD, EthernetSpeeds.SPEED_1000BASE_T_EEE, EthernetSpeeds.SPEED_100BASE_TX_FD, EthernetSpeeds.SPEED_100BASE_TX_HD,
             EthernetSpeeds.SPEED_100BASE_TX_EEE, EthernetSpeeds.SPEED_10BASE_T_1L, EthernetSpeeds.SPEED_10BASE_T_HD//.SPEED_10BASE_T_FD
            };

            List<string> speeds = new List<string>();

            foreach (var orderedAdvertisedSpeed in orderedAdvertisedSpeeds)
            {
                if (speedenums.Contains(orderedAdvertisedSpeed))
                {
                    speeds.Add(this.SpeedString(orderedAdvertisedSpeed));
                }
                else
                {
                    speeds.Add(string.Empty);
                }
            }

            return speeds;
        }
    }
}
