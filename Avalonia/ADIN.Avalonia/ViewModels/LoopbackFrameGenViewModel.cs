// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Avalonia.Commands;
using ADIN.Avalonia.Stores;
using ADIN.Device.Models;
using ADIN.Device.Services;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FTDIChip.Driver.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private EthPhyState _linkStatus = EthPhyState.Powerdown;

        public LoopbackFrameGenViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiServices)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiServices = ftdiServices;

            ResetFrameCheckerCommnad = new ResetFrameDeviceCheckerCommnad(this, selectedDeviceStore);
            ExecuteFrameCheckerCommand = new ExecuteFrameCheckerCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerStatusChanged += _selectedDeviceStore_FrameGenCheckerStatusChanged;
            _selectedDeviceStore.LinkStatusChanged += _selectedDeviceStore_LinkStatusChanged;
            _selectedDeviceStore.PhyModeChanged += _selectedDeviceStore_PhyModeChanged;
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;
        public string ButtonKind_Generate
        {
            get
            {
                if (IsDeviceSelected && IsComOpen && _linkStatus != EthPhyState.LinkUp)
                    return "tertiary";
                return "primary";
            }
        }
        public string ButtonKind_Clear
        {
            get
            {
                if (IsDeviceSelected && IsComOpen && _linkStatus != EthPhyState.LinkUp)
                    return "tertiary";
                return "secondary";
            }
        }
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

        public ObservableCollection<LoopbackModel> Loopbacks
        {
            get
            {
                ObservableCollection<LoopbackModel> shownLoopbacks = new ObservableCollection<LoopbackModel>();

                if (IsDeviceSelected && IsComOpen)
                {
                    foreach (LoopbackModel loopback in _loopback.Loopbacks)
                    {
                        if (loopback.DisabledModes != null && loopback.DisabledModes.Contains(_phyMode.ActivePhyMode))
                            continue;
                        shownLoopbacks.Add(loopback);
                    }
                }

                return shownLoopbacks;
            }
        }

        public LoopbackModel SelectedLoopback
        {
            get { return _loopback?.SelectedLoopback; }
            set
            {
                if (IsDeviceSelected && IsComOpen && value != _loopback?.SelectedLoopback && value != null)
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
            _selectedDeviceStore.LinkStatusChanged -= _selectedDeviceStore_LinkStatusChanged;
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

        private async void _selectedDeviceStore_FrameGenCheckerStatusChanged(string status)
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                FrameGeneratorButtonText = status;
                OnPropertyChanged(nameof(FrameGeneratorButtonText));
            });
        }

        private async void _selectedDeviceStore_LinkStatusChanged(EthPhyState linkStatus)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _linkStatus = linkStatus;
                OnPropertyChanged(nameof(ButtonKind_Generate));
                OnPropertyChanged(nameof(ButtonKind_Clear));
                OnPropertyChanged(nameof(FrameGeneratorButtonText));
            });
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(IsDeviceSelected));

            if (!IsDeviceSelected)
                return;

            OnPropertyChanged(nameof(HasActivePhyMode));
            OnPropertyChanged(nameof(ActivePhyMode));
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
            OnPropertyChanged(nameof(ButtonKind_Generate));
            OnPropertyChanged(nameof(ButtonKind_Clear));
            OnPropertyChanged(nameof(FrameGeneratorButtonText));
            OnPropertyChanged(nameof(FrameGenRunning));
        }

        private void _selectedDeviceStore_PhyModeChanged()
        {
            OnPropertyChanged(nameof(ActivePhyMode));

            OnPropertyChanged(nameof(Loopbacks));
            if (_loopback.SelectedLoopback.DisabledModes != null 
                && _loopback.SelectedLoopback.DisabledModes.Contains(_phyMode.ActivePhyMode))
                SelectedLoopback = _loopback.Loopbacks.Where(x => x.EnumLoopbackType == LoopBackMode.OFF).ToList()[0];
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
            OnPropertyChanged(nameof(ButtonKind_Generate));
            OnPropertyChanged(nameof(ButtonKind_Clear));
            OnPropertyChanged(nameof(FrameGeneratorButtonText));
            OnPropertyChanged(nameof(FrameGenRunning));
        }
    }
}
