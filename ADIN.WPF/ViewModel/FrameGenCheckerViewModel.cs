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

        public double FrameBurst_Slider
        {
            get
            {
                double frameBurstSlider = 8 * Math.Log((_frameGenChecker?.FrameBurst ?? 1) + 1) / Math.Log(2);
                return frameBurstSlider;
            }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _frameBurst = Convert.ToUInt32(Math.Pow(2, value / 8) - 1);
                    _frameGenChecker.FrameBurst = _frameBurst;
                }
                OnPropertyChanged(nameof(FrameBurst_Slider));
                OnPropertyChanged(nameof(FrameBurst_Value));
            }
        }

        public uint FrameBurst_Value
        {
            get { return _frameGenChecker?.FrameBurst ?? 0; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _frameBurst = value;
                    _frameGenChecker.FrameBurst = value;
                }
                OnPropertyChanged(nameof(FrameBurst_Value));
                OnPropertyChanged(nameof(FrameBurst_Slider));
            }
        }

        public List<FrameContentModel> FrameContents => _frameGenChecker?.FrameContents;

        public string FrameGeneratorButtonText
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN1100FirmwareAPI)
                {
                    ADIN1100FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1100FirmwareAPI;
                    return fwAPI.isFrameGenCheckerOngoing == true ? "Terminate" : "Generate";
                }
                else if (_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN1110FirmwareAPI)
                {
                    ADIN1110FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1110FirmwareAPI;
                    return fwAPI.isFrameGenCheckerOngoing == true ? "Terminate" : "Generate";
                }
                else if (_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN2111FirmwareAPI)
                {
                    ADIN2111FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN2111FirmwareAPI;
                    return fwAPI.isFrameGenCheckerOngoing == true ? "Terminate" : "Generate";
                }
                else if (_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN1200FirmwareAPI)
                {
                    ADIN1200FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice.FwAPI as ADIN1200FirmwareAPI;
                    return fwAPI.isFrameGenCheckerOngoing == true ? "Terminate" : "Generate";
                }
                else /*(_selectedDeviceStore.SelectedDevice?.FwAPI is ADIN1300FirmwareAPI fwADIN1300API)*/
                {
                    ADIN1300FirmwareAPI fwAPI = _selectedDeviceStore.SelectedDevice?.FwAPI as ADIN1300FirmwareAPI;
                    return fwAPI?.isFrameGenCheckerOngoing == true ? "Terminate" : "Generate";
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

        public double FrameLength_Slider
        {
            get
            {
                double frameLengthSlider = 16 * Math.Log((_frameGenChecker?.FrameLength ?? 1) + 1) / Math.Log(2);
                return frameLengthSlider;
            }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _frameLength = Convert.ToUInt32(Math.Pow(2, value / 16) - 1);
                    _frameGenChecker.FrameLength = _frameLength;
                }
                OnPropertyChanged(nameof(FrameLength_Slider));
                OnPropertyChanged(nameof(FrameLength_Value));
            }
        }

        public uint FrameLength_Value
        {
            get { return _frameGenChecker?.FrameLength ?? 0; }
            set
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    _frameLength = value;
                    _frameGenChecker.FrameLength = value;
                }
                OnPropertyChanged(nameof(FrameLength_Value));
                OnPropertyChanged(nameof(FrameLength_Slider));
            }
        }

        public bool IsADIN1100Board
        {
            get
            {
                return (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100)
                  || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1100_S1)
                  || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110)
                  || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111);
            }
        }

        public bool IsDeviceSelected => _selectedDeviceStore.SelectedDevice != null;
        public ICommand RemoteLoopbackCommand { get; set; }
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
                //FrameGeneratorButtonText = status;
                OnPropertyChanged(nameof(FrameGeneratorButtonText));
            }));
        }
        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(SelectedFrameContent));
            OnPropertyChanged(nameof(FrameGenRunning));
            OnPropertyChanged(nameof(FrameBurst_Slider));
            OnPropertyChanged(nameof(FrameBurst_Value));
            OnPropertyChanged(nameof(FrameLength_Slider));
            OnPropertyChanged(nameof(FrameLength_Value));
            OnPropertyChanged(nameof(FrameContents));
            OnPropertyChanged(nameof(SrcMacAddress));
            OnPropertyChanged(nameof(DestMacAddress));
            OnPropertyChanged(nameof(EnableMacAddress));
            OnPropertyChanged(nameof(EnableContinuousMode));
            OnPropertyChanged(nameof(FrameGeneratorButtonText));
            OnPropertyChanged(nameof(IsDeviceSelected));
            OnPropertyChanged(nameof(IsADIN1100Board));
        }
    }
}