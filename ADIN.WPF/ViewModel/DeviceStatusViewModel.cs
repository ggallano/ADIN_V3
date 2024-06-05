using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ADIN.WPF.ViewModel
{
    public class DeviceStatusViewModel : ViewModelBase
    {
        private List<string> _advertisedSpeedList = new List<string>()
        {
            "SPEED_1000BASE_T_FD_SPEED",
            "SPEED_1000BASE_T_HD_SPEED",
            "SPEED_1000BASE_EEE_SPEED",
            "SPEED_100BASE_TX_FD_SPEED",
            "SPEED_100BASE_TX_HD_SPEED",
            "SPEED_100BASE_EEE_SPEED",
            "SPEED_10BASE_T_FD_SPEED",
            "SPEED_10BASE_T_HD_SPEED"
        };

        private string _anStatus = "-";
        private BackgroundWorker _backgroundWorker;
        private string _checker = "-";
        private IFTDIServices _ftdiService;
        private string _generator = "-";
        private bool _isVisibleSpeedList = true;
        private string _linkLength;
        private string _linkStatus = "-";
        private List<string> _localAdvertisedSpeeds = new List<string>();
        private string _masterSlaveStatus = "-";
        private string _mseValue = "-";
        private BackgroundWorker _readRegisterWorker;
        private List<string> _remoteAdvertisedSpeeds = new List<string>();
        private SelectedDeviceStore _selectedDeviceStore;
        private string _speedMode = "-";
        private object _thisLock;
        private string _txLevelStatus = "-";

        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="selectedDeviceStore">selected device store</param>
        public DeviceStatusViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            SetBackgroundWroker();

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerResetDisplay += _selectedDeviceStore_FrameGenCheckerResetDisplay;
        }

        public string AdvertisedSpeed
        {
            get
            {
                foreach(var speed in _advertisedSpeedList)
                {
                    if (_localAdvertisedSpeeds.Contains(speed) && _remoteAdvertisedSpeeds.Contains(speed))
                    {
                        return speed;
                    }
                }
                return "-";
            }
        }

        public string AnStatus
        {
            get { return _anStatus; }
            set
            {
                _anStatus = value;
                OnPropertyChanged(nameof(AnStatus));
            }
        }

        public string BoardName => _selectedDeviceStore.SelectedDevice?.BoardName ?? "No Device";

        public string Checker
        {
            get { return _selectedDevice?.Checker ?? "-"; }
            set
            {
                if (_selectedDevice != null)
                {
                    _checker = value;
                    _selectedDevice.Checker = value;
                }
                OnPropertyChanged(nameof(Checker));
            }
        }

        public string DeviceType => _selectedDeviceStore.SelectedDevice?.DeviceType.ToString() ?? "-";

        public string Generator
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    return _generator;
                }
                return "-";
            }

            set
            {
                if (value != _generator)
                {
                    _generator = value;
                    OnPropertyChanged(nameof(Generator));
                }
            }
        }

        //public string LinkLength
        //{
        //    get { return _linkLength; }
        //    set
        //    {
        //        _linkLength = value;
        //        OnPropertyChanged(nameof(LinkLength));
        //    }
        //}

        public bool IsAnStatusVisible
        {
            get { return _selectedDevice?.DeviceType == BoardType.ADIN1100; }
        }

        public bool IsVisibleSpeedList
        {
            get { return _speedMode == "Advertised"; }
        }

        public string LinkStatus
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    return _linkStatus;
                }
                return "-";
            }
            set
            {
                switch ((EthPhyState)Enum.Parse(typeof(EthPhyState), value))
                {
                    case EthPhyState.Powerdown:
                        _selectedDeviceStore.OnSoftwarePowerDownChanged("Software Power Up");
                        break;

                    case EthPhyState.Standby:
                        _selectedDeviceStore.OnLinkStatusChanged(EthPhyState.Standby.ToString());
                        _selectedDeviceStore.OnSoftwarePowerDownChanged("Software Power Down");
                        break;

                    case EthPhyState.LinkDown:
                        _selectedDeviceStore.OnLinkStatusChanged(EthPhyState.LinkDown.ToString());
                        _selectedDeviceStore.OnSoftwarePowerDownChanged("Software Power Down");
                        break;

                    case EthPhyState.LinkUp:
                        _selectedDeviceStore.OnLinkStatusChanged(EthPhyState.LinkUp.ToString());
                        _selectedDeviceStore.OnSoftwarePowerDownChanged("Software Power Down");
                        break;

                    default:
                        break;
                }
                if (value != _linkStatus)
                {
                    _linkStatus = value;
                    OnPropertyChanged(nameof(LinkStatus));
                }
            }
        }

        //public string MasterSlaveStatus
        //{
        //    get { return _masterSlaveStatus; }
        //    set
        //    {
        //        _masterSlaveStatus = value;
        //        OnPropertyChanged(nameof(MasterSlaveStatus));
        //    }
        //}

        public List<string> LocalAdvertisedSpeeds
        {
            get { return _localAdvertisedSpeeds; }
            set
            {
                if (!value.SequenceEqual(_localAdvertisedSpeeds))
                {
                    _localAdvertisedSpeeds = value;
                    OnPropertyChanged(nameof(LocalAdvertisedSpeeds));
                    OnPropertyChanged(nameof(AdvertisedSpeed));
                }
            }
        }

        public string MseValue
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    return _mseValue;
                }
                return "-";
            }
            
            set
            {
                if (value != _mseValue)
                {
                    _mseValue = value;
                    OnPropertyChanged(nameof(MseValue));
                }
            }
        }

        public string PhyAddress => _selectedDeviceStore.SelectedDevice?.PhyAddress.ToString() ?? "-";
        public List<string> RemoteAdvertisedSpeeds
        {
            get { return _remoteAdvertisedSpeeds; }
            set
            {
                if (!value.SequenceEqual(_remoteAdvertisedSpeeds))
                {
                    _remoteAdvertisedSpeeds = value;
                    OnPropertyChanged(nameof(RemoteAdvertisedSpeeds));
                    OnPropertyChanged(nameof(AdvertisedSpeed));
                }
            }
        }

        public string SerialNumber => _selectedDeviceStore.SelectedDevice?.SerialNumber ?? "-";

        //public string TxLevelStatus
        //{
        //    get { return _txLevelStatus; }
        //    set
        //    {
        //        _txLevelStatus = value;
        //        OnPropertyChanged(nameof(TxLevelStatus));
        //    }
        //}

        public string SpeedMode
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    return _speedMode;
                }
                return "-";
            }

            set
            {
                if (value != _speedMode)
                {
                    _speedMode = value;
                    OnPropertyChanged(nameof(SpeedMode));
                    OnPropertyChanged(nameof(IsVisibleSpeedList));
                }
            }
        }

        private ADINDevice _selectedDevice => _selectedDeviceStore.SelectedDevice;
        protected override void Dispose()
        {
            _selectedDeviceStore.SelectedDeviceChanged -= _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerResetDisplay -= _selectedDeviceStore_FrameGenCheckerResetDisplay;
            base.Dispose();
        }

        private void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            string deviceStatus = (string)e.Argument;
            while (!_backgroundWorker.CancellationPending)
            {
                try
                {
                    //lock (_thisLock)
                    {
                        if (_selectedDevice != null && _ftdiService.IsComOpen)
                        {
                            //_selectedDevice.FwAPI.ReadRegsiters();
                            //_selectedDeviceStore.OnRegistersValueChanged();

                            // UI Control Update
                            //_selectedDevice.FirmwareAPI.GetNegotiationMasterSlaveInitialization(true);
                            //_selectedDevice.FirmwareAPI.GetPeakVoltageInitialization(true);
                            //_selectedDevice.FirmwareAPI.GetTestModeInitialization(true);
                            //_selectedDevice.FirmwareAPI.GetLoopbackInitialization(true);
                            //_selectedDevice.FirmwareAPI.GetFrameContentInitialization(true);

                            // Device Status
                            LinkStatus = _selectedDevice.FwAPI.GetLinkStatus();
                            MseValue = _selectedDevice.FwAPI.GetMseValue();
                            if (_selectedDevice.FwAPI is ADIN1100FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                AnStatus = fwAPI.GetAnStatus();
                                //MasterSlaveStatus = _selectedDevice.FwAPI.GetMasterSlaveStatus();
                                //TxLevelStatus = _selectedDevice.FwAPI.GetTxLevelStatus();
                                
                            }
                            else
                            {
                                SpeedMode = _selectedDevice.FwAPI.GetSpeedMode();
                                _selectedDevice.FwAPI.GetFrameCheckerStatus();
                                Generator = _selectedDevice.FwAPI.GetFrameGeneratorStatus();
                                LocalAdvertisedSpeeds = _selectedDevice.FwAPI.LocalAdvertisedSpeedList();
                                RemoteAdvertisedSpeeds = _selectedDevice.FwAPI.RemoteAdvertisedSpeedList();
                            }
                            
                        }

                        //OnPropertyChanged(nameof(LinkStatus));
                        //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    OnPropertyChanged(nameof(LinkStatus));
                        //});
                    }

                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                }
                e.Result = "Done";
            }
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("Progress Changed");
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("_backgroundWorker Completed");
        }

        private void _readRegisterWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("Progress Changed");
        }

        private void _readRegisterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("_readRegisterWorker Completed");
        }

        private void _selectedDeviceStore_FrameGenCheckerResetDisplay(string status)
        {
            Checker = status;
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            Debug.WriteLine($"[{SerialNumber}] Selected");

            OnPropertyChanged(nameof(BoardName));
            OnPropertyChanged(nameof(SerialNumber));
            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(AnStatus));
            OnPropertyChanged(nameof(IsAnStatusVisible));
            //OnPropertyChanged(nameof(MasterSlaveStatus));
            OnPropertyChanged(nameof(Generator));
            OnPropertyChanged(nameof(MseValue));
            //OnPropertyChanged(nameof(TxLevelStatus));
            OnPropertyChanged(nameof(DeviceType));
            OnPropertyChanged(nameof(PhyAddress));
            OnPropertyChanged(nameof(LocalAdvertisedSpeeds));
            OnPropertyChanged(nameof(RemoteAdvertisedSpeeds));
            OnPropertyChanged(nameof(AdvertisedSpeed));
            OnPropertyChanged(nameof(SpeedMode));
            OnPropertyChanged(nameof(IsVisibleSpeedList));
            OnPropertyChanged(nameof(Checker));
        }

        private void SetBackgroundWroker()
        {
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;

            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;

            _backgroundWorker.RunWorkerAsync();
        }
    }
}