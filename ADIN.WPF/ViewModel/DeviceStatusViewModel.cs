// <copyright file="DeviceStatusViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class DeviceStatusViewModel : ViewModelBase
    {
        private string _advertisedSpeed = "-";
        private string _anStatus = "-";
        private BackgroundWorker _backgroundWorker;
        private string _checker = "-";
        private IFTDIServices _ftdiService;
        private string _generator = "-";
        private string _linkStatus = "-";
        private List<string> _localAdvertisedSpeeds = new List<string>() { "" };
        private bool _loggedOneError = false;
        private object _mainLock;
        private string _masterSlaveStatus = "-";
        private MseModel _mseValue = new MseModel("-");
        private List<string> _remoteAdvertisedSpeeds = new List<string>();
        private SelectedDeviceStore _selectedDeviceStore;
        private string _speedMode = "-";
        private string _txLevelStatus = "-";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceStatusViewModel"/> class.
        /// </summary>
        /// <param name="selectedDeviceStore">selected device store</param>
        /// <param name="ftdiService">FTDI service</param>
        /// <param name="mainLock">mainLock</param>
        public DeviceStatusViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, object mainLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;
            _mainLock = mainLock;

            ResetSlicerCommand = new ResetSlicerErrorCommand(this, selectedDeviceStore);
            ResetSpikeCommand = new ResetSpikeCountCommand(this, selectedDeviceStore);

            SetBackgroundWroker();

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.FrameGenCheckerResetDisplay += _selectedDeviceStore_FrameGenCheckerResetDisplay;
            _selectedDeviceStore.PortNumChanged += _selectedDeviceStore_PortNumChanged;
        }

        public ICommand ResetSlicerCommand { get; set; }

        public ICommand ResetSpikeCommand { get; set; }

        public string AdvertisedSpeed
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice == null)
                    return "-";
                return _advertisedSpeed;
            }

            set
            {
                _advertisedSpeed = value;
                OnPropertyChanged(nameof(AdvertisedSpeed));
            }
        }

        public string AnStatus
        {
            get
            {
                return _anStatus;
            }

            set
            {
                _anStatus = value;
                OnPropertyChanged(nameof(AnStatus));
            }
        }

        public string BoardName => _selectedDeviceStore.SelectedDevice?.BoardName ?? "No Device";

        public string Checker
        {
            get
            {
                return _selectedDevice?.Checker ?? "-";
            }

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

#if !DISABLE_TSN && !DISABLE_T1L

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

        public bool IsGigabitBoard => !IsT1LBoard;

#elif !DISABLE_TSN
        public bool IsGigabitBoard { get; } = true;
        public bool IsT1LBoard { get; } = false;
#elif !DISABLE_T1L
        public bool IsGigabitBoard { get; } = false;

        public bool IsT1LBoard { get; } = true;
#endif

        public bool IsSpikeCountVisible => (_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN2111) || (_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1110);

        public bool IsVisibleSpeedList
        {
            get
            {
                return _speedMode == "Advertised" && IsGigabitBoard;
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
                        _selectedDeviceStore.OnLinkStatusChanged(EthPhyState.Powerdown);
                        _selectedDeviceStore.OnSoftwarePowerDownChanged("Software Power Up");
                        break;

                    case EthPhyState.Standby:
                        _selectedDeviceStore.OnLinkStatusChanged(EthPhyState.Standby);
                        _selectedDeviceStore.OnSoftwarePowerDownChanged("Software Power Down");
                        break;

                    case EthPhyState.LinkDown:
                        _selectedDeviceStore.OnLinkStatusChanged(EthPhyState.LinkDown);
                        _selectedDeviceStore.OnSoftwarePowerDownChanged("Software Power Down");
                        break;

                    case EthPhyState.LinkUp:
                        _selectedDeviceStore.OnLinkStatusChanged(EthPhyState.LinkUp);
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
            get
            {
                if (_selectedDeviceStore.SelectedDevice == null)
                    return new List<string>();
                return _localAdvertisedSpeeds;
            }

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
            get
            {
                return _masterSlaveStatus;
            }

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
                    return "-";

                if (_selectedDeviceStore.SelectedDevice.BoardRev == BoardRevision.Rev0
                 && _selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1100_S1)
                    return "N/A";

                return _selectedDeviceStore.SelectedDevice.MaxSlicerError.ToString();
            }

            set
            {
                if (Convert.ToDouble(value) != _selectedDeviceStore.SelectedDevice.MaxSlicerError)
                {
                    _selectedDeviceStore.SelectedDevice.MaxSlicerError = Convert.ToDouble(value);
                    OnPropertyChanged(nameof(MaxSlicerError));
                }
            }
        }

        public MseModel MseValue
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice != null)
                {
                    return _mseValue;
                }

                return new MseModel("-");
            }

            set
            {
                if (value != _mseValue)
                {
                    _mseValue = value;
                }

                OnPropertyChanged(nameof(MseValue));
            }
        }

        public string PhyAddress => _selectedDeviceStore.SelectedDevice?.PhyAddress.ToString() ?? "-";

        public List<string> RemoteAdvertisedSpeeds
        {
            get
            {
                if (_selectedDeviceStore.SelectedDevice == null)
                    return new List<string>();
                return _remoteAdvertisedSpeeds;
            }

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
                    return "-";

                if (_selectedDeviceStore.SelectedDevice.BoardRev == BoardRevision.Rev0
                 && _selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1100_S1)
                    return "N/A";

                return _selectedDeviceStore.SelectedDevice.SpikeCountPort.ToString();
            }

            set
            {
                if (Convert.ToDouble(value) != _selectedDeviceStore.SelectedDevice.SpikeCountPort)
                {
                    _selectedDeviceStore.SelectedDevice.SpikeCountPort = Convert.ToDouble(value);
                    OnPropertyChanged(nameof(SpikeCount));
                }
            }
        }

        public string TxLevelStatus
        {
            get
            {
                return _txLevelStatus;
            }

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
                            AdvertisedSpeed = _selectedDevice.FwAPI.AdvertisedSpeed();
                            LocalAdvertisedSpeeds = _selectedDevice.FwAPI.LocalAdvertisedSpeedList();
                            _selectedDevice.FwAPI.GetFrameCheckerStatus();
                            Generator = _selectedDevice.FwAPI.GetFrameGeneratorStatus();

                            // Specific ADIN Device Status
#if !DISABLE_T1L
                            if (_selectedDevice.FwAPI is ADIN1100FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN1100FirmwareAPI;
                                AnStatus = fwAPI.GetAnStatus();
                                MasterSlaveStatus = fwAPI.GetMasterSlaveStatus();
                                TxLevelStatus = fwAPI.GetTxLevelStatus();
                                MseValue = fwAPI.GetMseValue(_selectedDevice.BoardRev);

                                if (_selectedDevice.BoardRev != BoardRevision.Rev0)
                                {
                                    MaxSlicerError = fwAPI.GetMaxSlicer().ToString("0.00");
                                    SpikeCount = fwAPI.GetSpikeCount().ToString();
                                }
                            }
                            else if (_selectedDevice.FwAPI is ADIN1110FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN1110FirmwareAPI;
                                AnStatus = fwAPI.GetAnStatus();
                                MasterSlaveStatus = fwAPI.GetMasterSlaveStatus();
                                TxLevelStatus = fwAPI.GetTxLevelStatus();
                                MseValue = fwAPI.GetMseValue(_selectedDevice.BoardRev);
                                MaxSlicerError = fwAPI.GetMaxSlicer().ToString("0.00");
                                SpikeCount = fwAPI.GetSpikeCount().ToString();
                            }
                            else if (_selectedDevice.FwAPI is ADIN2111FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN2111FirmwareAPI;
                                AnStatus = fwAPI.GetAnStatus();
                                MasterSlaveStatus = fwAPI.GetMasterSlaveStatus();
                                TxLevelStatus = fwAPI.GetTxLevelStatus();
                                MseValue = fwAPI.GetMseValue(_selectedDevice.BoardRev);
                                MaxSlicerError = fwAPI.GetMaxSlicer().ToString("0.00");
                                SpikeCount = fwAPI.GetSpikeCount().ToString();
                            }
                            else
                            {
                                // Do nothing
                            }
#endif
#if !DISABLE_TSN
                            if (_selectedDevice.FwAPI is ADIN1200FirmwareAPI || _selectedDevice.FwAPI is ADIN1300FirmwareAPI)
                            {
                                MseValue = _selectedDevice.FwAPI.GetMseValue();
                                SpeedMode = _selectedDevice.FwAPI.GetSpeedMode();
                                RemoteAdvertisedSpeeds = _selectedDevice.FwAPI.RemoteAdvertisedSpeedList();
                            }

                            if (_selectedDevice.FwAPI is ADIN1200FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN1200FirmwareAPI;
                                fwAPI.CableDiagnosticsStatus();
                            }

                            if (_selectedDevice.FwAPI is ADIN1300FirmwareAPI)
                            {
                                var fwAPI = _selectedDevice.FwAPI as ADIN1300FirmwareAPI;
                                fwAPI.CableDiagnosticsStatus();
                            }
#endif
                        }

                    _loggedOneError = false;
                    Thread.Sleep(10);
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
            Debug.WriteLine($"[{SerialNumber}] Selected");

            OnPropertyChanged(nameof(IsGigabitBoard));
            OnPropertyChanged(nameof(IsT1LBoard));
            OnPropertyChanged(nameof(BoardName));
            OnPropertyChanged(nameof(SerialNumber));
            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(AnStatus));
            OnPropertyChanged(nameof(Generator));
            OnPropertyChanged(nameof(MseValue));
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