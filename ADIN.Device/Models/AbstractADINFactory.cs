using ADIN.Device.Services;

namespace ADIN.Device.Models
{
    public abstract class AbstractADINFactory
    {
        public string BoardName { get; set; }
        public string SerialNumber { get; set; }
        public abstract IFirmwareAPI FirmwareAPI { get; set; }
        public ILinkProperties LinkProperties { get; set; }
        public IClockPinControl ClockPinControl { get; set; }
    }
}
