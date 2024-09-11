using ADI.Register.Services;
using ADIN.Device.Models.ADIN1320;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System;
using System.IO;

namespace ADIN.Device.Models
{
    public class ADIN1320Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;
        private ADIN1300FirmwareAPI _fwAPI;
        private uint _phyAddress;
        private IRegisterService _registerService;
        private string registerJsonFile;

        public ADIN1320Model(IFTDIServices ftdiService, IRegisterService registerService, object mainLock)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;
            PhyAddress = 0;
            DeviceType = BoardType.ADIN1320;

            //Retrieve Registers
            Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1300.json"));
            Registers = registerService.GetAdditionalRegisterSet_ADIN1200_ADIN1300(Registers);

            //DeviceStatus = new DeviceStatusADIN1320();

            FirmwareAPI = new ADIN1300FirmwareAPI(_ftdiService, Registers, PhyAddress, mainLock);
            _fwAPI = FirmwareAPI as ADIN1300FirmwareAPI;

            //LinkProperties = new LinkPropertiesADIN1320();
            Loopback = new LoopbackADIN1320();
            FrameGenChecker = new FrameGenCheckerADIN1320();
            //ClockPinControl = new ClockPinControlADIN1320();
            //TestMode = new TestModeADIN1320();

            //GetInitialValuesLinkProperties();
            //GetInitialValuesClockPinControl();
            //GetInitialValuesLoopback();
            //GetInitialValuesFrameGenChecker();
            //GetInitialValuesTestMode();
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
