// <copyright file="FixedTargetSettings.cs" company="Analog Devices, Inc.">
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
    public class FixedTargetSettings : Settings
    {
        private EthernetSpeeds forcedSpeed = EthernetSpeeds.SPEED_10BASE_T_HD;

        private MasterSlaveFixed fixedMasterSlave = MasterSlaveFixed.Master;

        /// <summary>
        /// Gets or sets a value indicating
        /// </summary>
        public EthernetSpeeds ForcedSpeed
        {
            get
            {
                return this.forcedSpeed;
            }

            set
            {
                if (this.forcedSpeed != value)
                {
                    string propertyName = "ForcedSpeed";
                    this.forcedSpeed = value;
                    if (!this.propertiesChangedList.Contains(propertyName))
                    {
                        this.propertiesChangedList.Add(propertyName);
                    }

                    this.RaisePropertyChanged(propertyName);

                    propertyName = "ForcedSpeedProxy";
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
        public MasterSlaveFixed FixedMasterSlave
        {
            get
            {
                return this.fixedMasterSlave;
            }

            set
            {
                if (this.fixedMasterSlave != value)
                {
                    string propertyName = "FixedMasterSlave";
                    this.fixedMasterSlave = value;
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
        /// This can be done more elegently when I understand XANL better!!!
        /// </summary>
        public EthernetSpeedsForced ForcedSpeedProxy
        {
            get
            {
                EthernetSpeedsForced forcedSpeedProxy = EthernetSpeedsForced.SPEED_10BASE_T_HD;
                switch (this.ForcedSpeed)
                {
                    case EthernetSpeeds.SPEED_10BASE_T_HD:
                        forcedSpeedProxy = EthernetSpeedsForced.SPEED_10BASE_T_HD;
                        break;
                    case EthernetSpeeds.SPEED_10BASE_T_1L://dani 20april SPEED_10BASE_T_FD:
                        forcedSpeedProxy = EthernetSpeedsForced.SPEED_10BASE_T_1L;//dani20April_FD;
                        break;
                    case EthernetSpeeds.SPEED_100BASE_TX_HD:
                        forcedSpeedProxy = EthernetSpeedsForced.SPEED_100BASE_TX_HD;
                        break;
                    case EthernetSpeeds.SPEED_100BASE_TX_FD:
                        forcedSpeedProxy = EthernetSpeedsForced.SPEED_100BASE_TX_FD;
                        break;
                    //case EthernetSpeeds.SPEED_1000BASE_T_FD:
                    //    forcedSpeedProxy = EthernetSpeedsForced.SPEED_1000BASE_T_FD;
                    //    break;
                }

                return forcedSpeedProxy;
            }

            set
            {
                EthernetSpeeds forcedSpeed = EthernetSpeeds.SPEED_10BASE_T_HD;

                switch (value)
                {
                    case EthernetSpeedsForced.SPEED_10BASE_T_HD:
                        forcedSpeed = EthernetSpeeds.SPEED_10BASE_T_HD;
                        break;
                    case EthernetSpeedsForced.SPEED_10BASE_T_1L://dani 20 april SPEED_10BASE_T_FD:
                        forcedSpeed = EthernetSpeeds.SPEED_10BASE_T_1L;// SPEED_10BASE_T_FD;
                        break;
                    case EthernetSpeedsForced.SPEED_100BASE_TX_HD:
                        forcedSpeed = EthernetSpeeds.SPEED_100BASE_TX_HD;
                        break;
                    case EthernetSpeedsForced.SPEED_100BASE_TX_FD:
                        forcedSpeed = EthernetSpeeds.SPEED_100BASE_TX_FD;
                        break;

                    // case EthernetSpeedsForced.SPEED_1000BASE_T_FD:
                    //    forcedSpeed = EthernetSpeeds.SPEED_1000BASE_T_FD;
                    //    break;
                }

                this.ForcedSpeed = forcedSpeed;
            }
        }

        /// <summary>
        /// Clear the property changed list.
        /// </summary>
        public override void FlagAllPropertiesChanged()
        {
            var classType = typeof(FixedTargetSettings);
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
            var classType = typeof(FixedTargetSettings);

            return classType.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
