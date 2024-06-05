using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Service;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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

        public bool IsADIN1100Board
        {
            get { return _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100; }
        }

        public bool IsADIN1200Board_ADIN1300Board
        {
            get { return ((_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1300) || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1200)); }
        }

        public bool IsAdvertise_1000BASE_T_FD
        {
            get { return _linkProperties?.IsAdvertise_1000BASE_T_FD == true; }
            set
            {
                if (value != _linkProperties.IsAdvertise_1000BASE_T_FD)
                {
                    _linkProperties.IsAdvertise_1000BASE_T_FD = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.Speed1000FdAdvertisement(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.Speed1000FdAdvertisement(value);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.Speed1000FdAdvertisement(value);
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
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.LogAdvertisedSpeed(AdvertisedSpeeds);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.LogAdvertisedSpeed(AdvertisedSpeeds);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                    OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_FD));
                }
            }
        }

        public bool IsAdvertise_1000BASE_T_HD
        {
            get { return _linkProperties?.IsAdvertise_1000BASE_T_HD == true; }
            set
            {
                _linkProperties.IsAdvertise_1000BASE_T_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.Speed1000HdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.Speed1000HdAdvertisement(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.Speed1000HdAdvertisement(value);
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
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_HD));
            }
        }

        public bool IsAdvertise_100BASE_TX_FD
        {
            get { return _linkProperties?.IsAdvertise_100BASE_TX_FD == true; }
            set
            {
                _linkProperties.IsAdvertise_100BASE_TX_FD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.Speed100FdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.Speed100FdAdvertisement(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.Speed100FdAdvertisement(value);
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
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_FD));
            }
        }

        public bool IsAdvertise_100BASE_TX_HD
        {
            get { return _linkProperties?.IsAdvertise_100BASE_TX_HD == true; }
            set
            {
                _linkProperties.IsAdvertise_100BASE_TX_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.Speed100HdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.Speed100HdAdvertisement(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.Speed100HdAdvertisement(value);
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
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_HD));
            }
        }

        public bool IsAdvertise_10BASE_T_FD
        {
            get { return _linkProperties?.IsAdvertise_10BASE_T_FD == true; }
            set
            {
                _linkProperties.IsAdvertise_10BASE_T_FD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.Speed10FdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.Speed10FdAdvertisement(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.Speed10FdAdvertisement(value);
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
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_10BASE_T_FD));
            }
        }

        public bool IsAdvertise_10BASE_T_HD
        {
            get { return _linkProperties?.IsAdvertise_10BASE_T_HD == true; }
            set
            {
                _linkProperties.IsAdvertise_10BASE_T_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.Speed10HdAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.Speed10HdAdvertisement(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.Speed10HdAdvertisement(value);
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
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_10BASE_T_HD));
            }
        }

        public bool IsAdvertise_EEE_1000BASE_T
        {
            get { return _linkProperties?.IsAdvertise_EEE_1000BASE_T == true; }
            set
            {
                _linkProperties.IsAdvertise_EEE_1000BASE_T = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.Speed1000EEEAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.Speed1000EEEAdvertisement(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.Speed1000EEEAdvertisement(value);
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
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_EEE_1000BASE_T));
            }
        }

        public bool IsAdvertise_EEE_100BASE_TX
        {
            get { return _linkProperties?.IsAdvertise_EEE_100BASE_TX == true; }
            set
            {
                _linkProperties.IsAdvertise_EEE_100BASE_TX = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.Speed100EEEAdvertisement(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.Speed100EEEAdvertisement(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.Speed100EEEAdvertisement(value);
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
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.LogAdvertisedSpeed(AdvertisedSpeeds);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.LogAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_EEE_100BASE_TX));
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
            get { return _linkProperties?.IsDownSpeed_100BASE_TX_HD == true; }
            set
            {
                _linkProperties.IsDownSpeed_100BASE_TX_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.DownSpeed100Hd(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.DownSpeed100Hd(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.DownSpeed100Hd(value);
                OnPropertyChanged(nameof(IsDownSpeed_100BASE_TX_HD));
            }
        }

        public bool IsDownSpeed_10BASE_T_HD
        {
            get { return _linkProperties?.IsDownSpeed_10BASE_T_HD == true; }
            set
            {
                _linkProperties.IsDownSpeed_10BASE_T_HD = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.DownSpeed10Hd(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.DownSpeed10Hd(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.DownSpeed10Hd(value);
                OnPropertyChanged(nameof(IsDownSpeed_10BASE_T_HD));
            }
        }

        public bool IsEEEAdvertisementVisible { get; set; } = true;

        public bool IsForcedSpeedVisible
        {
            get { return _linkProperties?.SpeedMode == "Forced"; }
        }
        public List<string> MDIXs => _linkProperties?.MDIXs;

        public string SelectedEnergyDetectPowerDownMode
        {
            get { return _linkProperties?.EnergyDetectPowerDownMode; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.EnergyDetectPowerDownMode = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.EnableEnergyDetectPowerDown(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.EnableEnergyDetectPowerDown(value);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.EnableEnergyDetectPowerDown(value);
                }
                OnPropertyChanged(nameof(SelectedEnergyDetectPowerDownMode));
            }
        }

        public string SelectedForcedSpeed
        {
            get { return _linkProperties?.ForcedSpeed; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.ForcedSpeed = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetForcedSpeed(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetForcedSpeed(value);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.SetForcedSpeed(value);
                }
                OnPropertyChanged(nameof(SelectedForcedSpeed));
            }
        }

        public string SelectedMDIX
        {
            get { return _linkProperties?.MDIX; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.MDIX = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.AutoMDIXMode(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.AutoMDIXMode(value);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.AutoMDIXMode(value);
                }
                OnPropertyChanged(nameof(SelectedMDIX));
            }
        }

        public string SelectedSpeedMode
        {
            get { return _linkProperties?.SpeedMode; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.SpeedMode = value;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.AdvertisedForcedSpeed(value);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.AdvertisedForcedSpeed(value);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.AdvertisedForcedSpeed(value);
                }
                OnPropertyChanged(nameof(SelectedSpeedMode));
                OnPropertyChanged(nameof(IsANAdvertisedSpeedVisible));
                OnPropertyChanged(nameof(IsANAdvertised1GSpeedVisible));
                OnPropertyChanged(nameof(IsForcedSpeedVisible));
            }
        }

        public uint SetDownSpeedRetries
        {
            get { return _linkProperties?.DownSpeedRetries ?? 0; }
            set
            {
                _linkProperties.DownSpeedRetries = value;
                if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    fwADIN1200API.DownSpeedRetriesSetVal(value);
                }
                else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwADIN1300API.DownSpeedRetriesSetVal(value);
                }
                //_selectedDeviceStore.SelectedDevice.FwAPI.DownSpeedRetriesSetVal(value);
                OnPropertyChanged(nameof(SetDownSpeedRetries));
            }
        }

        public List<string> SpeedModes => _linkProperties?.SpeedModes;

        public List<string> MasterSlaveAdvertises => _linkProperties?.MasterSlaveAdvertises;

        public string SelectedMasterSlaveAdvertise
        {
            get { return _linkProperties?.MasterSlaveAdvertise; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.MasterSlaveAdvertise = value;
                    ((ADIN1100FirmwareAPI)_selectedDeviceStore.SelectedDevice.FwAPI).SetMasterSlave(_linkProperties.MasterSlaveAdvertise);
                }
                OnPropertyChanged(nameof(SelectedMasterSlaveAdvertise));
            }
        }

        public List<string> TxLevels => _linkProperties?.TxAdvertises;

        public string SelectedTxLevel
        {
            get { return _linkProperties?.TxAdvertise; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _linkProperties.TxAdvertise = value;
                    ((ADIN1100FirmwareAPI)_selectedDeviceStore.SelectedDevice.FwAPI).SetTxLevel(_linkProperties.TxAdvertise);
                }
                OnPropertyChanged(nameof(SelectedTxLevel));
            }
        }

        private ILinkProperties _linkProperties => _selectedDeviceStore.SelectedDevice?.LinkProperties;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SpeedModes));
            OnPropertyChanged(nameof(SelectedSpeedMode));
            OnPropertyChanged(nameof(IsANAdvertised1GSpeedVisible));
            OnPropertyChanged(nameof(IsANAdvertisedSpeedVisible));
            OnPropertyChanged(nameof(IsForcedSpeedVisible));
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
            OnPropertyChanged(nameof(SetDownSpeedRetries));
            OnPropertyChanged(nameof(ForcedSpeeds));
            OnPropertyChanged(nameof(SelectedForcedSpeed));
            OnPropertyChanged(nameof(MDIXs));
            OnPropertyChanged(nameof(SelectedMDIX));
            OnPropertyChanged(nameof(EnergyDetectPowerDownModes));
            OnPropertyChanged(nameof(SelectedEnergyDetectPowerDownMode));
            OnPropertyChanged(nameof(SetDownSpeedRetries));
            OnPropertyChanged(nameof(IsDeviceSelected));
            OnPropertyChanged(nameof(IsADIN1200Board_ADIN1300Board));
            OnPropertyChanged(nameof(IsADIN1100Board));
            OnPropertyChanged(nameof(MasterSlaveAdvertises));
            OnPropertyChanged(nameof(SelectedMasterSlaveAdvertise));
            OnPropertyChanged(nameof(TxLevels));
            OnPropertyChanged(nameof(SelectedTxLevel));
        }
    }
}