using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using Microsoft.Win32;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class FrameGenCheckerViewModel : ViewModelBase
    {
        private string _destMacAddress;
        private bool _enableContinuousMode;
        private bool _enableMacAddress;
        private uint _frameBurst;
        private string _frameGeneratorButtonText;
        private uint _frameLength;
        private IFTDIServices _ftdiService;
        private SelectedDeviceStore _selectedDeviceStore;
        private FrameContentModel _selectedFrameContent;
        private string _srcMacAddress;
        private object _thisLock;

        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="selectedDeviceStore"></param>
        /// <param name="ftdiService"></param>
        public FrameGenCheckerViewModel(SelectedDeviceStore selectedDeviceStore, object thisLock, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;
            _thisLock = thisLock;

            ResetFrameCheckerCommnad = new ResetFrameDeviceCheckerCommnad(this, selectedDeviceStore);
            ExecuteFrameCheckerCommand = new ExecuteFrameCheckerCommand(this, selectedDeviceStore);
            RemoteLoopbackCommand = new RemoteLoopbackCommand(this, selectedDeviceStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerStatusChanged += _selectedDeviceStore_FrameGenCheckerStatusChanged;
            _selectedDeviceStore.FrameContentChanged += _selectedDeviceStore_FrameContentChanged;
        }

        public ICommand RemoteLoopbackCommand { get; set; }

        public string DestMacAddress
        {
            get { return _frameGenChecker?.DestMacAddress ?? ":::::"; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _destMacAddress = value;
                    _frameGenChecker.DestMacAddress = value;

                    var octets = value.Split(':');
                    DestOctet = octets[5] == string.Empty ? string.Empty : octets[5];
                    _frameGenChecker.DestOctet = DestOctet;
                }
                OnPropertyChanged(nameof(DestMacAddress));
            }
        }

        public string DestOctet { get; set; }

        public bool EnableContinuousMode
        {
            get { return _frameGenChecker?.EnableContinuousMode == true; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _enableContinuousMode = value;
                    _frameGenChecker.EnableContinuousMode = value;
                }
                OnPropertyChanged(nameof(EnableContinuousMode));
            }
        }

        public bool EnableMacAddress
        {
            get { return _frameGenChecker?.EnableMacAddress == true; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _enableMacAddress = value;
                    _frameGenChecker.EnableMacAddress = value;
                }
                OnPropertyChanged(nameof(EnableMacAddress));
            }
        }

        public ICommand ExecuteFrameCheckerCommand { get; set; }

        public uint FrameBurst
        {
            get { return _frameGenChecker?.FrameBurst ?? 0; }
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

        public List<FrameContentModel> FrameContents => _frameGenChecker?.FrameContents;

        public string FrameGeneratorButtonText
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN1100FirmwareAPI fwADIN1100API)
                {
                    return fwADIN1100API?.isFrameGenCheckerOngoing == true ? "Terminate" : "Generate";
                }
                else if (_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN1200FirmwareAPI fwADIN1200API)
                {
                    return fwADIN1200API?.isFrameGenCheckerOngoing == true ? "Terminate" : "Generate";
                }
                else /*(_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN1300FirmwareAPI fwADIN1300API)*/
                {
                    ADIN1300FirmwareAPI fwADIN1300API = _selectedDeviceStore.SelectedDevice?.FwAPI as ADIN1300FirmwareAPI;
                    return fwADIN1300API?.isFrameGenCheckerOngoing == true ? "Terminate" : "Generate";
                }
                //return _selectedDeviceStore.SelectedDevice?.FwAPI.isFrameGenCheckerOngoing ==  true ? "Terminate" : "Generate";
            }
            set
            {
                if (value != null)
                {
                    _frameGeneratorButtonText = value;
                    //_selectedDevice.Checker = value;
                }
                OnPropertyChanged(nameof(FrameGeneratorButtonText));
            }
        }

        public bool FrameGenRunning => true;

        public uint FrameLength
        {
            get { return _frameGenChecker?.FrameLength ?? 0; }
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

        public ICommand ResetFrameCheckerCommnad { get; set; }

        public FrameContentModel SelectedFrameContent
        {
            get { return _frameGenChecker?.FrameContent; }
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

        public string SrcMacAddress
        {
            get { return _frameGenChecker?.SrcMacAddress ?? ":::::"; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _srcMacAddress = value;
                    _frameGenChecker.SrcMacAddress = value;

                    var octets = value.Split(':');
                    SrcOctet = octets[5] == string.Empty ? string.Empty : octets[5];
                    _frameGenChecker.SrcOctet = SrcOctet;
                }
                OnPropertyChanged(nameof(SrcMacAddress));
            }
        }

        public string SrcOctet { get; set; }
        
        private IFrameGenChecker _frameGenChecker => _selectedDeviceStore.SelectedDevice?.FrameGenChecker;

        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerStatusChanged -= _selectedDeviceStore_FrameGenCheckerStatusChanged;
            _selectedDeviceStore.FrameContentChanged -= _selectedDeviceStore_FrameContentChanged;
        }

        private void _selectedDeviceStore_FrameContentChanged(FrameType obj)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                SelectedFrameContent = _frameGenChecker.FrameContents.Where(x => x.FrameContentType == obj).ToList()[0];
            }));
        }

        private void _selectedDeviceStore_FrameGenCheckerStatusChanged(string status)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //FrameGeneratorButtonText = status;
                OnPropertyChanged(nameof(FrameGeneratorButtonText));
            }));
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            OnPropertyChanged(nameof(SelectedFrameContent));
            OnPropertyChanged(nameof(FrameGenRunning));
            OnPropertyChanged(nameof(FrameBurst));
            OnPropertyChanged(nameof(FrameLength));
            OnPropertyChanged(nameof(FrameContents));
            OnPropertyChanged(nameof(SrcMacAddress));
            OnPropertyChanged(nameof(DestMacAddress));
            OnPropertyChanged(nameof(EnableMacAddress));
            OnPropertyChanged(nameof(EnableContinuousMode));
            OnPropertyChanged(nameof(FrameGeneratorButtonText));
            OnPropertyChanged(nameof(IsDeviceSelected));
        }
    }
}