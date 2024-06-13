using ADI.Register.Services;
using ADIN.Device.Models;
using ADIN.Device.Services;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System.Globalization;
using System.Threading;

namespace ADIN.WPF.ViewModel
{
    public class OperationViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly SelectedDeviceStore _selectedDeviceStore;

        private bool _isCableDiagSelected;
        private bool _isCableDiagVisible;

        private bool _isTestModeSelected = false;

        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore, IRegisterService registerService, object mainLock)
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

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public ClockPinControlViewModel ClockPinControlVM { get; set; }
        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;
        public DeviceListingViewModel DeviceListingVM { get; }
        public DeviceStatusViewModel DeviceStatusVM { get; set; }
        public ExtraCommandsViewModel ExtraCommandsVM { get; set; }
        public FrameGenCheckerViewModel FrameGenCheckerVM { get; set; }
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

            if ((_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1100)
             || (_selectedDeviceStore.SelectedDevice?.DeviceType == BoardType.ADIN1110))
                IsCableDiagVisible = true;
            else
                IsCableDiagVisible = false;

            if ((!IsCableDiagVisible) && IsCableDiagSelected)
            {
                IsTestModeSelected = true;
            }
        }

        public MenuItemViewModel MenuItemVM { get; set; }
    }
}