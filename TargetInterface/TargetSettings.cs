// <copyright file="TargetSettings.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace TargetInterface
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using static TargetInterface.FirmwareAPI;

    /// <summary>
    /// Target Settings
    /// </summary>
    public class TargetSettings : Settings
    {
        private List<string> cableDiagnosticsStatus = new List<string>() { "Please run cable diagnostics for updated status." };

        private TargetInfoItem frameCheckerStatus = new TargetInfoItem("Checker:");

        private TargetInfoItem frameGeneratorStatus = new TargetInfoItem("Generator:");

        private TargetInfoItem detectedDevice = new TargetInfoItem("Device Type:");

        private EthPhyState phyState = EthPhyState.Powerdown;

        private NegotiationTargetSettings negotiate = new NegotiationTargetSettings();

        private FixedTargetSettings fixedsettings = new FixedTargetSettings();

        private LinkTargetSettings link = new LinkTargetSettings();

        private EnergyPowerDownMode ePDMode;

        private AutoMdixMode mdixMode = AutoMdixMode.Auto;

        private EthSpeedMode ethSpeedSelection = EthSpeedMode.Advertised;

        private DeviceType connectedDeviceType = DeviceType.ADIN1100;//dani

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetSettings"/> class.
        /// </summary>
        public TargetSettings()
        {
            this.Negotiate.PropertyChanged += this.NegotiationTargetSettings_PropertyChanged;
            this.Fixed.PropertyChanged += this.FixedTargetSettings_PropertyChanged;
            this.Link.PropertyChanged += this.LinkTargetSettings_PropertyChanged;
            this.FlagAllPropertiesChanged();
        }

        /// <summary>
        /// Gets or sets Frame Checker Status
        /// </summary>
        public TargetInfoItem FrameCheckerStatus
        {
            get
            {
                return this.frameCheckerStatus;
            }

            set
            {
                if (!this.frameCheckerStatus.Equals(value))
                {
                    string propertyName = "FrameCheckerStatus";
                    this.frameCheckerStatus = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets Frame Generator Status
        /// </summary>
        public TargetInfoItem FrameGeneratorStatus
        {
            get
            {
                return this.frameGeneratorStatus;
            }

            set
            {
                if (!this.frameGeneratorStatus.Equals(value))
                {
                    string propertyName = "FrameGeneratorStatus";
                    this.frameGeneratorStatus = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Detected Device
        /// </summary>
        public TargetInfoItem DetectedDevice
        {
            get
            {
                return this.detectedDevice;
            }

            set
            {
                if (!this.detectedDevice.Equals(value))
                {
                    string propertyName = "DetectedDevice";
                    this.detectedDevice = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets connected Device Type
        /// </summary>
        public DeviceType ConnectedDeviceType
        {
            get
            {
                return this.connectedDeviceType;
            }

            set
            {
                if (this.connectedDeviceType != value)
                {
                    string propertyName = "ConnectedDeviceType";
                    this.connectedDeviceType = value;
                    if (!this.propertiesChangedList.Contains(propertyName))
                    {
                        this.propertiesChangedList.Add(propertyName);
                    }

                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets Phy state
        /// </summary>
        public EthPhyState PhyState
        {
            get
            {
                return this.phyState;
            }

            set
            {
                if (this.phyState != value)
                {
                    string propertyName = "PhyState";
                    this.phyState = value;
                    if (!this.propertiesChangedList.Contains(propertyName))
                    {
                        this.propertiesChangedList.Add(propertyName);
                    }

                    this.RaisePropertyChanged(propertyName);
                    this.RaisePropertyChanged("InSoftwarePowerDown");
                    this.RaisePropertyChanged("InStandBy");
                }
            }
        }

        /// <summary>
        /// Gets or sets Cable Diagnostics Status
        /// </summary>
        public List<string> CableDiagnosticsStatus
        {
            get
            {
                return this.cableDiagnosticsStatus;
            }

            set
            {
                if (!this.cableDiagnosticsStatus.SequenceEqual(value))
                {
                    string propertyName = "CableDiagnosticsStatus";
                    this.cableDiagnosticsStatus = value;
                    if (!this.propertiesChangedList.Contains(propertyName))
                    {
                        this.propertiesChangedList.Add(propertyName);
                    }

                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets link negotiation property settings
        /// </summary>
        public NegotiationTargetSettings Negotiate
        {
            get
            {
                return this.negotiate;
            }
        }

        /// <summary>
        /// Gets fixed negotiation property settings
        /// </summary>
        public FixedTargetSettings Fixed
        {
            get
            {
                return this.fixedsettings;
            }
        }

        /// <summary>
        /// Gets link property settings
        /// </summary>
        public LinkTargetSettings Link
        {
            get
            {
                return this.link;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating
        /// </summary>
        public EnergyPowerDownMode EPDMode
        {
            get
            {
                return this.ePDMode;
            }

            set
            {
                if (this.ePDMode != value)
                {
                    string propertyName = "EPDMode";
                    this.ePDMode = value;
                    if (!this.propertiesChangedList.Contains(propertyName))
                    {
                        this.propertiesChangedList.Add(propertyName);
                    }

                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating
        /// </summary>
        public AutoMdixMode MdixMode
        {
            get
            {
                return this.mdixMode;
            }

            set
            {
                if (this.mdixMode != value)
                {
                    string propertyName = "MdixMode";
                    this.mdixMode = value;
                    if (!this.propertiesChangedList.Contains(propertyName))
                    {
                        this.propertiesChangedList.Add(propertyName);
                    }

                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating
        /// </summary>
        public EthSpeedMode EthSpeedSelection
        {
            get
            {
                return this.ethSpeedSelection;
            }

            set
            {
                if (this.ethSpeedSelection != value)
                {
                    string propertyName = "EthSpeedSelection";
                    this.ethSpeedSelection = value;
                    if (!this.propertiesChangedList.Contains(propertyName))
                    {
                        this.propertiesChangedList.Add(propertyName);
                    }

                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether device is in software pwoerdown
        /// </summary>
        public bool InSoftwarePowerDown
        {
            get
            {
                return this.phyState == EthPhyState.Powerdown;
            }

            set
            {
                string propertyName = "InSoftwarePowerDown";
                if (!this.propertiesChangedList.Contains(propertyName))
                {
                    this.propertiesChangedList.Add(propertyName);
                }

                this.RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether device is in standby state
        /// </summary>
        public bool InStandBy
        {
            get
            {
                return this.phyState == EthPhyState.Standby;
            }
        }

        /// <summary>
        /// We have just read some settings from the DUT..merge it
        /// </summary>
        /// <param name="status">The values read from the DUT</param>
        public void MergeStatusSettings(TargetSettings status)
        {
            string listproperty;
            if (status.PropertiesChangedList.Contains("PhyState"))
            {
                Trace.WriteLine("PHY State: " + status.PhyState.ToString());
            }

            listproperty = "PhyState";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.PhyState = status.PhyState;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "Advertise10HD";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.Advertise10HD = status.Negotiate.Advertise10HD;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "Advertise10FD";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.Advertise10FD = status.Negotiate.Advertise10FD;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "Advertise100HD";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.Advertise100HD = status.Negotiate.Advertise100HD;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "Advertise100FD";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.Advertise100FD = status.Negotiate.Advertise100FD;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "Advertise1000HD";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.Advertise1000HD = status.Negotiate.Advertise1000HD;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "Advertise1000FD";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.Advertise1000FD = status.Negotiate.Advertise1000FD;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "AdvertiseEEE100";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.AdvertiseEEE100 = status.Negotiate.AdvertiseEEE100;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "AdvertiseEEE1000";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.AdvertiseEEE1000 = status.Negotiate.AdvertiseEEE1000;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "EPDMode";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.EPDMode = status.EPDMode;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "MdixMode";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.MdixMode = status.MdixMode;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "EthSpeedSelection";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.EthSpeedSelection = status.EthSpeedSelection;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "ConnectedDeviceType";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.ConnectedDeviceType = status.ConnectedDeviceType;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "LocalRcvrOk";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.LocalRcvrOk = status.Link.LocalRcvrOk;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "RemoteRcvrOk";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.RemoteRcvrOk = status.Link.RemoteRcvrOk;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "FreqOffsetPpm";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.FreqOffsetPpm = status.Link.FreqOffsetPpm;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "CableLength";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.CableLength = status.Link.CableLength;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "PairMeanSquareError";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.PairMeanSquareError = status.Link.PairMeanSquareError;
                this.PropertiesChangedList.Remove(listproperty);
            }
			
			listproperty = "MasterSlaveStatus";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.MasterSlaveStatus = status.Link.MasterSlaveStatus;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "AnStatus";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.AnStatus = status.Link.AnStatus;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "MseValue";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.MseValue = status.Link.MseValue;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "CableVoltage";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.CableVoltage = status.Link.CableVoltage;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "PairMeanSquareErrorStats";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.PairMeanSquareErrorStats = status.Link.PairMeanSquareErrorStats;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "CableDiagnosticsStatus";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.CableDiagnosticsStatus = status.CableDiagnosticsStatus;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "FrameGeneratorStatus";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.FrameGeneratorStatus = status.FrameGeneratorStatus;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "DetectedDevice";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.DetectedDevice = status.DetectedDevice;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "FrameCheckerStatus";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.FrameCheckerStatus = status.FrameCheckerStatus;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "ResolvedHCD";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.ResolvedHCD = status.Link.ResolvedHCD;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "AutoNegCompleted";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.AutoNegCompleted = status.Negotiate.AutoNegCompleted;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "PreferMasterSlave";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.PreferMasterSlave = status.Negotiate.PreferMasterSlave;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "PkPkVoltage";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.PkPkVoltage = status.Negotiate.PkPkVoltage;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "NegotiateMasterSlave";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.NegotiateMasterSlave = status.Negotiate.NegotiateMasterSlave;
                this.PropertiesChangedList.Remove(listproperty);
            }
			
            /* Local and remote advertised settings */
            listproperty = "LocalAdvSpeeds";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.LocalAdvSpeeds = status.Negotiate.LocalAdvSpeeds;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "RemoteAdvSpeeds";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.RemoteAdvSpeeds = status.Negotiate.RemoteAdvSpeeds;
                this.PropertiesChangedList.Remove(listproperty);
            }

            /* Downspeed settings */
            listproperty = "DownSpeed10Enabled";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.DownSpeed10Enabled = status.Negotiate.DownSpeed10Enabled;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "DownSpeed100Enabled";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.DownSpeed100Enabled = status.Negotiate.DownSpeed100Enabled;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "DownSpeedRetries";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Negotiate.DownSpeedRetries = status.Negotiate.DownSpeedRetries;
                this.PropertiesChangedList.Remove(listproperty);
            }

			listproperty = "FixedMasterSlave";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Fixed.FixedMasterSlave = status.Fixed.FixedMasterSlave;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "ForcedSpeed";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Fixed.ForcedSpeed = status.Fixed.ForcedSpeed;
                this.PropertiesChangedList.Remove(listproperty);
            }

            /* Dump these if they exist, they are not interesting */
            listproperty = "Negotiate";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "Fixed";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "Link";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "PropertiesChangedList";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "DownspeedTo100Possible";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "DownspeedTo10Possible";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "InSoftwarePowerDown";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.InSoftwarePowerDown = status.InSoftwarePowerDown;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "InEnergyPowerDown";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.InEnergyPowerDown = status.Link.InEnergyPowerDown;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "FrameGenRunning";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.FrameGenRunning = status.Link.FrameGenRunning;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "LinkingEnabled";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.LinkingEnabled = status.Link.LinkingEnabled;
                this.PropertiesChangedList.Remove(listproperty);
            }

            listproperty = "LinkEstablished";
            if (status.PropertiesChangedList.Contains(listproperty))
            {
                status.PropertiesChangedList.Remove(listproperty);
                this.Link.LinkEstablished = status.Link.LinkEstablished;
                this.PropertiesChangedList.Remove(listproperty);
            }

            /* Anything not handled? */
            foreach (var property in status.PropertiesChangedList)
            {
                Trace.WriteLine(property);
            }

            status.ClearPropertiesChangedList();
        }

        /// <summary>
        /// Clear the property changed list.
        /// </summary>
        public override void FlagAllPropertiesChanged()
        {
            var classType = typeof(TargetSettings);
            PropertyInfo[] myPropertyInfo;

            myPropertyInfo = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < myPropertyInfo.Length; i++)
            {
                this.NotifyPropertyChange(myPropertyInfo[i].Name);
            }

            this.Link.FlagAllPropertiesChanged();
            this.Negotiate.FlagAllPropertiesChanged();
            this.Link.FlagAllPropertiesChanged();
        }

        /// <summary>
        /// Clear the property changed list.
        /// </summary>
        public override void ClearPropertiesChangedList()
        {
            this.propertiesChangedList.Clear();
            this.Negotiate.ClearPropertiesChangedList();
            this.Link.ClearPropertiesChangedList();
            this.Fixed.ClearPropertiesChangedList();
        }

        /// <summary>
        /// Negotiation sub-settings is flagging a change.
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void NegotiationTargetSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChange(e.PropertyName);
        }

        /// <summary>
        /// Fixed sub-settings is flagging a change.
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void FixedTargetSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChange(e.PropertyName);
        }

        /// <summary>
        /// Link sub-settings is flagging a change.
        /// </summary>
        /// <param name="sender">The sender instance</param>
        /// <param name="e">Property Changes arguments</param>
        public virtual void LinkTargetSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.NotifyPropertyChange(e.PropertyName);
        }
    }
}
