using ADI.Register.Models;
using ADIN.Device.Services;
using System.Collections.ObjectModel;

namespace ADIN.Device.Models
{
    public abstract class AbstractADINFactory
    {
        public string BoardName { get; set; }
        public string SerialNumber { get; set; }
        public abstract IFirmwareAPI FirmwareAPI { get; set; }
        public ILinkProperties LinkProperties { get; set; }
        public IClockPinControl ClockPinControl { get; set; }
        public ITestMode TestMode { get; set; }
        public ObservableCollection<RegisterModel> Registers { get; set; }
    }
}
