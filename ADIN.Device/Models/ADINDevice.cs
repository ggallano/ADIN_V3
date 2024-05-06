using ADI.Register.Models;
using ADIN.Device.Services;
using System.Collections.ObjectModel;

namespace ADIN.Device.Models
{
    public class ADINDevice
    {
        public AbstractADINFactory Device { get; set; }
        public ADINDevice(AbstractADINFactory device)
        {
            Device = device;
        }

        public string SerialNumber => Device.SerialNumber;

        public string BoardName => Device.BoardName;

        public IFirmwareAPI FwAPI => Device.FirmwareAPI;

        public ILinkProperties LinkProperties => Device.LinkProperties;

        public IClockPinControl ClockPinControl => Device.ClockPinControl;
        public ITestMode TestMode => Device.TestMode;

        public ObservableCollection<RegisterModel> Registers => Device.Registers;
        public ObservableCollection<RegisterModel> RegistersBG => Device.RegistersBG;
    }
}