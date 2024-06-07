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

        private bool _isCableDiagVisible;

        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore, IRegisterService registerService, object mainLock)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _navigationStore = navigationStore;

            LogActivityVM = new LogActivityViewModel(_selectedDeviceStore);
            DeviceListingVM = new DeviceListingViewModel(selectedDeviceStore, ftdiService, registerService, LogActivityVM, mainLock);
            ExtraCommandsVM = new ExtraCommandsViewModel(_selectedDeviceStore, ftdiService);
            RegisterListingVM = new RegisterListingViewModel(selectedDeviceStore, ftdiService);
            LinkPropertiesVM = new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore);
            LoopbackVM = new LoopbackViewModel(_selectedDeviceStore, ftdiService);
            FrameGenCheckerVM = new FrameGenCheckerViewModel(_selectedDeviceStore, mainLock, ftdiService);
            ClockPinControlVM = new ClockPinControlViewModel(_navigationStore, _selectedDeviceStore);
            TestModeVM = new TestModeViewModel(_selectedDeviceStore);
            DeviceStatusVM = new DeviceStatusViewModel(selectedDeviceStore, ftdiService);
            RegisterAccessVM = new RegisterAccessViewModel(selectedDeviceStore, navigationStore);

            _selectedDeviceStore.SelectedDeviceChanged += _selectedDeviceStore_SelectedDeviceChanged;
        }

        public ClockPinControlViewModel ClockPinControlVM { get; set; }

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

        public DeviceListingViewModel DeviceListingVM { get; }

        public DeviceStatusViewModel DeviceStatusVM { get; set; }

        public ExtraCommandsViewModel ExtraCommandsVM { get; set; }

        public FrameGenCheckerViewModel FrameGenCheckerVM { get; set; }

        public bool IsCableDiagVisible
        {
            get { return _isCableDiagVisible; }
            set
            {
                _isCableDiagVisible = value;
                OnPropertyChanged(nameof(IsCableDiagVisible));
            }
        }
        public LinkPropertiesViewModel LinkPropertiesVM { get; set; }

        public LogActivityViewModel LogActivityVM { get; set; }

        public LoopbackViewModel LoopbackVM { get; set; }

        public RegisterAccessViewModel RegisterAccessVM { get; set; }

        public RegisterListingViewModel RegisterListingVM { get; set; }

        public TestModeViewModel TestModeVM { get; set; }

        private void _selectedDeviceStore_SelectedDeviceChanged()
        {
            if (_selectedDeviceStore.SelectedDevice != null)
            {
                if (_selectedDeviceStore.SelectedDevice.DeviceType == BoardType.ADIN1100)
                    IsCableDiagVisible = true;
                else
                    IsCableDiagVisible = false;
            }

        }
    }
}