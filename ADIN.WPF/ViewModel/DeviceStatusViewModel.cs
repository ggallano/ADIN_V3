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
        private List<string> _localAdvertisedSpeeds = new List<string>() { "" };
        private bool _loggedOneError = false;
        private object _mainLock;
        private string _masterSlaveStatus = "-";
        private string _maxSlicerError;
        private string _mseValue = "-";
        private BackgroundWorker _readRegisterWorker;
        private List<string> _remoteAdvertisedSpeeds = new List<string>();
        private SelectedDeviceStore _selectedDeviceStore;
        private string _speedMode = "-";
        private string _spikeCount;
        private object _thisLock;
        private string _txLevelStatus = "-";

        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="selectedDeviceStore">selected device store</param>
        public DeviceStatusViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, object mainLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;
            _mainLock = mainLock;

            SetBackgroundWroker();

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerResetDisplay += _selectedDeviceStore_FrameGenCheckerResetDisplay;
            _selectedDeviceStore.PortNumChanged += _selectedDeviceStore_PortNumChanged;
        }

        public string AdvertisedSpeed
        {
            get
            {
                if ((_selectedDevice?.DeviceType == BoardType.ADIN1100)
                 || (_selectedDevice?.DeviceType == BoardType.ADIN1100_S1)
                 || (_selectedDevice?.DeviceType == BoardType.ADIN1110)
                 || (_selectedDevice?.DeviceType == BoardType.ADIN2111))
                    return _localAdvertisedSpeeds[0];

                if (_selectedDevice?.DeviceType == BoardType.ADIN1200
                 || _selectedDevice?.DeviceType == BoardType.ADIN1300)
                {
                    List<string> matchingSpeed =
                        (from localSpeed in _localAdvertisedSpeeds
                         where (localSpeed != "") && _remoteAdvertisedSpeeds.Contains(localSpeed)
                         select localSpeed).ToList();

                    if (matchingSpeed.Count > 0)
                    {
                        return matchingSpeed[0];
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

        public bool Is1100Visible
        {
            get
            {
                return (_selectedDevice?.DeviceType == BoardType.ADIN1100)
                    || (_selectedDevice?.DeviceType == BoardType.ADIN1100_S1)
                    || (_selectedDevice?.DeviceType == BoardType.ADIN1110)
                    || (_selectedDevice?.DeviceType == BoardType.ADIN2111);
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
        public bool IsSpikeCountVisible => ((_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN2111) || (_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1110));

        public bool IsVisibleSpeedList
        {
            get
            {
                return _speedMode == "Advertised"
                    && ((_selectedDevice?.DeviceType != BoardType.ADIN1100)
                    && (_selectedDevice?.DeviceType != BoardType.ADIN1100_S1)
                    && (_selectedDevice?.DeviceType != BoardType.ADIN1110)
                    && (_selectedDevice?.DeviceType != BoardType.ADIN2111));
            }
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

        public string MasterSlaveStatus
        {
            get { return _masterSlaveStatus; }
            set
            {
                _masterSlaveStatus = value;
                OnPropertyChanged(nameof(MasterSlaveStatus));
            }
        }

        public string MaxSlicerError
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice == null)
                    return "N/A";

                if (_selectedDevice.PortNumber == 1)
                    return _selectedDeviceStore.SelectedDevice.MaxSlicerErrorPort1.ToString();
                else
                    return _selectedDeviceStore.SelectedDevice.MaxSlicerErrorPort2.ToString();
            }
            set
            {
                if (_selectedDevice.PortNumber == 1)
                {
                    if (Convert.ToDouble(value) != _selectedDeviceStore.SelectedDevice.MaxSlicerErrorPort1)
                    {
                        _selectedDeviceStore.SelectedDevice.MaxSlicerErrorPort1 = Convert.ToDouble(value);
                        OnPropertyChanged(nameof(MaxSlicerError));
                    }
                }
                else
                {
                    if (Convert.ToDouble(value) != _selectedDeviceStore.SelectedDevice.MaxSlicerErrorPort2)
                    {
                        _selectedDeviceStore.SelectedDevice.MaxSlicerErrorPort2 = Convert.ToDouble(value);
                        OnPropertyChanged(nameof(MaxSlicerError));
                    }
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

        public string SpikeCount
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice == null)
                    return "N/A";

                if (_selectedDevice.PortNumber == 1)
                    return _selectedDeviceStore.SelectedDevice.SpikeCountPortPort1.ToString();
                else
                    return _selectedDeviceStore.SelectedDevice.SpikeCountPortPort2.ToString();
            }
            set
            {
                if (_selectedDevice.PortNumber == 1)
                {
                    if (Convert.ToDouble(value) != _selectedDeviceStore.SelectedDevice.SpikeCountPortPort1)
                    {
                        _selectedDeviceStore.SelectedDevice.SpikeCountPortPort1 = Convert.ToDouble(value);
                        OnPropertyChanged(nameof(SpikeCount));
                    }
                }
                else
                {
                    if (Convert.ToDouble(value) != _selectedDeviceStore.SelectedDevice.SpikeCountPortPort2)
                    {
                        _selectedDeviceStore.SelectedDevice.SpikeCountPortPort2 = Convert.ToDouble(value);
                        OnPropertyChanged(nameof(SpikeCount));
                    }
                }
            }
        }

        public string TxLevelStatus
        {
            get { return _txLevelStatus; }
            set
            {
                _txLevelStatus = value;
                OnPropertyChanged(nameof(TxLevelStatus));
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
                    lock (_mainLock)
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

                            // Common ADIN Device Status
                            LinkStatus = _selectedDevice.FwAPI.GetLinkStatus();
                            LocalAdvertisedSpeeds = _selectedDevice.FwAPI.LocalAdvertisedSpeedList();

                            // Specific ADIN Device Status
#if !DISABLE_T1L
                            if (_selectedDevice.FwAPI is ADIN1100FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                AnStatus = fwAPI.GetAnStatus();
                                MasterSlaveStatus = fwAPI.GetMasterSlaveStatus();
                                TxLevelStatus = fwAPI.GetTxLevelStatus();
                                MseValue = fwAPI.GetMseValue(_selectedDevice.BoardRev);
                                MaxSlicerError = fwAPI.GetMaxSlicer();
                                SpikeCount = fwAPI.GetSpikeCount();
                            }
                            else if (_selectedDevice.FwAPI is ADIN1110FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN1110FirmwareAPI;
                                AnStatus = fwAPI.GetAnStatus();
                                MasterSlaveStatus = fwAPI.GetMasterSlaveStatus();
                                TxLevelStatus = fwAPI.GetTxLevelStatus();
                                MseValue = fwAPI.GetMseValue(_selectedDevice.BoardRev);
                                MaxSlicerError = fwAPI.GetMaxSlicer();
                                SpikeCount = fwAPI.GetSpikeCount();
                            }
                            else if (_selectedDevice.FwAPI is ADIN2111FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN2111FirmwareAPI;
                                AnStatus = fwAPI.GetAnStatus();
                                MasterSlaveStatus = fwAPI.GetMasterSlaveStatus();
                                TxLevelStatus = fwAPI.GetTxLevelStatus();
                                MseValue = fwAPI.GetMseValue(_selectedDevice.BoardRev);
                                MaxSlicerError = fwAPI.GetMaxSlicer();
                                SpikeCount = fwAPI.GetSpikeCount();
                            }
                            else { } //Do nothing
#endif
#if !DISABLE_TSN
                            if (_selectedDevice.FwAPI is ADIN1200FirmwareAPI || _selectedDevice.FwAPI is ADIN1300FirmwareAPI)
                            {
                                MseValue = _selectedDevice.FwAPI.GetMseValue();
                                SpeedMode = _selectedDevice.FwAPI.GetSpeedMode();
                                _selectedDevice.FwAPI.GetFrameCheckerStatus();
                                Generator = _selectedDevice.FwAPI.GetFrameGeneratorStatus();
                                RemoteAdvertisedSpeeds = _selectedDevice.FwAPI.RemoteAdvertisedSpeedList();
                            }
#endif
                        }

                    _loggedOneError = false;
                    Thread.Sleep(500);
                }
                catch (NotImplementedException)
                {
                    _selectedDeviceStore.OnViewModelFeedbackLog("Function not implemented for this board.", Helper.Feedback.FeedbackType.Error);
                }
                catch (ApplicationException ex)
                {
                    string errorMsg = ex.Message;
                    if (!_loggedOneError)
                        _selectedDeviceStore.OnViewModelFeedbackLog(errorMsg, Helper.Feedback.FeedbackType.Error);
                    _loggedOneError = true;
                }
                catch (Exception)
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
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Checker = status;
        }

        private void _selectedDeviceStore_PortNumChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            OnPropertyChanged(nameof(MaxSlicerError));
            OnPropertyChanged(nameof(SpikeCount));
        }
        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            Debug.WriteLine($"[{SerialNumber}] Selected");

            OnPropertyChanged(nameof(BoardName));
            OnPropertyChanged(nameof(SerialNumber));
            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(AnStatus));
            OnPropertyChanged(nameof(Is1100Visible));
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
            OnPropertyChanged(nameof(IsSpikeCountVisible));
            OnPropertyChanged(nameof(MaxSlicerError));
            OnPropertyChanged(nameof(SpikeCount));
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