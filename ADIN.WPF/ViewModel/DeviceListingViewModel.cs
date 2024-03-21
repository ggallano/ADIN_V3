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
        //private FeedbackModel _feedback;
        private IFTDIServices _ftdiService;

        private WqlEventQuery _insertQuery;
        private ManagementEventWatcher _insertWatcher;
        private WqlEventQuery _removeQuery;
        private ManagementEventWatcher _removeWatcher;
        private DeviceListingItemViewModel _selectedDeviceListingItemViewModel;
        //private object _thisLock;

        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="selectedDeviceStore">selected device store</param>
        /// <param name="ftdiService">ftdi service</param>
        public DeviceListingViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;

            _deviceListingViewModels = new ObservableCollection<DeviceListingItemViewModel>();
            //_feedback = new FeedbackModel();

            //CheckConnectedDevice();

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

        //public FeedbackModel Feedback
        //{
        //    get { return _feedback; }
        //    set
        //    {
        //        _feedback = value;
        //        OnPropertyChanged(nameof(Feedback));
        //    }
        //}

        public ICommand RefreshCommand { get; set; }

        /// <summary>
        /// gets or sets the selected device listing item viewmodel
        /// </summary>
        public DeviceListingItemViewModel SelectedDeviceListingItemViewModel
        {
            get { return _selectedDeviceListingItemViewModel; }
            set
            {
                //lock (_thisLock)
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
            //lock (_thisLock)
            {
                if (!_ftdiService.IsComOpen)
                {
                    InsertNewDevice();
                    return;
                }

                var serialNum = _ftdiService.GetSerialNumber();
                _ftdiService.Close();
                //InsertNewDevice();
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
                //lock (_thisLock)
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
                if (!ADINConfirmBoard.ConfirmADINBoard(currentNewDevice.Description))
                    continue;
                if (currentNewDevice.Description == "")
                    continue;
                if (previousDetectedDevices.Contains(currentNewDevice.SerialNumber))
                    continue;

                _ftdiService.Open(currentNewDevice.SerialNumber);

                ADINDevice adin = ADINConfirmBoard.GetADINBoard(currentNewDevice.Description, _ftdiService);
                if (adin != null)
                {
                    adin.Device.SerialNumber = currentNewDevice.SerialNumber;
                    adin.Device.BoardName = currentNewDevice.Description;
                }

                _ftdiService.Close();

                Application.Current.Dispatcher.Invoke(new Action (() =>
                {
                    _deviceListingViewModels.Add(new DeviceListingItemViewModel(adin));
                    //_feedback.Message = $"Device Added: {device.SerialNumber}";
                    //_feedback.FeedBackType = FeedbackType.Verbose;
                    //_logActivityViewModel.SetFeedback(_feedback);
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
                    //_feedback.message = $"device removed: {removedevice[0].serialnumber}";
                    //_feedback.feedbacktype = feedbacktype.info;
                    //_logactivityviewmodel.setfeedback(_feedback);
                    _deviceListingViewModels.Remove(removeDevice[0]);
                }));
            }
        }
    }
}