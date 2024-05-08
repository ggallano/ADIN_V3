using ADIN.Device.Models;
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
        private bool _isANAdvertisedSpeedVisible = true;
        private bool _isANAdvertised1GSpeedVisible = true;
        private NavigationStore _navigationStore;
        private SelectedDeviceStore _selectedDeviceStore;

        public LinkPropertiesViewModel(NavigationStore navigationStore, SelectedDeviceStore selectedDeviceStore)
        {
            _navigationStore = navigationStore;
            _selectedDeviceStore = selectedDeviceStore;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public bool IsAdvertise_1000BASE_T_FD
        {
            get { return _linkProperties?.IsAdvertise_1000BASE_T_FD == true; }
            set
            {
                if (value != _linkProperties.IsAdvertise_1000BASE_T_FD)
                {
                    _linkProperties.IsAdvertise_1000BASE_T_FD = value;
                    _selectedDeviceStore.SelectedDevice.FwAPI.Speed1000FdAdvertisement(value);
                    if (value)
                    {
                        _linkProperties.AdvertisedSpeeds.Add("SPEED_1000BASE_T_FD_SPEED");
                    }
                    else
                    {
                        _linkProperties.AdvertisedSpeeds.Remove("SPEED_1000BASE_T_FD_SPEED");
                    }
                    _selectedDeviceStore.SelectedDevice.FwAPI.CheckAdvertisedSpeed(AdvertisedSpeeds);
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
                _selectedDeviceStore.SelectedDevice.FwAPI.Speed1000HdAdvertisement(value);
                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_1000BASE_T_HD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_1000BASE_T_HD_SPEED");
                }
                _selectedDeviceStore.SelectedDevice.FwAPI.CheckAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_HD));
            }
        }

        public bool IsAdvertise_100BASE_TX_FD
        {
            get { return _linkProperties?.IsAdvertise_100BASE_TX_FD == true; }
            set
            {
                _linkProperties.IsAdvertise_100BASE_TX_FD = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.Speed100FdAdvertisement(value);
                if(value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_100BASE_TX_FD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_100BASE_TX_FD_SPEED");
                }
                _selectedDeviceStore.SelectedDevice.FwAPI.CheckAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_FD));
            }
        }

        public bool IsAdvertise_100BASE_TX_HD
        {
            get { return _linkProperties?.IsAdvertise_100BASE_TX_HD == true; }
            set
            {
                _linkProperties.IsAdvertise_100BASE_TX_HD = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.Speed100HdAdvertisement(value);
                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_100BASE_TX_HD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_100BASE_TX_HD_SPEED");
                }
                _selectedDeviceStore.SelectedDevice.FwAPI.CheckAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_100BASE_TX_HD));
            }
        }

        public bool IsAdvertise_10BASE_T_FD
        {
            get { return _linkProperties?.IsAdvertise_10BASE_T_FD == true; }
            set
            {
                _linkProperties.IsAdvertise_10BASE_T_FD = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.Speed10FdAdvertisement(value);
                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_10BASE_T_FD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_10BASE_T_FD_SPEED");
                }
                _selectedDeviceStore.SelectedDevice.FwAPI.CheckAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_10BASE_T_FD));
            }
        }

        public bool IsAdvertise_10BASE_T_HD
        {
            get { return _linkProperties?.IsAdvertise_10BASE_T_HD == true; }
            set
            {
                _linkProperties.IsAdvertise_10BASE_T_HD = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.Speed10HdAdvertisement(value);
                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_10BASE_T_HD_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_10BASE_T_HD_SPEED");
                }
                _selectedDeviceStore.SelectedDevice.FwAPI.CheckAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_10BASE_T_HD));
            }
        }

        public bool IsAdvertise_EEE_1000BASE_T
        {
            get { return _linkProperties?.IsAdvertise_EEE_1000BASE_T == true; }
            set
            {
                _linkProperties.IsAdvertise_EEE_1000BASE_T = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.Speed1000EEEAdvertisement(value);
                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_1000BASE_EEE_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_1000BASE_EEE_SPEED");
                }
                _selectedDeviceStore.SelectedDevice.FwAPI.CheckAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_EEE_1000BASE_T));
            }
        }

        public bool IsAdvertise_EEE_100BASE_TX
        {
            get { return _linkProperties?.IsAdvertise_EEE_100BASE_TX == true; }
            set
            {
                _linkProperties.IsAdvertise_EEE_100BASE_TX = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.Speed100EEEAdvertisement(value);
                if (value)
                {
                    _linkProperties.AdvertisedSpeeds.Add("SPEED_100BASE_EEE_SPEED");
                }
                else
                {
                    _linkProperties.AdvertisedSpeeds.Remove("SPEED_100BASE_EEE_SPEED");
                }
                _selectedDeviceStore.SelectedDevice.FwAPI.CheckAdvertisedSpeed(AdvertisedSpeeds);
                OnPropertyChanged(nameof(IsAdvertise_EEE_100BASE_TX));
            }
        }

        public bool IsANAdvertisedSpeedVisible
        {
            get { return _isANAdvertisedSpeedVisible; }
            set
            {
                _isANAdvertisedSpeedVisible = value;
                OnPropertyChanged(nameof(IsANAdvertisedSpeedVisible));
            }
        }
        public List<string> AdvertisedSpeeds => _linkProperties.AdvertisedSpeeds;

        public bool IsANAdvertised1GSpeedVisible
        {
            get { return _isANAdvertised1GSpeedVisible && (_linkProperties?.IsSpeedCapable1G != false); }
            set
            {
                _isANAdvertised1GSpeedVisible = value;
                OnPropertyChanged(nameof(IsANAdvertised1GSpeedVisible));
            }
        }

        public bool IsEEEAdvertisementVisible { get; set; } = true;

        public string SelectedSpeedMode
        {
            get { return _linkProperties?.SpeedMode; }
            set
            {
                _linkProperties.SpeedMode = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.AdvertisedForcedSpeed(value);
                OnPropertyChanged(nameof(SelectedSpeedMode));

                IsANAdvertisedSpeedVisible = true;
                IsANAdvertised1GSpeedVisible = true;
                if (_linkProperties.SpeedMode == "Forced")
                {
                    IsANAdvertisedSpeedVisible = false;
                    IsANAdvertised1GSpeedVisible = false;
                }
            }
        }
        public List<string> SpeedModes => _linkProperties?.SpeedModes;
        public string SelectedMDIX
        {
            get { return _linkProperties?.MDIX; }
            set
            {
                _linkProperties.MDIX = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.AutoMDIXMode(value);
                OnPropertyChanged(nameof(SelectedMDIX));
            }
        }
        public List<string> MDIXs => _linkProperties?.MDIXs;
        public string SelectedEnergyDetectPowerDownMode
        {
            get { return _linkProperties?.EnergyDetectPowerDownMode; }
            set
            {
                _linkProperties.EnergyDetectPowerDownMode = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.EnableEnergyDetectPowerDown(value);
                OnPropertyChanged(nameof(SelectedEnergyDetectPowerDownMode));
            }
        }
        public List<string> EnergyDetectPowerDownModes => _linkProperties?.EnergyDetectPowerDownModes;
        public bool IsDownSpeed_100BASE_TX_HD
        {
            get { return _linkProperties?.IsDownSpeed_100BASE_TX_HD == true; }
            set
            {
                _linkProperties.IsDownSpeed_100BASE_TX_HD = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.DownSpeed100Hd(value);
                OnPropertyChanged(nameof(IsDownSpeed_100BASE_TX_HD));
            }
        }

        public bool IsDownSpeed_10BASE_T_HD
        {
            get { return _linkProperties?.IsDownSpeed_10BASE_T_HD == true; }
            set
            {
                _linkProperties.IsDownSpeed_10BASE_T_HD = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.DownSpeed10Hd(value);
                OnPropertyChanged(nameof(IsDownSpeed_10BASE_T_HD));
            }
        }
        public uint SetDownSpeedRetries
        {
            get { return _linkProperties?.DownSpeedRetries ?? 0; }
            set
            {
                _linkProperties.DownSpeedRetries = value;
                _selectedDeviceStore.SelectedDevice.FwAPI.DownSpeedRetriesSetVal(value);
                OnPropertyChanged(nameof(SetDownSpeedRetries));
            }
        }
        private ILinkProperties _linkProperties => _selectedDeviceStore.SelectedDevice?.LinkProperties;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsANAdvertised1GSpeedVisible));
            OnPropertyChanged(nameof(SpeedModes));
            OnPropertyChanged(nameof(SelectedSpeedMode));
            OnPropertyChanged(nameof(MDIXs));
            OnPropertyChanged(nameof(SelectedMDIX));
            OnPropertyChanged(nameof(EnergyDetectPowerDownModes));
            OnPropertyChanged(nameof(SelectedEnergyDetectPowerDownMode));
            OnPropertyChanged(nameof(SelectedSpeedMode));
        }
    }
}