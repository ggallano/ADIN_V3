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
        private readonly SelectedDeviceStore _selectedDeviceStore;
        private readonly NavigationStore _navigationStore;

        public LinkPropertiesViewModel LinkPropertiesVM { get; set; }

        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore)
        {
            _selectedDeviceStore = selectedDeviceStore;
            _navigationStore = navigationStore;

            DeviceListingViewModel = new DeviceListingViewModel(selectedDeviceStore, ftdiService);
            LinkPropertiesVM = new LinkPropertiesViewModel(_navigationStore, _selectedDeviceStore);

            _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;
        }

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;
        public DeviceListingViewModel DeviceListingViewModel { get; }

        private void _navigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}