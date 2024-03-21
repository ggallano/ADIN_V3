using ADIN.Device.Models;

namespace ADIN.WPF.ViewModel
{
    public class DeviceListingItemViewModel : ViewModelBase
    {
        public DeviceListingItemViewModel(ADINDeviceModel device)
        {
            Device = device;
            ImagePath = @"..\Images\icons\Applications-Industrial-Automation-Ethernet-Icon.png";
        }

        public ADINDeviceModel Device { get; }

        public string ImagePath { get; }
        public string Name => Device.Name;
        public string SerialNumber => Device.SerialNumber;
    }
}