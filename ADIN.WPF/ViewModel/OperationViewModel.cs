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
        public OperationViewModel(SelectedDeviceStore selectedDeviceStore, IFTDIServices ftdiService)
        {
            DeviceListingViewModel = new DeviceListingViewModel(selectedDeviceStore, ftdiService);
        }

        public DeviceListingViewModel DeviceListingViewModel { get; }
    }
}