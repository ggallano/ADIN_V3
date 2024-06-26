using ADI.Register.Services;
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace ADIN.WPF.ViewModel
{
    public class OperationViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly SelectedDeviceStore _selectedDeviceStore;

        private bool _isCableDiagSelected;
        private bool _isCableDiagVisible;
        private bool _isAdin2111 = false;

        private bool _isTestModeSelected = false;

        private bool _enableTabs = true;

        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore, IRegisterService registerService, ScriptService scriptService, object mainLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _navigationStore = navigationStore;

            MenuItemVM = new MenuItemViewModel();
            LogActivityVM = new LogActivityViewModel(_selectedDeviceStore);
            DeviceListingVM = new DeviceListingViewModel(_selectedDeviceStore, ftdiService, registerService, LogActivityVM, mainLock);
            ExtraCommandsVM = new ExtraCommandsViewModel(_selectedDeviceStore, ftdiService);
            RegisterListingVM = new RegisterListingViewModel(selectedDeviceStore, ftdiService);
            LinkPropertiesVM = new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore);
            LoopbackVM = new LoopbackViewModel(_selectedDeviceStore, ftdiService);
            FrameGenCheckerVM = new FrameGenCheckerViewModel(_selectedDeviceStore, mainLock, ftdiService);
            ClockPinControlVM = new ClockPinControlViewModel(_navigationStore, _selectedDeviceStore);
            TestModeVM = new TestModeViewModel(_selectedDeviceStore);
            DeviceStatusVM = new DeviceStatusViewModel(selectedDeviceStore, ftdiService, mainLock);
            RegisterAccessVM = new RegisterAccessViewModel(_selectedDeviceStore, navigationStore);
            TDRVM = new TimeDomainReflectometryViewModel(_selectedDeviceStore, mainLock);
            StatusStripVM = new StatusStripViewModel(_selectedDeviceStore, scriptService);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
            _selectedDeviceStore.OnGoingCalibrationStatusChanged += _selectedDeviceStore_OnGoingCalibrationStatusChanged;
        }

        public ClockPinControlViewModel ClockPinControlVM { get; set; }
        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;
        public DeviceListingViewModel DeviceListingVM { get; }
        public DeviceStatusViewModel DeviceStatusVM { get; set; }
        public ExtraCommandsViewModel ExtraCommandsVM { get; set; }
        public FrameGenCheckerViewModel FrameGenCheckerVM { get; set; }
        public StatusStripViewModel StatusStripVM { get; set; }
        
        public bool IsADIN2111
        {
            get { return _isAdin2111; }
            set
            {
                _isAdin2111 = value;
                OnPropertyChanged(nameof(IsADIN2111));
            }
        }
        
        public bool IsCableDiagSelected
        {
            get { return _isCableDiagSelected; }
            set
            {
                _isCableDiagSelected = value;
                OnPropertyChanged(nameof(IsCableDiagSelected));
            }
        }

        public bool IsCableDiagVisible
        {
            get { return _isCableDiagVisible; }
            set
            {
                _isCableDiagVisible = value;
                OnPropertyChanged(nameof(IsCableDiagVisible));
            }
        }

        public bool IsTestModeSelected
        {
            get { return _isTestModeSelected; }
            set
            {
                _isTestModeSelected = value;
                OnPropertyChanged(nameof(IsTestModeSelected));
            }
        }

        public bool EnableTabs
        {
            get { return _enableTabs; }
            set
            {
                _enableTabs = value;
                OnPropertyChanged(nameof(EnableTabs));
            }
        }

        public LinkPropertiesViewModel LinkPropertiesVM { get; set; }
        public LogActivityViewModel LogActivityVM { get; set; }
        public LoopbackViewModel LoopbackVM { get; set; }
        public RegisterAccessViewModel RegisterAccessVM { get; set; }
        public RegisterListingViewModel RegisterListingVM { get; set; }
        public TimeDomainReflectometryViewModel TDRVM { get; set; }
        public TestModeViewModel TestModeVM { get; set; }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice == null)
                return;

            if (((_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1100)
                && (_selectedDeviceStore.SelectedDevice.BoardName != "DEMO-ADIN1100D2Z"))
             || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110)
             || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111))
                IsCableDiagVisible = true;
            else
                IsCableDiagVisible = false;

            if ((_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN2111)
             || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110))
            {
                IsADIN2111 = true;
                IsCableDiagSelected = true;
            }
            else
                IsADIN2111 = false;

            if ((!IsCableDiagVisible) && IsCableDiagSelected)
            {
                IsTestModeSelected = true;
            }
        }

        private void _selectedDeviceStore_OnGoingCalibrationStatusChanged(bool onGoingCalibrationStatus)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                EnableTabs = !onGoingCalibrationStatus;
            }));
        }

        public MenuItemViewModel MenuItemVM { get; set; }
    }
}