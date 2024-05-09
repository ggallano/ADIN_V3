using ADI.Register.Services;
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
        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore, IRegisterService registerService)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _navigationStore = navigationStore;

            LogActivityVM = new LogActivityViewModel(_selectedDeviceStore);
            DeviceListingVM = new DeviceListingViewModel(selectedDeviceStore, ftdiService, registerService, LogActivityVM);
            RegisterListingVM = new RegisterListingViewModel(selectedDeviceStore, ftdiService);
            LinkPropertiesVM = new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore);
            ClockPinControlVM = new ClockPinControlViewModel(_navigationStore, _selectedDeviceStore);
            TestModeVM = new TestModeViewModel(_selectedDeviceStore);
            //DeviceStatusVM = new DeviceStatusViewModel(selectedDeviceStore, ftdiService);

            _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;
        }

        private void _navigationStore_CurrentViewModelChanged1()
        {
            throw new System.NotImplementedException();
        }

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;
        public DeviceListingViewModel DeviceListingVM { get; }
        public LinkPropertiesViewModel LinkPropertiesVM { get; set; }
        public ClockPinControlViewModel ClockPinControlVM { get; set; }
        public TestModeViewModel TestModeVM { get; set; }
        public RegisterListingViewModel RegisterListingVM { get; set; }
        public DeviceStatusViewModel DeviceStatusVM { get; set; }
        public LogActivityViewModel LogActivityVM { get; set; }
        private void _navigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}