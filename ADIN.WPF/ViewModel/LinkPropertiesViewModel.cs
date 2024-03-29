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
        private bool _isANAdvertisedSpeedVisible;
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
            get { return _linkProperties?.IsAdvertise_1000BASE_T_FD == false; }
            set
            {
                _linkProperties.IsAdvertise_1000BASE_T_FD = value;
                OnPropertyChanged(nameof(IsAdvertise_1000BASE_T_FD));
            }
        }

        public bool IsAdvertise_1000BASE_T_HD
        {
            get { return _linkProperties?.IsAdvertise_1000BASE_T_HD == false; }
            set
            {
                _linkProperties.IsAdvertise_1000BASE_T_HD = value;
            }
        }

        public bool IsAdvertise_100BASE_TX_FD
        {
            get { return _linkProperties?.IsAdvertise_100BASE_TX_FD == false; }
            set
            {
                _linkProperties.IsAdvertise_100BASE_TX_FD = value;
            }
        }

        public bool IsAdvertise_100BASE_TX_HD
        {
            get { return _linkProperties?.IsAdvertise_100BASE_TX_HD == false; }
            set
            {
                _linkProperties.IsAdvertise_100BASE_TX_HD = value;
            }
        }

        public bool IsAdvertise_10BASE_T_FD
        {
            get { return _linkProperties?.IsAdvertise_10BASE_T_FD == false; }
            set
            {
                _linkProperties.IsAdvertise_10BASE_T_FD = value;
            }
        }

        public bool IsAdvertise_10BASE_T_HD
        {
            get { return _linkProperties?.IsAdvertise_10BASE_T_HD == false; }
            set
            {
                _linkProperties.IsAdvertise_10BASE_T_HD = value;
            }
        }

        public bool IsAdvertise_EEE_1000BASE_T
        {
            get { return _linkProperties?.IsAdvertise_EEE_1000BASE_T == false; }
            set
            {
                _linkProperties.IsAdvertise_EEE_1000BASE_T = value;
            }
        }

        public bool IsAdvertise_EEE_100BASE_TX
        {
            get { return _linkProperties?.IsAdvertise_EEE_100BASE_TX == false; }
            set
            {
                _linkProperties.IsAdvertise_EEE_100BASE_TX = value;
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

        public bool IsEEEAdvertisementVisible { get; set; } = true;

        public string SelectedSpeedMode
        {
            get { return _linkProperties?.SpeedMode; }
            set
            {
                _linkProperties.SpeedMode = value;
                OnPropertyChanged(nameof(SelectedSpeedMode));

                IsANAdvertisedSpeedVisible = true;
                if (_linkProperties.SpeedMode == "Forced")
                {
                    IsANAdvertisedSpeedVisible = false;
                }
            }
        }
        public List<string> SpeedModes => _linkProperties?.SpeedModes;
        private ILinkProperties _linkProperties => _selectedDeviceStore.SelectedDevice?.LinkProperties;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SpeedModes));
            OnPropertyChanged(nameof(SelectedSpeedMode));
        }
    }
}