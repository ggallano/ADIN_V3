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
        private ADIN1200FirmwareAPI _fwAPI;
        private uint _phyAddress;
        private IRegisterService _registerService;
        private string registerJsonFile;

        public ADIN1320Model(IFTDIServices ftdiService, IRegisterService registerService, object mainLock)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;
            PhyAddress = 4;
            DeviceType = BoardType.ADIN1320;

            //Retrieve Registers
            Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1200.json"));
            Registers = registerService.GetAdditionalRegisterSet_ADIN1200_ADIN1300(Registers);

            DeviceStatus = new DeviceStatusADIN1320();

            FirmwareAPI = new ADIN1200FirmwareAPI(_ftdiService, Registers, PhyAddress, mainLock);
            _fwAPI = FirmwareAPI as ADIN1200FirmwareAPI;

            //LinkProperties = new LinkPropertiesADIN1320();
            Loopback = new LoopbackADIN1320();
            FrameGenChecker = new FrameGenCheckerADIN1320();
            //ClockPinControl = new ClockPinControlADIN1320();
            //TestMode = new TestModeADIN1320();

            //GetInitialValuesLinkProperties();
            //GetInitialValuesClockPinControl();
            GetInitialValuesLoopback();
            GetInitialValuesFrameGenChecker();
            //GetInitialValuesTestMode();
        }

        private void GetInitialValuesFrameGenChecker()
        {
            FrameGenChecker.EnableContinuousMode = _fwAPI.RegisterRead("FgContModeEn") == "1" ? true : false;
            uint frameBurstH = Convert.ToUInt32(_fwAPI.RegisterRead("FgNfrmH")) * 65536;
            uint frameBurstL = Convert.ToUInt32(_fwAPI.RegisterRead("FgNfrmL"));
            FrameGenChecker.FrameBurst = frameBurstH + frameBurstL;
            FrameGenChecker.FrameLength = Convert.ToUInt32(_fwAPI.RegisterRead("FgFrmLen"));
            var FgCntrl = _fwAPI.RegisterRead("FgCntrl");

            if (FgCntrl == "1")
                FrameGenChecker.FrameContent = FrameGenChecker.FrameContents[0];
            if (FgCntrl == "2")
                FrameGenChecker.FrameContent = FrameGenChecker.FrameContents[1];
            if (FgCntrl == "3")
                FrameGenChecker.FrameContent = FrameGenChecker.FrameContents[2];
            if (FgCntrl == "4")
                FrameGenChecker.FrameContent = FrameGenChecker.FrameContents[3];
            if (FgCntrl == "5")
                FrameGenChecker.FrameContent = FrameGenChecker.FrameContents[4];
        }

        private void GetInitialValuesLoopback()
        {
            var LoopbackEn = _fwAPI.RegisterRead("Loopback") == "1" ? true : false;

            if (!LoopbackEn)
                Loopback.SelectedLoopback = Loopback.Loopbacks[0];

            if (LoopbackEn)
                if (_fwAPI.RegisterRead("LbAllDigSel") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[1];

            if (LoopbackEn)
                if (_fwAPI.RegisterRead("LbLdSel") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[2];

            if (LoopbackEn)
                if (_fwAPI.RegisterRead("LbExtEn") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[3];

            if (LoopbackEn)
                if (_fwAPI.RegisterRead("LbRemoteEn") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[4];

            Loopback.RxSuppression = _fwAPI.RegisterRead("IsolateRx") == "1" ? true : false;
            Loopback.TxSuppression = _fwAPI.RegisterRead("LbTxSup") == "1" ? true : false;
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
