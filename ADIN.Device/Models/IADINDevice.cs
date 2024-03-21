using ADIN.Device.Services;

namespace ADIN.Device.Models
{
    public interface IADINDevice
    {
        string BoardName { get; set; }
        string SerialNumber { get; set; }
        IFirmwareAPI FirmwareAPI { get; set; }
    }
}
