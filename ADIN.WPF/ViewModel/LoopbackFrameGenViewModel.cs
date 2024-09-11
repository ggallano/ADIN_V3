
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class LoopbackFrameGenViewModel : ViewModelBase
    {
        private string _destMacAddress;
        private bool _enableContinuousMode;
        private bool _enableMacAddress;
        private uint _frameBurst;
        private uint _frameLength;
        private IFTDIServices _ftdiService;
        private SelectedDeviceStore _selectedDeviceStore;
        private FrameContentModel _selectedFrameContent;
        private string _srcMacAddress;
        private object _thisLock;
        private bool _isFrameGenOn;

        public LoopbackFrameGenViewModel(SelectedDeviceStore selectedDeviceStore, object thisLock, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;
            _thisLock = thisLock;

            ResetFrameCheckerCommnad = new ResetFrameDeviceCheckerCommnad(this, selectedDeviceStore);
            ExecuteFrameCheckerCommand = new ExecuteFrameCheckerCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerStatusChanged += _selectedDeviceStore_FrameGenCheckerStatusChanged;
            _selectedDeviceStore.FrameContentChanged += _selectedDeviceStore_FrameContentChanged;
        }

        public bool IsCuPhySelected
        {
            get
            {
                return _frameGenChecker?.IsSerDesSelected != true;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null && _frameGenChecker.IsSerDesSelected == value)
                    _frameGenChecker.IsSerDesSelected = !value;

                if (_selectedDeviceStore.SelectedDevice != null && (IsLoopback_SerDesDigital || IsLoopback_SerDes || IsLoopback_LineInterface || IsLoopback_MII))
                    IsLoopback_None = true;

                OnPropertyChanged(nameof(IsCuPhySelected));
                OnPropertyChanged(nameof(IsSerDesSelected));
            }
        }

        public bool IsSerDesSelected
        {
            get
            {
                return _frameGenChecker?.IsSerDesSelected == true;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null && _frameGenChecker.IsSerDesSelected != value)
                    _frameGenChecker.IsSerDesSelected = value;

                if (_selectedDeviceStore.SelectedDevice != null && (IsLoopback_Digital || IsLoopback_LineDriver))
                    IsLoopback_None = true;

                OnPropertyChanged(nameof(IsSerDesSelected));
                OnPropertyChanged(nameof(IsCuPhySelected));
            }
        }

        public bool IsFrameGenOff
        {
            get
            {
                return _frameGenChecker?.FrameGeneratorButtonText != "Terminate";
            }
        }
        public List<LoopbackModel> Loopbacks => _loopback?.Loopbacks;

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
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
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
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
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
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
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
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
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
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
                    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback, _loopback.TxSuppression, _loopback.RxSuppression);
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
                    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
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
                    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
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
                    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
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
                    ADIN1320FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1320FirmwareAPI;
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

        public string ImagePath => _loopback?.SelectedLoopback.ImagePath;
        public string ImagePath_TxSuppression => _loopback?.ImagePath_TxSuppression;
        public string ImagePath_RxSuppression => _loopback?.ImagePath_RxSuppression;

        public bool EnableContinuousMode
        {
            get
            {
                return _frameGenChecker?.EnableContinuousMode == true;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _enableContinuousMode = value;
                    _frameGenChecker.EnableContinuousMode = value;
                }

                OnPropertyChanged(nameof(EnableContinuousMode));
                OnPropertyChanged(nameof(EnableFrameBurst));
            }
        }

        public bool EnableFrameBurst => _frameGenChecker?.EnableContinuousMode == false;

        public uint FrameBurst_Value
        {
            get
            {
                return _frameGenChecker?.FrameBurst ?? 0;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _frameBurst = value;
                    _frameGenChecker.FrameBurst = value;
                }

                OnPropertyChanged(nameof(FrameBurst_Value));
            }
        }

        public uint FrameLength_Value
        {
            get
            {
                return _frameGenChecker?.FrameLength ?? 0;
            }

            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _frameLength = value;
                    _frameGenChecker.FrameLength = value;
                }

                OnPropertyChanged(nameof(FrameLength_Value));
            }
        }

        public FrameContentModel SelectedFrameContent
        {
            get
            {
                return _frameGenChecker?.FrameContent;
            }

            set
            {
                if (value != null)
                {
                    _selectedFrameContent = value;
                    _frameGenChecker.FrameContent = value;
                }

                OnPropertyChanged(nameof(SelectedFrameContent));
            }
        }

        public List<FrameContentModel> FrameContents => _frameGenChecker?.FrameContents;

        public string FrameGeneratorButtonText
        {
            get
            {
                return _frameGenChecker?.FrameGeneratorButtonText ?? "Generate";
            }

            set
            {
                if (value != null)
                {
                    _frameGenChecker.FrameGeneratorButtonText = value;
                }

                OnPropertyChanged(nameof(FrameGeneratorButtonText));
            }
        }

        public ICommand ResetFrameCheckerCommnad { get; set; }

        public ICommand ExecuteFrameCheckerCommand { get; set; }

        public bool FrameGenRunning => true;

        private ILoopback _loopback => _selectedDeviceStore.SelectedDevice?.Loopback;

        private IFrameGenChecker _frameGenChecker => _selectedDeviceStore.SelectedDevice?.FrameGenChecker;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerStatusChanged -= _selectedDeviceStore_FrameGenCheckerStatusChanged;
            _selectedDeviceStore.FrameContentChanged -= _selectedDeviceStore_FrameContentChanged;
        }

        private void _selectedDeviceStore_FrameContentChanged(FrameType obj)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedFrameContent = _frameGenChecker.FrameContents.Where(x => x.FrameContentType == obj).ToList()[0];
            }));
        }

        private void _selectedDeviceStore_FrameGenCheckerStatusChanged(string status)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                FrameGeneratorButtonText = status;
                OnPropertyChanged(nameof(FrameGeneratorButtonText));
            }));
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(IsCuPhySelected));
            OnPropertyChanged(nameof(IsSerDesSelected));
            OnPropertyChanged(nameof(Loopbacks));
            OnPropertyChanged(nameof(IsLoopback_None));
            OnPropertyChanged(nameof(IsLoopback_Digital));
            OnPropertyChanged(nameof(IsLoopback_LineDriver));
            OnPropertyChanged(nameof(IsLoopback_ExtCable));
            OnPropertyChanged(nameof(IsLoopback_Remote));
            OnPropertyChanged(nameof(IsLoopback_SerDesDigital));
            OnPropertyChanged(nameof(IsLoopback_SerDes));
            OnPropertyChanged(nameof(IsLoopback_LineInterface));
            OnPropertyChanged(nameof(IsLoopback_MII));
            OnPropertyChanged(nameof(IsTxSuppression));
            OnPropertyChanged(nameof(IsRxSuppression));
            OnPropertyChanged(nameof(ImagePath));
            OnPropertyChanged(nameof(ImagePath_TxSuppression));
            OnPropertyChanged(nameof(ImagePath_RxSuppression));

            OnPropertyChanged(nameof(EnableContinuousMode));
            OnPropertyChanged(nameof(EnableFrameBurst));
            OnPropertyChanged(nameof(FrameBurst_Value));
            OnPropertyChanged(nameof(FrameLength_Value));
            OnPropertyChanged(nameof(FrameContents));
            OnPropertyChanged(nameof(SelectedFrameContent));
            OnPropertyChanged(nameof(FrameGeneratorButtonText));
            OnPropertyChanged(nameof(FrameGenRunning));
        }
    }
}
