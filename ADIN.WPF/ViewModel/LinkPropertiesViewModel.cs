// <copyright file="LinkPropertiesViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using System.Collections.Generic;

namespace ADIN.WPF.ViewModel
{
    public class LinkPropertiesViewModel : ViewModelBase
    {
        private NavigationStore _navigationStore;
        private SelectedDeviceStore _selectedDeviceStore;

        public LinkPropertiesViewModel(NavigationStore navigationStore, SelectedDeviceStore selectedDeviceStore)
        {
            _navigationStore = navigationStore;
            _selectedDeviceStore = selectedDeviceStore;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public List<string> AdvertisedSpeeds => _linkProperties.AdvertisedSpeeds;

        public List<string> EnergyDetectPowerDownModes => _linkProperties?.EnergyDetectPowerDownModes;

        public List<string> ForcedSpeeds => _linkProperties?.ForcedSpeeds;

        public bool IsAdvertise_1000BASE_T_FD
        {
            get
            {
                return _linkProperties?.IsAdvertise_1000BASE_T_FD == true;
            }

            set
            {
                if (value != _linkProperties.IsAdvertise_1000BASE_T_FD)
                {
                    _linkProperties.IsAdvertise_1000BASE_T_FD = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.Speed1000FdAdvertisement(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.Speed1000FdAdvertisement(value);
                    }

                    if (value)
                    {
                        _linkProperties.AdvertisedSpeeds.Add("SPEED_1000BASE_T_FD_SPEED");
                    }
                    else
                    {
                        _linkProperties.AdvertisedSpeeds.Remove("SPEED_1000BASE_T_FD_SPEED");
                    }

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                    }

                    OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_FD));
                    OnPropertyChanged(nameof(IsEEE1000Enabled));
                    OnPropertyChanged(nameof(IsDownspeed100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                    OnPropertyChanged(nameof(IsMasterSlavePrefVisible));
                }
            }
        }

        public bool IsAdvertise_1000BASE_T_HD
        {
            get
            {
                return _linkProperties?.IsAdvertise_1000BASE_T_HD == true;
            }

            set
            {
                _linkProperties.IsAdvertise_1000BASE_T_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.Speed1000HdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.Speed1000HdAdvertisement(value);
                }

                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_1000BASE_T_HD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_1000BASE_T_HD_SPEED");
                }

                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }

                OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_HD));
                OnPropertyChanged(nameof(IsDownspeed100Enabled));
                OnPropertyChanged(nameof(IsDownspeed10Enabled));
                OnPropertyChanged(nameof(IsMasterSlavePrefVisible));
            }
        }

        public bool IsAdvertise_100BASE_TX_FD
        {
            get
            {
                return _linkProperties?.IsAdvertise_100BASE_TX_FD == true;
            }

            set
            {
                _linkProperties.IsAdvertise_100BASE_TX_FD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.Speed100FdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.Speed100FdAdvertisement(value);
                }

                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_100BASE_TX_FD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_100BASE_TX_FD_SPEED");
                }

                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }

                OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_FD));
                OnPropertyChanged(nameof(IsEEE100Enabled));
                OnPropertyChanged(nameof(IsDownspeed100Enabled));
                OnPropertyChanged(nameof(IsDownspeed10Enabled));
            }
        }

        public bool IsAdvertise_100BASE_TX_HD
        {
            get
            {
                return _linkProperties?.IsAdvertise_100BASE_TX_HD == true;
            }

            set
            {
                _linkProperties.IsAdvertise_100BASE_TX_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.Speed100HdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.Speed100HdAdvertisement(value);
                }

                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_100BASE_TX_HD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_100BASE_TX_HD_SPEED");
                }

                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }

                OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_HD));
                OnPropertyChanged(nameof(IsDownspeed100Enabled));
                OnPropertyChanged(nameof(IsDownspeed10Enabled));
            }
        }

        public bool IsAdvertise_10BASE_T_FD
        {
            get
            {
                return _linkProperties?.IsAdvertise_10BASE_T_FD == true;
            }

            set
            {
                _linkProperties.IsAdvertise_10BASE_T_FD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.Speed10FdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.Speed10FdAdvertisement(value);
                }

                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_10BASE_T_FD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_10BASE_T_FD_SPEED");
                }

                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }

                OnPropertyChanged(nameof(IsAdvertise_10BASE_T_FD));
                OnPropertyChanged(nameof(IsDownspeed10Enabled));
            }
        }

        public bool IsAdvertise_10BASE_T_HD
        {
            get
            {
                return _linkProperties?.IsAdvertise_10BASE_T_HD == true;
            }

            set
            {
                _linkProperties.IsAdvertise_10BASE_T_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.Speed10HdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.Speed10HdAdvertisement(value);
                }

                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_10BASE_T_HD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_10BASE_T_HD_SPEED");
                }

                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }

                OnPropertyChanged(nameof(IsAdvertise_10BASE_T_HD));
                OnPropertyChanged(nameof(IsDownspeed10Enabled));
            }
        }

        public bool IsAdvertise_EEE_1000BASE_T
        {
            get
            {
                return _linkProperties?.IsAdvertise_EEE_1000BASE_T == true;
            }

            set
            {
                _linkProperties.IsAdvertise_EEE_1000BASE_T = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.Speed1000EEEAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.Speed1000EEEAdvertisement(value);
                }

                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_1000BASE_EEE_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_1000BASE_EEE_SPEED");
                }

                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }

                OnPropertyChanged(nameof(IsAdvertise_EEE_1000BASE_T));
                OnPropertyChanged(nameof(IsDownspeed100Enabled));
                OnPropertyChanged(nameof(IsDownspeed10Enabled));
            }
        }

        public bool IsAdvertise_EEE_100BASE_TX
        {
            get
            {
                return _linkProperties?.IsAdvertise_EEE_100BASE_TX == true;
            }

            set
            {
                _linkProperties.IsAdvertise_EEE_100BASE_TX = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.Speed100EEEAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.Speed100EEEAdvertisement(value);
                }

                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_100BASE_EEE_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_100BASE_EEE_SPEED");
                }

                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                }

                OnPropertyChanged(nameof(IsAdvertise_EEE_100BASE_TX));
                OnPropertyChanged(nameof(IsDownspeed100Enabled));
                OnPropertyChanged(nameof(IsDownspeed10Enabled));
            }
        }

        public bool IsANAdvertised1GSpeedVisible
        {
            get { return (_linkProperties?.SpeedMode != "Forced") && (_linkProperties?.IsSpeedCapable1G != false); }
        }

        public bool IsANAdvertisedSpeedVisible
        {
            get { return _linkProperties?.SpeedMode != "Forced"; }
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        public bool IsDownSpeed_100BASE_TX_HD
        {
            get
            {
                return _linkProperties?.IsDownSpeed_100BASE_TX_HD == true;
            }

            set
            {
                _linkProperties.IsDownSpeed_100BASE_TX_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.DownSpeed100Hd(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.DownSpeed100Hd(value);
                }

                OnPropertyChanged(nameof(IsDownSpeed_100BASE_TX_HD));
            }
        }

        public bool IsDownSpeed_10BASE_T_HD
        {
            get
            {
                return _linkProperties?.IsDownSpeed_10BASE_T_HD == true;
            }

            set
            {
                _linkProperties.IsDownSpeed_10BASE_T_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.DownSpeed10Hd(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.DownSpeed10Hd(value);
                }

                OnPropertyChanged(nameof(IsDownSpeed_10BASE_T_HD));
            }
        }

        public bool IsDownspeed100Enabled
        {
            get
            {
                if ((IsANAdvertised1GSpeedVisible == true)
                    && (IsAdvertise_100BASE_TX_FD == true
                        || IsAdvertise_100BASE_TX_HD == true
                        || IsAdvertise_EEE_100BASE_TX == true))
                {
                    if (IsAdvertise_1000BASE_T_FD == true
                        || IsAdvertise_1000BASE_T_HD == true
                        || IsAdvertise_EEE_1000BASE_T == true)
                    { 
                        return true; 
                    }
                }

                return false;
            }
        }

        public bool IsDownspeed10Enabled
        {
            get
            {
                if (IsAdvertise_10BASE_T_FD == true
                    || IsAdvertise_10BASE_T_HD == true)
                {
                    if (IsAdvertise_1000BASE_T_FD == true
                        || IsAdvertise_1000BASE_T_HD == true
                        || IsAdvertise_EEE_1000BASE_T == true
                        || IsAdvertise_100BASE_TX_FD == true
                        || IsAdvertise_100BASE_TX_HD == true
                        || IsAdvertise_EEE_100BASE_TX == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsEEE1000Enabled
        {
            get { return (IsAdvertise_1000BASE_T_FD == true)
                    && (IsANAdvertised1GSpeedVisible == true); }
        }

        public bool IsEEE100Enabled
        {
            get { return IsAdvertise_100BASE_TX_FD == true; }
        }

        public bool IsForcedSpeedVisible
        {
            get { return _linkProperties?.SpeedMode == "Forced"; }
        }

        public bool IsMasterSlaveForcedVisible
        {
            get
            {
                return (IsGigabitBoard == true)
                    && (_linkProperties?.SpeedMode == "Forced")
                    && (_linkProperties?.ForcedSpeed == "SPEED_1000BASE_T_FD"); 
            }
        }

        public bool IsMasterSlavePrefVisible
        {
            get
            {
                return (IsGigabitBoard == true)
                    && (_linkProperties?.SpeedMode == "Advertised")
                    && (IsAdvertise_1000BASE_T_FD == true || IsAdvertise_1000BASE_T_HD == true);
            }
        }

#if !DISABLE_TSN && !DISABLE_T1L
        public bool IsT1LBoard
        {
            get
            {
                return ((_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)) == true;
            }
        }

        public bool IsGigabitBoard => !IsT1LBoard;

#elif !DISABLE_TSN
        public bool IsGigabitBoard { get; } = true;

        public bool IsT1LBoard { get; } = false;
#elif !DISABLE_T1L
        public bool IsGigabitBoard { get; } = false;

        public bool IsT1LBoard { get; } = true;
#endif

        public List<string> MasterSlaveAdvertises => _linkProperties?.MasterSlaveAdvertises;

        public List<string> MDIXs => _linkProperties?.MDIXs;

        public string SelectedEnergyDetectPowerDownMode
        {
            get
            {
                return _linkProperties?.EnergyDetectPowerDownMode;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.EnergyDetectPowerDownMode = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.EnableEnergyDetectPowerDown(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.EnableEnergyDetectPowerDown(value);
                    }
                }

                OnPropertyChanged(nameof(SelectedEnergyDetectPowerDownMode));
            }
        }

        public string SelectedForcedSpeed
        {
            get
            {
                return _linkProperties?.ForcedSpeed;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.ForcedSpeed = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.SetForcedSpeed(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.SetForcedSpeed(value);
                    }
                }

                OnPropertyChanged(nameof(SelectedForcedSpeed));
                OnPropertyChanged(nameof(IsMasterSlaveForcedVisible));
            }
        }

        public string SelectedMasterSlaveAdvertise
        {
            get
            {
                return _linkProperties?.MasterSlaveAdvertise;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.MasterSlaveAdvertise = value;

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;

                        if (_linkProperties.MasterSlaveAdvertise == "Master" && _linkProperties.SpeedMode == "Advertised")
                            fwAPI.SetMasterSlave("Prefer_Master");
                        else if (_linkProperties.MasterSlaveAdvertise == "Slave" && _linkProperties.SpeedMode == "Advertised")
                            fwAPI.SetMasterSlave("Prefer_Slave");
                        else if (_linkProperties.MasterSlaveAdvertise == "Master" && _linkProperties.SpeedMode == "Forced")
                            fwAPI.SetMasterSlave("Forced_Master");
                        else if (_linkProperties.MasterSlaveAdvertise == "Slave" && _linkProperties.SpeedMode == "Forced")
                            fwAPI.SetMasterSlave("Forced_Slave");
                        else
                        {
                            //Do nothing
                        }
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwAPI.SetMasterSlave(_linkProperties.MasterSlaveAdvertise);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                    {
                        ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                        fwAPI.SetMasterSlave(_linkProperties.MasterSlaveAdvertise);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                    {
                        ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                        fwAPI.SetMasterSlave(_linkProperties.MasterSlaveAdvertise);
                    }
                    else
                    {
                        //Do nothing
                    }
                }

                OnPropertyChanged(nameof(SelectedMasterSlaveAdvertise));
            }
        }

        public string SelectedMDIX
        {
            get
            {
                return _linkProperties?.MDIX;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.MDIX = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.AutoMDIXMode(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.AutoMDIXMode(value);
                    }
                }

                OnPropertyChanged(nameof(SelectedMDIX));
            }
        }

        public string SelectedSpeedMode
        {
            get
            {
                return _linkProperties?.SpeedMode;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.SpeedMode = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.AdvertisedForcedSpeed(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.AdvertisedForcedSpeed(value);
                    }
                }

                OnPropertyChanged(nameof(SelectedSpeedMode));
                OnPropertyChanged(nameof(IsANAdvertisedSpeedVisible));
                OnPropertyChanged(nameof(IsANAdvertised1GSpeedVisible));
                OnPropertyChanged(nameof(IsForcedSpeedVisible));
                OnPropertyChanged(nameof(IsMasterSlaveForcedVisible));
                OnPropertyChanged(nameof(IsMasterSlavePrefVisible));
            }
        }

        public string SelectedTxLevel
        {
            get
            {
                return _linkProperties?.TxAdvertise;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.TxAdvertise = value;

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                        ((ADIN1100FirmwareAPI)_selectedDeviceStore.SelectedDevice.FwAPI).SetTxLevel(_linkProperties.TxAdvertise);
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                        ((ADIN1110FirmwareAPI)_selectedDeviceStore.SelectedDevice.FwAPI).SetTxLevel(_linkProperties.TxAdvertise);
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                        ((ADIN2111FirmwareAPI)_selectedDeviceStore.SelectedDevice.FwAPI).SetTxLevel(_linkProperties.TxAdvertise);
                    else
                    {
                        //Do nothings
                    }
                }

                OnPropertyChanged(nameof(SelectedTxLevel));
            }
        }

        public uint SetDownSpeedRetries
        {
            get
            {
                return _linkProperties?.DownSpeedRetries ?? 0;
            }

            set
            {
                _linkProperties.DownSpeedRetries = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwAPI.DownSpeedRetriesSetVal(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.DownSpeedRetriesSetVal(value);
                }

                OnPropertyChanged(nameof(SetDownSpeedRetries));
            }
        }

        public List<string> SpeedModes => _linkProperties?.SpeedModes;

        public List<string> TxLevels => _linkProperties?.TxAdvertises;

        private ILinkProperties _linkProperties => _selectedDeviceStore.SelectedDevice?.LinkProperties;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));

            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(IsANAdvertised1GSpeedVisible));
            OnPropertyChanged(nameof(IsANAdvertisedSpeedVisible));
            OnPropertyChanged(nameof(IsForcedSpeedVisible));
            OnPropertyChanged(nameof(IsGigabitBoard));
            OnPropertyChanged(nameof(IsT1LBoard));

            switch (_selectedDeviceStore.SelectedDevice.DeviceType)
            {
                case BoardType.ADIN1100_S1:
                case BoardType.ADIN1100:
                case BoardType.ADIN1110:
                case BoardType.ADIN2111:
                    OnPropertyChanged(nameof(TxLevels));
                    OnPropertyChanged(nameof(SelectedTxLevel));
                    OnPropertyChanged(nameof(MasterSlaveAdvertises));
                    OnPropertyChanged(nameof(SelectedMasterSlaveAdvertise));
                    break;
                case BoardType.ADIN1200:
                case BoardType.ADIN1300:
                    OnPropertyChanged(nameof(SpeedModes));
                    OnPropertyChanged(nameof(SelectedSpeedMode));
                    OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_FD));
                    OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_HD));
                    OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_FD));
                    OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_HD));
                    OnPropertyChanged(nameof(IsAdvertise_10BASE_T_FD));
                    OnPropertyChanged(nameof(IsAdvertise_10BASE_T_HD));
                    OnPropertyChanged(nameof(IsAdvertise_EEE_1000BASE_T));
                    OnPropertyChanged(nameof(IsAdvertise_EEE_100BASE_TX));
                    OnPropertyChanged(nameof(IsDownSpeed_100BASE_TX_HD));
                    OnPropertyChanged(nameof(IsDownSpeed_10BASE_T_HD));
                    OnPropertyChanged(nameof(IsDownspeed100Enabled));
                    OnPropertyChanged(nameof(IsDownspeed10Enabled));
                    OnPropertyChanged(nameof(IsEEE1000Enabled));
                    OnPropertyChanged(nameof(IsEEE100Enabled));
                    OnPropertyChanged(nameof(SetDownSpeedRetries));
                    OnPropertyChanged(nameof(ForcedSpeeds));
                    OnPropertyChanged(nameof(SelectedForcedSpeed));
                    OnPropertyChanged(nameof(MDIXs));
                    OnPropertyChanged(nameof(SelectedMDIX));
                    OnPropertyChanged(nameof(EnergyDetectPowerDownModes));
                    OnPropertyChanged(nameof(SelectedEnergyDetectPowerDownMode));
                    OnPropertyChanged(nameof(SetDownSpeedRetries));
                    OnPropertyChanged(nameof(MasterSlaveAdvertises));
                    OnPropertyChanged(nameof(SelectedMasterSlaveAdvertise));
                    break;
                default:
                    break;
            }
            OnPropertyChanged(nameof(IsMasterSlaveForcedVisible));
            OnPropertyChanged(nameof(IsMasterSlavePrefVisible));
        }
    }
}