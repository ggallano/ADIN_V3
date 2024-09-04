// <copyright file="LoopbackViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Windows;

namespace ADIN.WPF.ViewModel
{
    public class LoopbackViewModel : ViewModelBase
    {
        private IFTDIServices _ftdiService;
        private bool _isRxSuppression;
        private bool _isTxSuppression;
        private bool _enableSerDesLoopbacks = false;
        private SelectedDeviceStore _selectedDeviceStore;

        public LoopbackViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerSetToSerDes += _selectedDeviceStore_FrameGenCheckerSerDesChanged;
        }

        public string ImagePath => _loopback?.SelectedLoopback.ImagePath;

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        public bool IsLoopback_Digital
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.Digital;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[1];
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    //else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1320FirmwareAPI)
                    //{
                    //    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    //}
                    else { } //Do nothing
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsLoopback_ExtCable
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.ExtCable;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[3];
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    //else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1320FirmwareAPI)
                    //{
                    //    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    //}
                    else { } //Do nothing
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsLoopback_LineDriver
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.LineDriver;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[2];
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    //else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1320FirmwareAPI)
                    //{
                    //    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    //}
                    else { } //Do nothing
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsLoopback_LineInterface
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.LineInterface;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[7];
                    //ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsLoopback_MII
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.MII;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[8];
                    //ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsLoopback_None
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.OFF;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[0];
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    //else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1320FirmwareAPI)
                    //{
                    //    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    //}
                    else { } //Do nothing
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsLoopback_Remote
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.MacRemote;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[4];
                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1200FirmwareAPI)
                    {
                        ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1300FirmwareAPI)
                    {
                        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _isTxSuppression, _isRxSuppression);
                    }
                    //else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1320FirmwareAPI)
                    //{
                    //    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                    //}
                    else { } //Do nothing
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsLoopback_SerDes
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.SerDes;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[6];
                    //ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsLoopback_SerDesDigital
        {
            get
            {
                return _loopback?.SelectedLoopback.EnumLoopbackType == LoopBackMode.SerDesDigital;
            }

            set
            {
                if (value)
                {
                    _loopback.SelectedLoopback = Loopbacks[5];
                    //ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
                    //fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
                }

                OnPropertyChanged(nameof(IsLoopback_None));
                OnPropertyChanged(nameof(IsLoopback_Digital));
                OnPropertyChanged(nameof(IsLoopback_LineDriver));
                OnPropertyChanged(nameof(IsLoopback_ExtCable));
                OnPropertyChanged(nameof(IsLoopback_Remote));
                OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
                OnPropertyChanged(nameof(IsLoopback_SerDes));
                OnPropertyChanged(nameof(IsLoopback_LineInterface));
                OnPropertyChanged(nameof(IsLoopback_MII));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        public bool IsRxSuppression
        {
            get
            {
                return _loopback?.RxSuppression ?? false;
            }

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
            get
            {
                return _loopback?.TxSuppression ?? false;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _loopback.TxSuppression = value;
                }

                OnPropertyChanged(nameof(IsTxSuppression));
            }
        }

#if !DISABLE_TSN && !DISABLE_T1L
        public bool IsADIN1320
        {
            get
            {
                return _selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1320;
            }
        }

        public bool IsGigabitBoard
        {
            get
            {
                return ((_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1200)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1300)
                    || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1320)) == true;
            }
        }

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

#elif !DISABLE_TSN
        public bool IsADIN1320 { get; } = false;

        public bool IsGigabitBoard { get; } = true;

        public bool IsT1LBoard { get; } = false;
#elif !DISABLE_T1L
        public bool IsADIN1320 { get; } = false;

        public bool IsGigabitBoard { get; } = false;

        public bool IsT1LBoard { get; } = true;
#endif

        public List<LoopbackModel> Loopbacks => _loopback?.Loopbacks;

        public LoopbackModel SelectedLoopback
        {
            get
            {
                return _loopback?.SelectedLoopback;
            }

            set
            {
                if (value != null)
                {
                    _isTxSuppression = IsTxSuppression;
                    _isRxSuppression = IsRxSuppression;
                    _loopback.SelectedLoopback = value;
                    _loopback.RxSuppression = _isRxSuppression;
                    _loopback.TxSuppression = _isTxSuppression;

                    if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1100FirmwareAPI)
                    {
                        ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, IsTxSuppression, IsRxSuppression);
                    }
                    else if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN1110FirmwareAPI)
                    {
                        ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, IsTxSuppression, IsRxSuppression);
                    }
                    else //if (_selectedDeviceStore.SelectedDevice.FwAPI is ADIN2111FirmwareAPI)
                    {
                        ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                        fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, IsTxSuppression, IsRxSuppression);
                    }
                }

                OnPropertyChanged(nameof(SelectedLoopback));
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        private ILoopback _loopback => _selectedDeviceStore.SelectedDevice?.Loopback;

        public bool EnableSerDesLoopbacks
        {
            get
            {
                return _enableSerDesLoopbacks;
            }
            
            set
            {
                _enableSerDesLoopbacks = value;
                OnPropertyChanged(nameof(EnableSerDesLoopbacks));
                OnPropertyChanged(nameof(EnablePhyLoopbacks));
            }
        }

        public bool EnablePhyLoopbacks => !EnableSerDesLoopbacks;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerSetToSerDes -= _selectedDeviceStore_FrameGenCheckerSerDesChanged;
            base.Dispose();
        }

        private void _selectedDeviceStore_FrameGenCheckerSerDesChanged(bool isSerDesSelected)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IsLoopback_None = true;
                EnableSerDesLoopbacks = isSerDesSelected;
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));

            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(IsADIN1320));
            OnPropertyChanged(nameof(IsGigabitBoard));
            OnPropertyChanged(nameof(IsT1LBoard));

            OnPropertyChanged(nameof(IsLoopback_None));
            OnPropertyChanged(nameof(IsLoopback_Digital));
            OnPropertyChanged(nameof(IsLoopback_LineDriver));
            OnPropertyChanged(nameof(IsLoopback_ExtCable));
            OnPropertyChanged(nameof(IsLoopback_Remote));
            OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
            OnPropertyChanged(nameof(IsLoopback_SerDes));
            OnPropertyChanged(nameof(IsLoopback_LineInterface));
            OnPropertyChanged(nameof(IsLoopback_MII));

            OnPropertyChanged(nameof(SelectedLoopback));
            OnPropertyChanged(nameof(Loopbacks));

            OnPropertyChanged(nameof(IsTxSuppression));
            OnPropertyChanged(nameof(IsRxSuppression));

            OnPropertyChanged(nameof(ImagePath));
        }
    }
}