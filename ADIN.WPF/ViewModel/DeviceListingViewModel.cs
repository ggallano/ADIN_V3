using ADI.Register.Services;
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Commands;
using ADIN.WPF.Models;
using ADIN.WPF.Stores;
using FTD2XX_NET;
using FTDIChip.Driver.Services;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Input;

namespace ADIN.WPF.ViewModel
{
    public class DeviceListingViewModel : ViewModelBase
    {
        //private const string INSERT_QUERY = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2";
        private const string INSERT_QUERY = "SELECT * FROM __InstanceCreationEvent " + "WITHIN 2 " + "WHERE TargetInstance ISA 'Win32_PnPEntity'";
        private const string REMOVE_QUERY = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3";

        private readonly IRegisterService _registerService;
        private readonly SelectedDeviceStore _selectedDeviceStore;
        private ObservableCollection<DeviceListingItemViewModel> _deviceListingViewModels;
        private List<ADINDeviceModel> _devices;
        private FeedbackModel _feedback;
        private IFTDIServices _ftdiService;

        private WqlEventQuery _insertQuery;
        private ManagementEventWatcher _insertWatcher;
        private LogActivityViewModel _logActivityViewModel;
        private WqlEventQuery _removeQuery;
        private ManagementEventWatcher _removeWatcher;
        private DeviceListingItemViewModel _selectedDeviceListingItemViewModel;
        private object _thisLock;

        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="selectedDeviceStore">selected device store</param>
        /// <param name="ftdiService">ftdi service</param>
        public DeviceListingViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, IRegisterService registerService, LogActivityViewModel logActivityViewModel, object thisLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;
            _registerService = registerService;
            _logActivityViewModel = logActivityViewModel;
            _thisLock = thisLock;

            _deviceListingViewModels = new ObservableCollection<DeviceListingItemViewModel>();
            _devices = new List<ADINDeviceModel>();
            _feedback = new FeedbackModel();

            RefreshCommand = new RefreshCommand(this);
            CheckConnectedDevice();

            _insertQuery = new WqlEventQuery(INSERT_QUERY);
            _insertWatcher = new ManagementEventWatcher(_insertQuery);
            _insertWatcher.EventArrived += _insertWatcher_EventArrived;
            _insertWatcher.Start();

            _removeQuery = new WqlEventQuery(REMOVE_QUERY);
            _removeWatcher = new ManagementEventWatcher(_removeQuery);
            _removeWatcher.EventArrived += _removeWatcher_EventArrived;
            _removeWatcher.Start();
        }

        /// <summary>
        /// gets the device listing viewmodel
        /// </summary>
        public ObservableCollection<DeviceListingItemViewModel> DeviceListingItemViewModels => _deviceListingViewModels;

        public FeedbackModel Feedback
        {
            get { return _feedback; }
            set
            {
                _feedback = value;
                OnPropertyChanged(nameof(Feedback));
            }
        }

        public ICommand RefreshCommand { get; set; }

        /// <summary>
        /// gets or sets the selected device listing item viewmodel
        /// </summary>
        public DeviceListingItemViewModel SelectedDeviceListingItemViewModel
        {
            get { return _selectedDeviceListingItemViewModel; }
            set
            {
                lock (_thisLock)
                {
                    _ftdiService.Close();
                    _selectedDeviceListingItemViewModel = value;
                    if (value != null)
                    {
                        _selectedDeviceStore.SelectedDevice = _selectedDeviceListingItemViewModel.Device;
                        _ftdiService.Open(_selectedDeviceStore.SelectedDevice.SerialNumber);
                    }
                    else
                    {
                        _selectedDeviceStore.SelectedDevice = null;
                    }
                }
                OnPropertyChanged(nameof(SelectedDeviceListingItemViewModel));
            }
        }

        /// <summary>
        /// checks if there are devices that is connected already
        /// </summary>
        internal void CheckConnectedDevice()
        {
            InsertNewDevice();
        }

        protected override void Dispose()
        {
            _insertWatcher.EventArrived -= _insertWatcher_EventArrived;
            _removeWatcher.EventArrived -= _removeWatcher_EventArrived;
            base.Dispose();
        }

        /// <summary>
        /// Insert USB Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _insertWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            Debug.WriteLine("=================== Insert Event Fired ==============================");
            lock (_thisLock)
            {
                if (!_ftdiService.IsComOpen)
                {
                    InsertNewDevice();
                    return;
                }

                var serialNum = _ftdiService.GetSerialNumber();
                _ftdiService.Close();
                InsertNewDevice();
                _ftdiService.Open(_selectedDeviceListingItemViewModel.SerialNumber);
            }
        }

        /// <summary>
        /// Removal USB Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _removeWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            List<string> connectedDevices = new List<string>();
            try
            {
                lock (_thisLock)
                {
                    if (!_ftdiService.IsComOpen)
                    {
                        RemoveDevice();
                        return;
                    }
                    if (_ftdiService.GetSerialNumber() != string.Empty)
                    {
                        RemoveDevice();
                        return;
                    }

                    _ftdiService.Close();

                    SelectedDeviceListingItemViewModel = null;
                    RemoveDevice();

                    //do
                    //{
                    //    connectedDevices = GetConnectedDevices();
                    //    if (connectedDevices.Count == 0)
                    //        break;
                    //    if (connectedDevices[0] != "")
                    //        break;

                    //} while (true);

                    //if (connectedDevices.Count >= 0)
                    //{
                    //    var result = DeviceListingItemViewModels.Where(x => x.SerialNumber == connectedDevices[0]).ToList()[0];
                    //    Application.Current.Dispatcher.Invoke(() =>
                    //    {
                    //        SelectedDeviceListingItemViewModel = result;
                    //    });
                    //}
                    //else
                    //    SelectedDeviceListingItemViewModel = null;
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SelectedDeviceListingItemViewModel = null;
                }));
                Debug.WriteLine(ex.Message);
            }
        }

        private List<string> GetConnectedDevices()
        {
            return (_ftdiService.GetDeviceList().ToList()).Select(y => y.SerialNumber).ToList();
        }

        private List<string> GetPreviousConnectedDevices()
        {
            return _deviceListingViewModels.Select(x => x.SerialNumber).ToList();
        }

        /// <summary>
        /// Inserts the new device connected
        /// </summary>
        /// <param name="newDevices">lists of connected devices</param>
        /// <param name="previousDetectedDevices">previous list of devices</param>
        private void InsertNewDevice()
        {
            FTDI.FT_DEVICE_INFO_NODE[] newDevices = _ftdiService.GetDeviceList();
            List<string> previousDetectedDevices = _deviceListingViewModels.Select(x => x.SerialNumber).ToList();

            foreach (var currentNewDevice in newDevices)
            {
                if (!ADIN1100ConfirmBoard.ConfirmADINBoard(currentNewDevice.Description))
                    continue;
                if (currentNewDevice.Description == "")
                    continue;
                if (previousDetectedDevices.Contains(currentNewDevice.SerialNumber))
                    continue;

                _ftdiService.Open(currentNewDevice.SerialNumber);
                ADINDeviceModel device = new ADINDeviceModel(currentNewDevice.SerialNumber, currentNewDevice.Description, _ftdiService, _registerService);

                //Link Properties
                device.LinkProperty.AutoNegMasterSlaveAdvertisements.Add(new AutoNegMasterSlaveAdvertisementModel() { Name = AutoNegMasterSlaveAdvertisementItem.Prefer_Master.ToString() });
                device.LinkProperty.AutoNegMasterSlaveAdvertisements.Add(new AutoNegMasterSlaveAdvertisementModel() { Name = AutoNegMasterSlaveAdvertisementItem.Prefer_Slave.ToString() });
                device.LinkProperty.AutoNegMasterSlaveAdvertisements.Add(new AutoNegMasterSlaveAdvertisementModel() { Name = AutoNegMasterSlaveAdvertisementItem.Forced_Master.ToString() });
                device.LinkProperty.AutoNegMasterSlaveAdvertisements.Add(new AutoNegMasterSlaveAdvertisementModel() { Name = AutoNegMasterSlaveAdvertisementItem.Forced_Slave.ToString() });
                var result = device.FirmwareAPI.GetNegotiationMasterSlaveInitialization().ToString();
                device.LinkProperty.AutoNegMasterSlaveAdvertisement = device.LinkProperty.AutoNegMasterSlaveAdvertisements.Where(x => x.Name == result).ToList()[0];

                device.LinkProperty.AutoNegTxLevelAdvertisements.Add(new AutoNegTxLevelAdvertisementModel() { Name = PeakVoltageAdvertisementItem.Capable2p4Volts_Requested2p4Volts.ToString() });
                device.LinkProperty.AutoNegTxLevelAdvertisements.Add(new AutoNegTxLevelAdvertisementModel() { Name = PeakVoltageAdvertisementItem.Capable2p4Volts_Requested1Volt.ToString() });
                device.LinkProperty.AutoNegTxLevelAdvertisements.Add(new AutoNegTxLevelAdvertisementModel() { Name = PeakVoltageAdvertisementItem.Capable1Volt.ToString() });
                result = device.FirmwareAPI.GetPeakVoltageInitialization().ToString();
                device.LinkProperty.AutoNegTxLevelAdvertisement = device.LinkProperty.AutoNegTxLevelAdvertisements.Where(x => x.Name == result).ToList()[0];

                //FrameGenChecker
                device.FrameGenChecker.EnableContinuousMode = false;
                device.FrameGenChecker.EnableMacAddress = false;
                device.FrameGenChecker.FrameBurst = 64001;
                device.FrameGenChecker.FrameLength = 1250;
                device.FrameGenChecker.FrameContents.Add(new FrameContentModel() { Name = "Random", FrameContentType = FrameType.Random });
                device.FrameGenChecker.FrameContents.Add(new FrameContentModel() { Name = "All 0s", FrameContentType = FrameType.All0s });
                device.FrameGenChecker.FrameContents.Add(new FrameContentModel() { Name = "All 1s", FrameContentType = FrameType.All1s });
                device.FrameGenChecker.FrameContents.Add(new FrameContentModel() { Name = "Alt 10s", FrameContentType = FrameType.Alt10s });
                var frameType = device.FirmwareAPI.GetFrameContentInitialization();
                device.FrameGenChecker.FrameContent = device.FrameGenChecker.FrameContents.Where(x => x.FrameContentType == frameType).ToList()[0];
                device.FrameGenChecker.SrcMacAddress = ":::::";
                device.FrameGenChecker.DestMacAddress = ":::::";

                //Test modes
                device.TestMode.TestModes.Add(new TestModeListingModel() { Name1 = "10BASE-T1L Normal Mode", Name2 = "", Description = "PHY is in normal mode", TestmodeType = TestModeType.Normal });
                device.TestMode.TestModes.Add(new TestModeListingModel() { Name1 = "10BASE-T1L Test Mode 1:", Name2 = "Tx output voltage, Tx clock frequency and jitter.", Description = "PHY repeatedly transmit the data symbol sequence (+1,-1)", TestmodeType = TestModeType.Test1 });
                device.TestMode.TestModes.Add(new TestModeListingModel() { Name1 = "10BASE-T1L Test Mode 2:", Name2 = "Tx output droop", Description = "PHY Transmit ten '+1' symbols followed by ten '-1' symbols", TestmodeType = TestModeType.Test2 });
                device.TestMode.TestModes.Add(new TestModeListingModel() { Name1 = "10BASE-T1L Test Mode 3:", Name2 = "Power Spectral Density (PSD) and power level", Description = "PHY transmit as in non-test operation and in the MASTER data mode with data set to normal Inter-Frame idle signals", TestmodeType = TestModeType.Test3 });
                device.TestMode.TestModes.Add(new TestModeListingModel() { Name1 = "10BASE-T1L Transmit Disable:", Name2 = "MDI Return Loss", Description = "PHY's receive and transmit paths as in notmal operation but PHY transmits 0 symbols continuously", TestmodeType = TestModeType.Transmit });
                var testmodeResult = device.FirmwareAPI.GetTestModeInitialization();
                device.TestMode.TestMode = device.TestMode.TestModes.Where(x => x.TestmodeType == testmodeResult).ToList()[0];

                //Loopback
                device.Loopback.Loopbacks.Add(new LoopbackListingModel() { Name = "None", EnumLoopbackType = LoopBackMode.OFF, ImagePath = @"../Images/loopback/NoLoopback.png" });
                device.Loopback.Loopbacks.Add(new LoopbackListingModel() { Name = "MAC I/F Remote", EnumLoopbackType = LoopBackMode.MacRemote, ImagePath = @"../Images/loopback/MACRemoteLoopback.png" });
                device.Loopback.Loopbacks.Add(new LoopbackListingModel() { Name = "MAC I/F", EnumLoopbackType = LoopBackMode.MAC, ImagePath = @"../Images/loopback/MACLoopback.png" });
                device.Loopback.Loopbacks.Add(new LoopbackListingModel() { Name = "PCS", EnumLoopbackType = LoopBackMode.Digital, ImagePath = @"../Images/loopback/PCSLoopback.png" });
                device.Loopback.Loopbacks.Add(new LoopbackListingModel() { Name = "PMA", EnumLoopbackType = LoopBackMode.LineDriver, ImagePath = @"../Images/loopback/PMALoopback.png" });
                device.Loopback.Loopbacks.Add(new LoopbackListingModel() { Name = "External MII/RMII", EnumLoopbackType = LoopBackMode.ExtCable, ImagePath = @"../Images/loopback/ExternalLoopback.png" });
                var loopbackResult = device.FirmwareAPI.GetLoopbackInitialization();
                device.Loopback.Loopback = device.Loopback.Loopbacks.Where(x => x.EnumLoopbackType == loopbackResult).ToList()[0];

                //Fault Detector
                device.FaultDetector.CableDiagnostics.NVP = Convert.ToDecimal(device.FirmwareAPI.GetNvp(), CultureInfo.InvariantCulture);
                device.FaultDetector.CableDiagnostics.CableOffset = Convert.ToDecimal(device.FirmwareAPI.GetOffset(), CultureInfo.InvariantCulture);

                _ftdiService.Close();

                Application.Current.Dispatcher.Invoke(new Action (() =>
                {
                    _deviceListingViewModels.Add(new DeviceListingItemViewModel(device));
                    _feedback.Message = $"Device Added: {device.SerialNumber}";
                    _feedback.FeedBackType = FeedbackType.Verbose;
                    _logActivityViewModel.SetFeedback(_feedback);
                }));
            }
        }

        /// <summary>
        /// Removes the device in the current list of devices
        /// </summary>
        /// <param name="connectedDevices">connected devices</param>
        /// <param name="previousDetectedDevices">previous list of devices</param>
        private void RemoveDevice()
        {
            List<string> connectedDevices = GetConnectedDevices();
            List<string> previousDetectedDevices = GetPreviousConnectedDevices();

            foreach (var stillConnectedDevice in previousDetectedDevices)
            {
                if (connectedDevices.Contains(stillConnectedDevice))
                    continue;

                var removeDevice = _deviceListingViewModels.Where(a => a.SerialNumber == stillConnectedDevice).ToList();

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    _feedback.Message = $"Device Removed: {removeDevice[0].SerialNumber}";
                    _feedback.FeedBackType = FeedbackType.Info;
                    _logActivityViewModel.SetFeedback(_feedback);
                    DeviceListingItemViewModels.Remove(removeDevice[0]);
                }));
            }
        }
    }
}