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

        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;

        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService, NavigationStore navigationStore)
        {
            DeviceListingViewModel = new DeviceListingViewModel(selectedDeviceStore, ftdiService);
            
            _navigationStore = navigationStore;
            _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;

        }

        private void _navigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }

        public DeviceListingViewModel DeviceListingViewModel { get; }
    }
}