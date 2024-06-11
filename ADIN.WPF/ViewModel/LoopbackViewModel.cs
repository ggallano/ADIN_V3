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

        public bool IsADIN1100Board
        {
            get { return (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100) || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1); }
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        public bool IsLoopback_Digital
        {
            get { return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.Digital; }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[1];

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
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
                    _loopback.SelectedLoopback = Loopbacks[3];

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
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
                    _loopback.SelectedLoopback = Loopbacks[2];

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
            }
        }

        public bool IsLoopback_None
        {
            get { return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.OFF; }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[0];

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
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
                    _loopback.SelectedLoopback = Loopbacks[4];

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwADIN1200API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwADIN1200API.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else /*if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)*/
                    {
                        ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwADIN1300API.SetLoopbackSetting(_loopback.SelectedLoopback, _isTxSuppression, _isRxSuppression);
                    }
                }
                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
            }
        }

        public bool IsRxSuppression
        {
            get { return _loopback?.RxSuppression ?? false; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _loopback.RxSuppression = value;
                }
                OnPropertyChanged(nameof(IsRxSuppression));
            }
        }

        public bool IsTxSuppression
        {
            get { return _loopback?.TxSuppression ?? false; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _loopback.TxSuppression = value;
                }
                OnPropertyChanged(nameof(IsTxSuppression));
            }
        }
        public List<LoopbackModel> Loopbacks => _loopback?.Loopbacks;

        private ILoopback _loopback => _selectedDeviceStore.SelectedDevice?.Loopback;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            base.Dispose();
        }
        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));
            OnPropertyChanged(nameof(IsADIN1100Board));

            OnPropertyChanged(nameof(IsLoopback_None));
            OnPropertyChanged(nameof(IsLoopback_Digital));
            OnPropertyChanged(nameof(IsLoopback_LineDriver));
            OnPropertyChanged(nameof(IsLoopback_ExtCable));
            OnPropertyChanged(nameof(IsLoopback_Remote));

            OnPropertyChanged(nameof(SelectedLoopback));
            OnPropertyChanged(nameof(Loopbacks));

            OnPropertyChanged(nameof(IsTxSuppression));
            OnPropertyChanged(nameof(IsRxSuppression));

            OnPropertyChanged(nameof(ImagePath));
        }

        public LoopbackModel SelectedLoopback
        {
            get { return _loopback?.SelectedLoopback; }
            set
            {
                if (value != null)
                {
                    _isTxSuppression = IsTxSuppression;
                    _isRxSuppression = IsRxSuppression;
                    _loopback.SelectedLoopback = value;
                    _loopback.RxSuppression = _isRxSuppression;
                    _loopback.TxSuppression = _isTxSuppression;

                    ADIN1100FirmwareAPI fwADIN1100API = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                    fwADIN1100API.SetLoopbackSetting(_loopback.SelectedLoopback, IsTxSuppression, IsRxSuppression);
                }
                OnPropertyChanged(nameof(SelectedLoopback));
                OnPropertyChanged(nameof(ImagePath));
            }
        }
    }
}