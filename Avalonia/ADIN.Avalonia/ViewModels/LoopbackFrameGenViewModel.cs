using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Stores;
using ADIN.Device.Models;
using ADIN.Device.Services;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Threading;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform;

namespace ADIN.Avalonia.ViewModels
{
    public class LoopbackFrameGenViewModel : ViewModelBase
    {
        private string _destMacAddress;
        private bool _enableContinuousMode;
        private bool _enableMacAddress;
        private uint _frameBurst;
        private uint _frameLength;
        private SelectedDeviceStore _selectedDeviceStore;
        private IFTDIServices _ftdiServices;
        private FrameContentModel _selectedFrameContent;
        private string _srcMacAddress;
        private bool _isFrameGenOn;

        public LoopbackFrameGenViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiServices)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiServices = ftdiServices;

            ResetFrameCheckerCommnad = new ResetFrameDeviceCheckerCommnad(this, selectedDeviceStore);
            ExecuteFrameCheckerCommand = new ExecuteFrameCheckerCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerStatusChanged += _selectedDeviceStore_FrameGenCheckerStatusChanged;
            //_selectedDeviceStore.FrameContentChanged += _selectedDeviceStore_FrameContentChanged;
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;
        public bool IsComOpen => _ftdiServices.IsComOpen;

        public bool HasActivePhyMode => _phyMode?.ActivePhyMode != null;

        public string ActivePhyMode
        {
            get
            {
                if (_phyMode?.ActivePhyMode == "Auto Media Detect_Cu"
                    || _phyMode?.ActivePhyMode == "Auto Media Detect_Fi")
                    return "Auto Media Detect";
                else if (HasActivePhyMode)
                    return _phyMode?.ActivePhyMode;
                else
                    return string.Empty;
            }
        }

        public ObservableCollection<LoopbackModel> Loopbacks => _loopback?.Loopbacks;

        public LoopbackModel SelectedLoopback
        {
            get { return _loopback?.SelectedLoopback; }
            set
            {
                if (IsDeviceSelected && IsComOpen && value != _loopback?.SelectedLoopback)
                {
                    _loopback.SelectedLoopback = value;
                    ILoopbackAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ILoopbackAPI;
                    fwAPI.SetLoopbackSetting(_loopback.SelectedLoopback);
                    OnPropertyChanged(nameof(ImagePath));
                    OnPropertyChanged(nameof(SelectedLoopback));
                }
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
                    ILoopbackAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ILoopbackAPI;
                    fwAPI.SetRxSuppression(_loopback.RxSuppression);
                    OnPropertyChanged(nameof(IsRxSuppression));
                    OnPropertyChanged(nameof(ImagePath_RxSuppression));
                }
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
                    ILoopbackAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ILoopbackAPI;
                    fwAPI.SetTxSuppression(_loopback.TxSuppression);
                    OnPropertyChanged(nameof(IsTxSuppression));
                    OnPropertyChanged(nameof(ImagePath_TxSuppression));
                }
            }
        }

        public Bitmap ImagePath
        {
            get
            {
                if (IsDeviceSelected)
                {
                    try
                    {
                        Uri imageUri = new Uri($"avares://ADIN.Avalonia{_loopback?.SelectedLoopback?.ImagePath}");
                        return new Bitmap(AssetLoader.Open(imageUri));
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        public Bitmap ImagePath_TxSuppression
        {
            get
            {
                if (IsDeviceSelected)
                {
                    try
                    {
                        Uri imageUri = new Uri($"avares://ADIN.Avalonia{_loopback?.ImagePath_TxSuppression}");
                        return new Bitmap(AssetLoader.Open(imageUri));
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        public Bitmap ImagePath_RxSuppression
        {
            get
            {
                if (IsDeviceSelected && IsComOpen)
                {
                    try
                    {
                        Uri imageUri = new Uri($"avares://ADIN.Avalonia{_loopback?.ImagePath_RxSuppression}");
                        return new Bitmap(AssetLoader.Open(imageUri));
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }
        }

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

        public uint FrameBurst
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

                OnPropertyChanged(nameof(FrameBurst));
            }
        }

        public uint FrameLength
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

                OnPropertyChanged(nameof(FrameLength));
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
                OnPropertyChanged(nameof(SelectedFrameContent.Name));
            }
        }

        public ObservableCollection<FrameContentModel> FrameContents => _frameGenChecker?.FrameContents;

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
        private IPhyMode _phyMode => _selectedDeviceStore.SelectedDevice?.PhyMode;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerStatusChanged -= _selectedDeviceStore_FrameGenCheckerStatusChanged;
            //_selectedDeviceStore.FrameContentChanged -= _selectedDeviceStore_FrameContentChanged;
        }

        //private void _selectedDeviceStore_FrameContentChanged(FrameType obj)
        //{
        //    if (_selectedDeviceStore.SelectedDevice == null)
        //        return;

        //    Dispatcher.UIThread.Post(() =>
        //    {
        //        SelectedFrameContent = _frameGenChecker.FrameContents.Where(x => x.FrameContentType == obj).ToList()[0];
        //    });
        //}

        private void _selectedDeviceStore_FrameGenCheckerStatusChanged(string status)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Dispatcher.UIThread.Post(() =>
            {
                FrameGeneratorButtonText = status;
                OnPropertyChanged(nameof(FrameGeneratorButtonText));
            });
        }

        //private void UpdateFrameGenSettings()
        //{
        //    if (_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN1300FirmwareAPI)
        //    {
        //        ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1300FirmwareAPI;
        //        IFrameGenChecker frameGenSettings = fwAPI.GetFrameGenSettings(_frameGenChecker.FrameContents, IsCuPhySelected);

        //        _frameGenChecker.EnableContinuousMode = frameGenSettings.EnableContinuousMode;
        //        _frameGenChecker.FrameBurst = frameGenSettings.FrameBurst;
        //        _frameGenChecker.FrameLength = frameGenSettings.FrameLength;
        //        _frameGenChecker.FrameContent = frameGenSettings.FrameContent;

        //        OnPropertyChanged(nameof(EnableContinuousMode));
        //        OnPropertyChanged(nameof(EnableFrameBurst));
        //        OnPropertyChanged(nameof(FrameBurst_Value));
        //        OnPropertyChanged(nameof(FrameLength_Value));
        //        OnPropertyChanged(nameof(FrameContents));
        //        OnPropertyChanged(nameof(SelectedFrameContent));
        //        OnPropertyChanged(nameof(FrameGeneratorButtonText));
        //        OnPropertyChanged(nameof(FrameGenRunning));
        //    }
        //}

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));

            if (!IsDeviceSelected)
                return;

            OnPropertyChanged(nameof(ActivePhyMode));
            OnPropertyChanged(nameof(HasActivePhyMode));
            OnPropertyChanged(nameof(Loopbacks));
            OnPropertyChanged(nameof(SelectedLoopback));
            OnPropertyChanged(nameof(IsTxSuppression));
            OnPropertyChanged(nameof(IsRxSuppression));
            OnPropertyChanged(nameof(ImagePath));
            OnPropertyChanged(nameof(ImagePath_TxSuppression));
            OnPropertyChanged(nameof(ImagePath_RxSuppression));

            OnPropertyChanged(nameof(EnableContinuousMode));
            OnPropertyChanged(nameof(EnableFrameBurst));
            OnPropertyChanged(nameof(FrameBurst));
            OnPropertyChanged(nameof(FrameLength));
            OnPropertyChanged(nameof(FrameContents));
            OnPropertyChanged(nameof(SelectedFrameContent));
            OnPropertyChanged(nameof(FrameGeneratorButtonText));
            OnPropertyChanged(nameof(FrameGenRunning));
        }
    }
}
