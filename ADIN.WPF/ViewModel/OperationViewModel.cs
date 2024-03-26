using ADI.Register.Services;
using ADIN.Device.Services;
using ADIN.WPF.Service;
using ADIN.WPF.Stores;
using FTDIChip.Driver.Services;
using System.Globalization;
using System.IO.Pipes;
using System.Threading;

namespace ADIN.WPF.ViewModel
{
    public class OperationViewModel : ViewModelBase
    {
        private readonly SelectedDeviceStore _selectedDeviceStore;
        private readonly IFTDIServices _ftdiService;
        private readonly NavigationStore _navigationStore;

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;
        public NavigationBarViewModel NavigationBarViewModel { get; set; }

        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _ftdiService = ftdiService;
            _navigationStore = navigationStore;

           

            DeviceListingViewModel = new DeviceListingViewModel(_selectedDeviceStore, _ftdiService);
            NavigationBarViewModel = new NavigationBarViewModel(CreateLinkPropNavigationService(), 
                CreateRegisterAccNavigationService());

            _navigationStore.CurrentViewModel = new LinkPropertiesViewModel(_navigationStore, NavigationBarViewModel);


            _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;

        }

        private NavigationService<LinkPropertiesViewModel> CreateLinkPropNavigationService()
        {
            return new NavigationService<LinkPropertiesViewModel>(_navigationStore, () => new LinkPropertiesViewModel(_navigationStore, NavigationBarViewModel));
        }

        private NavigationService<RegisterAccessViewModel> CreateRegisterAccNavigationService()
        {
            return new NavigationService<RegisterAccessViewModel>(_navigationStore, () => new RegisterAccessViewModel(_navigationStore, NavigationBarViewModel));
        }

        private void _navigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        public DeviceListingViewModel DeviceListingViewModel { get; }
    }
}