using ADI.Register.Models;
using ADIN.Device.Services;
using System.Collections.Generic;
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
        public BoardRevision BoardRev => Device.BoardRev;
        public uint PhyAddress => Device.PhyAddress;
        public List<string> AdvertisedSpeeds => Device.AdvertisedSpeeds;
        public BoardType DeviceType => Device.DeviceType;
        public IFirmwareAPI FwAPI => Device.FirmwareAPI;
        public string Checker { get; set; }
        public ILinkProperties LinkProperties => Device.LinkProperties;
        public ILoopback Loopback => Device.Loopback;
        public IFrameGenChecker FrameGenChecker => Device.FrameGenChecker;
        public IClockPinControl ClockPinControl => Device.ClockPinControl;
        public ITestMode TestMode => Device.TestMode;
        public ObservableCollection<RegisterModel> Registers => Device.Registers;
        public IDeviceStatus DeviceStatus => Device.DeviceStatus;
        public ITimeDomainReflectometry TimeDomainReflectometry => Device.TimeDomainReflectometry;
    }
}