using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class LoopbackViewModel : ViewModelBase
    {
        private IFTDIServices _ftdiService;
        private bool _isRxSuppression;
        private bool _isTxSuppression;
        private SelectedDeviceStore _selectedDeviceStore;

        public LoopbackViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public string ImagePath => _loopback?.SelectedLoopback.ImagePath;

        public bool IsRxSuppression
        {
            get { return _loopback?.SelectedLoopback.RxSuppression ?? false; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _loopback.SelectedLoopback.RxSuppression = value;
                }
                OnPropertyChanged(nameof(IsRxSuppression));
            }
        }

        public bool IsTxSuppression
        {
            get { return _loopback?.SelectedLoopback.TxSuppression ?? false; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _loopback.SelectedLoopback.TxSuppression = value;
                }
                OnPropertyChanged(nameof(IsTxSuppression));
            }
        }

        public bool IsLoopback_None
        {
            get { return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.OFF; }

            set
            {
                if (value)
                {
                    _isTxSuppression = IsTxSuppression;
                    _isRxSuppression = IsRxSuppression;
                    _loopback.SelectedLoopback = Loopbacks[0];
                    _loopback.SelectedLoopback.TxSuppression = _isTxSuppression;
                    _loopback.SelectedLoopback.RxSuppression = _isRxSuppression;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
            }
        }
        public bool IsLoopback_Digital
        {
            get { return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.Digital; }

            set
            {
                if (value)
                {
                    _isTxSuppression = IsTxSuppression;
                    _isRxSuppression = IsRxSuppression;
                    _loopback.SelectedLoopback = Loopbacks[1];
                    _loopback.SelectedLoopback.TxSuppression = _isTxSuppression;
                    _loopback.SelectedLoopback.RxSuppression = _isRxSuppression;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
            }
        }
        public bool IsLoopback_LineDriver
        {
            get { return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.LineDriver; }

            set
            {
                if (value)
                {
                    _isTxSuppression = IsTxSuppression;
                    _isRxSuppression = IsRxSuppression;
                    _loopback.SelectedLoopback = Loopbacks[2];
                    _loopback.SelectedLoopback.TxSuppression = _isTxSuppression;
                    _loopback.SelectedLoopback.RxSuppression = _isRxSuppression;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
            }
        }
        public bool IsLoopback_ExtCable
        {
            get { return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.ExtCable; }

            set
            {
                if (value)
                {
                    _isTxSuppression = IsTxSuppression;
                    _isRxSuppression = IsRxSuppression;
                    _loopback.SelectedLoopback = Loopbacks[3];
                    _loopback.SelectedLoopback.TxSuppression = _isTxSuppression;
                    _loopback.SelectedLoopback.RxSuppression = _isRxSuppression;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
            }
        }
        public bool IsLoopback_Remote
        {
            get { return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.MacRemote; }

            set
            {
                if (value)
                {
                    _isTxSuppression = IsTxSuppression;
                    _isRxSuppression = IsRxSuppression;
                    _loopback.SelectedLoopback = Loopbacks[4];
                    _loopback.SelectedLoopback.TxSuppression = _isTxSuppression;
                    _loopback.SelectedLoopback.RxSuppression = _isRxSuppression;
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback);
                    }
                    //_selectedDeviceStore.SelectedDevice.FwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
            }
        }

        public List<LoopbackListingModel> Loopbacks => _loopback?.Loopbacks;

        private ILoopback _loopback => _selectedDeviceStore.SelectedDevice?.Loopback;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            base.Dispose();
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(Loopbacks));
            OnPropertyChanged(nameof(IsTxSuppression));
            OnPropertyChanged(nameof(IsRxSuppression));
            OnPropertyChanged(nameof(ImagePath));
            OnPropertyChanged(nameof(IsLoopback_None));
            OnPropertyChanged(nameof(IsLoopback_Digital));
            OnPropertyChanged(nameof(IsLoopback_LineDriver));
            OnPropertyChanged(nameof(IsLoopback_ExtCable));
            OnPropertyChanged(nameof(IsLoopback_Remote));
            OnPropertyChanged(nameof(IsDeviceSelected));
        }
    }
}