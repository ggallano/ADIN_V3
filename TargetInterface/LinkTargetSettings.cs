// <copyright file="LinkTargetSettings.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace TargetInterface
{
    using System.Collections.Generic;
    using System.Reflection;
    using static FirmwareAPI;

    /// <summary>
    /// Link Target Settings
    /// </summary>
    public class LinkTargetSettings : Settings
    {
        private EthernetSpeeds resolvedHCD;

        private bool frameGenRunning = false;

        private bool linkEstablished = false;

        private bool linkingEnabled = false;

        private bool inEnergyPowerDown = false;

        private double freqOffsetPpm = 0.0;

        private bool localRcvrOk = false;

        private bool remoteRcvrOk = false;

        private TargetInfoItem cableLength = new TargetInfoItem("Cable Length:");

        private TargetInfoItem pairMeanSquareError = new TargetInfoItem("Mean Square Error:");

        private TargetInfoItem pairMeanSquareErrorStats = new TargetInfoItem("Mean Square Error:");

        private TargetInfoItem masterSlaveStatus = new TargetInfoItem("Master\\Slave:");

        private TargetInfoItem anStatus = new TargetInfoItem("AN Status:");// dani merge Glenn new textField

        private TargetInfoItem cableVoltage = new TargetInfoItem("Tx Level:");//old VableVoltage

        private TargetInfoItem mseValue = new TargetInfoItem("MSE:");

        /// <summary>
        /// Gets or sets a value indicating whether the frame generator is running or not
        /// </summary>
        public bool FrameGenRunning
        {
            get
            {
                return this.frameGenRunning;
            }

            set
            {
                this.HandledChangedProperty("FrameGenRunning", ref this.frameGenRunning, value);
            }
        }

        /// <summary>
        /// Gets or sets the MSE Value
        /// </summary>
        public TargetInfoItem MseValue
        {
            get
            {
                return this.mseValue;
            }

            set
            {
                if (!this.mseValue.Equals(value))
                {
                    string propertyName = "MseValue";
                    this.mseValue = value;
                    this.RaisePropertyChanged(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets AN Status item
        /// </summary>
        public TargetInfoItem AnStatus
        {
            get
            {
                return this.anStatus;
            }

            set
            {
                if (!this.anStatus.Equals(value))
                {
                    string propertyName = "AnStatus";
                    this.anStatus = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 10-BASE-T is enabled.
        /// </summary>
        public bool LocalRcvrOk
        {
            get
            {
                return this.localRcvrOk;
            }

            set
            {
                this.HandledChangedProperty("LocalRcvrOk", ref this.localRcvrOk, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 10-BASE-T is enabled.
        /// </summary>
        public bool RemoteRcvrOk
        {
            get
            {
                return this.remoteRcvrOk;
            }

            set
            {
                this.HandledChangedProperty("RemoteRcvrOk", ref this.remoteRcvrOk, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 10-BASE-T is enabled.
        /// </summary>
        public double FreqOffsetPpm
        {
            get
            {
                if ((this.ResolvedHCD >= EthernetSpeeds.SPEED_100BASE_TX_HD) && (this.ResolvedHCD <= EthernetSpeeds.SPEED_1000BASE_T_FD))
                {
                    return this.freqOffsetPpm;
                }
                else
                {
                    return 0.0;
                }
            }

            set
            {
                if (this.freqOffsetPpm != value)
                {
                    string propertyName = "FreqOffsetPpm";
                    this.freqOffsetPpm = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets Cable Length information item
        /// </summary>
        public TargetInfoItem CableLength
        {
            get
            {
                return this.cableLength;
            }

            set
            {
                if (!this.cableLength.Equals(value))
                {
                    string propertyName = "CableLength";
                    this.cableLength = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets Pair Mean Square Error item
        /// </summary>
        public TargetInfoItem PairMeanSquareError
        {
            get
            {
                return this.pairMeanSquareError;
            }

            set
            {
                if (!this.pairMeanSquareError.Equals(value))
                {
                    string propertyName = "PairMeanSquareError";
                    this.pairMeanSquareError = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets Pair Mean Square Error Stats item
        /// </summary>
        public TargetInfoItem PairMeanSquareErrorStats
        {
            get
            {
                return this.pairMeanSquareErrorStats;
            }

            set
            {
                if (!this.pairMeanSquareErrorStats.Equals(value))
                {
                    string propertyName = "PairMeanSquareErrorStats";
                    this.pairMeanSquareErrorStats = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets Master Slave Status Stats item
        /// </summary>
        public TargetInfoItem MasterSlaveStatus
        {
            get
            {
                return this.masterSlaveStatus;
            }

            set
            {
                if (!this.masterSlaveStatus.Equals(value))
                {
                    string propertyName = "MasterSlaveStatus";
                    this.masterSlaveStatus = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets Cable Voltage Stats item
        /// </summary>
        public TargetInfoItem CableVoltage
        {
            get
            {
                return this.cableVoltage;
            }

            set
            {
                if (!this.cableVoltage.Equals(value))
                {
                    string propertyName = "CableVoltage";
                    this.cableVoltage = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 10-BASE-T is enabled.
        /// </summary>
        public bool LinkEstablished
        {
            get
            {
                return this.linkEstablished;
            }

            set
            {
                this.HandledChangedProperty("LinkEstablished", ref this.linkEstablished, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether downspeed to 10-BASE-T is enabled.
        /// </summary>
        public bool InEnergyPowerDown
        {
            get
            {
                return this.inEnergyPowerDown;
            }

            set
            {
                this.HandledChangedProperty("InEnergyPowerDown", ref this.inEnergyPowerDown, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether linking is enabled or not
        /// </summary>
        public bool LinkingEnabled
        {
            get
            {
                return this.linkingEnabled;
            }

            set
            {
                this.HandledChangedProperty("LinkingEnabled", ref this.linkingEnabled, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether 10HD is advertised
        /// </summary>
        public EthernetSpeeds ResolvedHCD
        {
            get
            {
                return this.resolvedHCD;
            }

            set
            {
                if (this.resolvedHCD != value)
                {
                    string propertyName = "ResolvedHCD";
                    this.resolvedHCD = value;
                    this.NotifyPropertyChange(propertyName);
                }
            }
        }

        /// <summary>
        /// Clear the property changed list.
        /// </summary>
        public override void FlagAllPropertiesChanged()
        {
            var classType = typeof(LinkTargetSettings);
            PropertyInfo[] myPropertyInfo;

            myPropertyInfo = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < myPropertyInfo.Length; i++)
            {
                this.NotifyPropertyChange(myPropertyInfo[i].Name);
            }
        }

        /// <summary>
        /// Get the value of a private boolean field by name
        /// </summary>
        /// <param name="field">Name of the proprty</param>
        /// <returns>Value of field</returns>
        protected FieldInfo GetFieldInfo(string field)
        {
            var classType = typeof(LinkTargetSettings);

            return classType.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
