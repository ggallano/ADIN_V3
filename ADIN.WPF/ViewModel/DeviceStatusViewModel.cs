using ADIN.Device.Models;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace ADIN.WPF.ViewModel
{
    public class DeviceStatusViewModel : ViewModelBase
    {
        private string _anStatus = "-";
        private BackgroundWorker _backgroundWorker;
        private string _checker = "-";
        private IFTDIServices _ftdiService;
        private string _generator = "-";
        private string _linkLength;
        private string _linkStatus = "-";
        private string _masterSlaveStatus = "-";
        private string _mseValue = "-";
        private BackgroundWorker _readRegisterWorker;
        private SelectedDeviceStore _selectedDeviceStore;
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

            //SetBackgroundWroker();
            //SetRegsiterWorker();

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        //public string AdvertisedSpeed => _selectedDevice?.AdvertisedSpeed ?? "-";

        public string AnStatus
        {
            get { return _anStatus; }
            set
            {
                _anStatus = value;
                OnPropertyChanged(nameof(AnStatus));
            }
        }

        //public string BoardName => _selectedDevice?.Name ?? "No Device";

        //public string Checker
        //{
        //    get { return _selectedDevice?.Checker ?? "-"; }
        //    set
        //    {
        //        if (_selectedDevice != null)
        //        {
        //            _checker = value;
        //            _selectedDevice.Checker = value;
        //        }
        //        OnPropertyChanged(nameof(Checker));
        //    }
        //}

        //public string DeviceType => _selectedDevice?.EvalBoardType.ToString() ?? "-";

        public string Generator
        {
            get { return _generator; }
            set
            {
                _generator = value;
                OnPropertyChanged(nameof(Generator));
            }
        }

        public string LinkLength
        {
            get { return _linkLength; }
            set
            {
                _linkLength = value;
                OnPropertyChanged(nameof(LinkLength));
            }
        }

        public string LinkStatus
        {
            get { return _linkStatus; }
            set
            {
                switch ((EthPhyState)Enum.Parse(typeof(EthPhyState), value))
                {
                    case EthPhyState.Powerdown:
                        break;

                    case EthPhyState.Standby:
                        break;

                    case EthPhyState.LinkDown:
                        break;

                    case EthPhyState.LinkUp:
                        break;

                    default:
                        break;
                }
                _linkStatus = value;
                OnPropertyChanged(nameof(LinkStatus));
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

        public string MseValue
        {
            get { return _mseValue; }
            set
            {
                _mseValue = value;
                OnPropertyChanged(nameof(MseValue));
            }
        }

        //public string PhyAddress => _selectedDevice?.PhyAddress.ToString() ?? "-";
        public string SerialNumber => _selectedDevice?.SerialNumber ?? "-";

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
                    lock (_thisLock)
                    {
                        if (_selectedDevice != null && _ftdiService.IsComOpen)
                        {
                            //_selectedDevice.FirmwareAPI.ReadRegsiters();
                            //_selectedDeviceStore.OnRegistersValueChanged();

                            // UI Control Update
                            //_selectedDevice.FirmwareAPI.GetNegotiationMasterSlaveInitialization(true);
                            //_selectedDevice.FirmwareAPI.GetPeakVoltageInitialization(true);
                            //_selectedDevice.FirmwareAPI.GetTestModeInitialization(true);
                            //_selectedDevice.FirmwareAPI.GetLoopbackInitialization(true);
                            //_selectedDevice.FirmwareAPI.GetFrameContentInitialization(true);

                            // Device Status
                            //LinkStatus = _selectedDevice.FwAPI.GetLinkStatus();
                            //AnStatus = _selectedDevice.FwAPI.GetAnStatus();
                            //MasterSlaveStatus = _selectedDevice.FwAPI.GetMasterSlaveStatus();
                            //TxLevelStatus = _selectedDevice.FwAPI.GetTxLevelStatus();
                            //MseValue = _selectedDevice.FwAPI.GetMseValue();

                            //_selectedDevice.FwAPI.GetFrameCheckerStatus();
                            //Generator = _selectedDevice.FwAPI.GetFrameGeneratorStatus();
                        }
                    }

                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                }
            }
            e.Result = "Done";
        }

        private void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("Progress Changed");
        }

        private void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("_backgroundWorker Completed");
        }

        private void _readRegisterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_readRegisterWorker.CancellationPending)
            {
                try
                {
                    lock (_thisLock)
                    {
                        if (_selectedDevice != null && _ftdiService.IsComOpen)
                            //_selectedDevice.FwAPI.ReadRegsiters();

                        _selectedDeviceStore.OnRegistersValueChanged();
                    }
                    Thread.Sleep(50);
                }
                catch (Exception)
                {
                }
                e.Result = "Done";
            }
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
            //Checker = status;
        }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            Debug.WriteLine($"[{SerialNumber}] Selected");

            //OnPropertyChanged(nameof(BoardName));
            OnPropertyChanged(nameof(SerialNumber));
            OnPropertyChanged(nameof(LinkStatus));
            OnPropertyChanged(nameof(AnStatus));
            OnPropertyChanged(nameof(MasterSlaveStatus));
            OnPropertyChanged(nameof(MseValue));
            OnPropertyChanged(nameof(TxLevelStatus));
            //OnPropertyChanged(nameof(DeviceType));
            //OnPropertyChanged(nameof(PhyAddress));
            //OnPropertyChanged(nameof(AdvertisedSpeed));
            //OnPropertyChanged(nameof(Checker));
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

        private void SetRegsiterWorker()
        {
            _readRegisterWorker = new BackgroundWorker();
            _readRegisterWorker.WorkerReportsProgress = true;
            _readRegisterWorker.WorkerSupportsCancellation = true;

            _readRegisterWorker.DoWork += _readRegisterWorker_DoWork;
            _readRegisterWorker.RunWorkerCompleted += _readRegisterWorker_RunWorkerCompleted;
            _readRegisterWorker.ProgressChanged += _readRegisterWorker_ProgressChanged;

            _readRegisterWorker.RunWorkerAsync();
        }
    }
}