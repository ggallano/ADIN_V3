using ADIN.Avalonia.Stores;
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.Register.Services;
using Avalonia.Threading;
using FTD2XX_NET;
using FTDIChip.Driver.Services;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace ADIN.Avalonia.ViewModels
{
    public class DeviceListingViewModel : ViewModelBase
    {
        //private const string INSERT_QUERY = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2";
        private const string INSERT_QUERY = "SELECT * FROM __InstanceCreationEvent " + "WITHIN 2 " + "WHERE TargetInstance ISA 'Win32_PnPEntity'";
        private const string REMOVE_QUERY = "SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3";

        private readonly IRegisterService _registerService;
        private readonly SelectedDeviceStore _selectedDeviceStore;
        private ObservableCollection<DeviceListingItemViewModel> _deviceListingViewModels;
        private bool _enableSelectDevice = true;
        private FeedbackModel _feedback;
        private IFTDIServices _ftdiService;

        private WqlEventQuery _insertQuery;
        private ManagementEventWatcher _insertWatcher;
        private LogActivityViewModel _logActivityViewModel;
        private object _mainLock;
        private WqlEventQuery _removeQuery;
        private ManagementEventWatcher _removeWatcher;
        private DeviceListingItemViewModel _selectedDeviceListingItemViewModel;
        private bool _isMultiChipSupport = true;
        private SelectedDeviceStore selectedDeviceStore;
        private IFTDIServices ftdiService;
        private IRegisterService registerService;
        private LogActivityViewModel logActivityVM;
        private object mainLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceListingViewModel"/> class.
        /// </summary>
        /// <param name="selectedDeviceStore">selected device store</param>
        /// <param name="ftdiService">ftdi service</param>
        public DeviceListingViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, IRegisterService registerService, LogActivityViewModel logActivityViewModel, object mainLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;
            _registerService = registerService;
            _logActivityViewModel = logActivityViewModel;
            _mainLock = mainLock;

            _deviceListingViewModels = new ObservableCollection<DeviceListingItemViewModel>();
            _feedback = new FeedbackModel();

            CheckConnectedDevice();

            //ADINDevice adin = new ADINDevice(new ADIN1300Model(_ftdiService, _registerService));
            //adin.Device.SerialNumber = "SerialNumber";
            //adin.Device.BoardName = "Description";

            //_deviceListingViewModels.Add(new DeviceListingItemViewModel(adin));

            _insertQuery = new WqlEventQuery(INSERT_QUERY);
            _insertWatcher = new ManagementEventWatcher(_insertQuery);
            _insertWatcher.EventArrived += _insertWatcher_EventArrived;
            _insertWatcher.Start();

            _removeQuery = new WqlEventQuery(REMOVE_QUERY);
            _removeWatcher = new ManagementEventWatcher(_removeQuery);
            _removeWatcher.EventArrived += _removeWatcher_EventArrived;
            _removeWatcher.Start();

            ItemSamples = new ObservableCollection<string>()
            {
                "Item1",
                "Item2",
                "Item3"
            };
            OnPropertyChanged(nameof(ItemSamples));

            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
        }

        /// <summary>
        /// gets the device listing viewmodel.
        /// </summary>
        public ObservableCollection<DeviceListingItemViewModel> DeviceListingItemViewModels => _deviceListingViewModels;

        private ObservableCollection<string> _items;
        public ObservableCollection<string> ItemSamples
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged(nameof(ItemSamples));
            }
        }

        public bool EnableSelectDevice
        {
            get { return _enableSelectDevice; }

            set
            {
                _enableSelectDevice = value;
                OnPropertyChanged(nameof(EnableSelectDevice));
            }
        }

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
                lock (_mainLock)
                {
                    _ftdiService.Close();
                    _selectedDeviceListingItemViewModel = value;
                    if (value != null)
                    {
                        _selectedDeviceStore.SelectedDevice = _selectedDeviceListingItemViewModel.Device;
                        CheckCableDiagT1L();
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

            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;

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
            lock (_mainLock)
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
            Debug.WriteLine("=================== Remove Event Fired ==============================");
            List<string> connectedDevices = new List<string>();
            try
            {
                lock (_mainLock)
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
                Dispatcher.UIThread.Invoke(new Action(() =>
                {
                    SelectedDeviceListingItemViewModel = null;
                }));
                Debug.WriteLine(ex.Message);
            }
        }

        private void _selectedDeviceStore_OnGoingCalibrationStatusChanged(bool onGoingCalibrationStatus)
        {
            Dispatcher.UIThread.Invoke(new Action(() =>
            {
                EnableSelectDevice = !onGoingCalibrationStatus;
            }));
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
                if (!ADINConfirmBoard.ConfirmADINBoard(currentNewDevice.Description))
                    continue;
                if (currentNewDevice.Description == "")
                    continue;
                if (previousDetectedDevices.Contains(currentNewDevice.SerialNumber))
                    continue;

                _ftdiService.Open(currentNewDevice.SerialNumber);

                //var isMultiChipSupported = Properties.Settings.Default.MultiChipSupport;
                var isMultiChipSupported = _isMultiChipSupport;

                try
                {
                    List<ADINDevice> adin = ADINConfirmBoard.GetADINBoard(currentNewDevice.Description, _ftdiService, _registerService, _mainLock, isMultiChipSupported);

                    foreach (var adinSubDevice in adin)
                    {
                        adinSubDevice.Device.SerialNumber = currentNewDevice.SerialNumber;
                        adinSubDevice.Device.BoardName = currentNewDevice.Description;

                        Dispatcher.UIThread.Invoke(new Action(() => 
                        {
                            _deviceListingViewModels.Add(new DeviceListingItemViewModel(adinSubDevice));
                            var tempstr = _deviceListingViewModels.Where(x => x.BoardType == adinSubDevice.DeviceType).Select(x => x.DeviceHeader).ToList();
                            _feedback.Message = $"Device Added: {tempstr.Last()}";
                            _feedback.FeedBackType = FeedbackType.Info;
                            _logActivityViewModel.SetFeedback(_feedback, false);
                        }));
                    }
                }
                catch (ApplicationException)
                {
                    _feedback.Message = $" {currentNewDevice.SerialNumber} Board phyaddress is not set at zero(0).";
                    _feedback.FeedBackType = FeedbackType.Error;
                    _logActivityViewModel.SetFeedback(_feedback, false);
                }

                _ftdiService.Close();
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

                Dispatcher.UIThread.Invoke((Action)(() =>
                {
                    _feedback.Message = $"Device removed: {removeDevice[0].SerialNumber}";
                    _feedback.FeedBackType = FeedbackType.Info;
                    _logActivityViewModel.SetFeedback(_feedback, false);
                    _deviceListingViewModels.Remove(removeDevice[0]);
                }));
            }
        }

        private void CheckCableDiagT1L()
        {
            if (!_selectedDeviceStore.SelectedDevice.IsADIN1100CableDiagAvailable
             && !_selectedDeviceStore.SelectedDevice.CableDiagOneTimePopUp
             && _selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1100)
            {
                _selectedDeviceStore.SelectedDevice.CableDiagOneTimePopUp = true;
                _feedback.Message = $"[{_selectedDeviceStore.SelectedDevice.SerialNumber}] ADIN1100 board requires a firmware upgrade to enable TDR fault detector.";
                _feedback.FeedBackType = FeedbackType.Warning;
                _logActivityViewModel.SetFeedback(_feedback, false);
            }
        }
    }
}
