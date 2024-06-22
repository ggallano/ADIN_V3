using ADIN.Device.Models;

namespace ADIN.WPF.ViewModel
{
    public class DeviceListingItemViewModel : ViewModelBase
    {
        public DeviceListingItemViewModel(ADINDevice device)
        {
            Device = device;
            ImagePath = @"..\Images\icons\Applications-Industrial-Automation-Ethernet-Icon.png";
        }

        public ADINDevice Device { get; }

        public string ImagePath { get; }
        public string Name => Device.Device.BoardName;
        public string SerialNumber => Device.Device.SerialNumber;
        public string ADINChip => Device.Device.DeviceType.ToString();
    }
}