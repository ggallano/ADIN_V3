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

            LinkProperties = new LinkPropertiesADIN1320();
            Loopback = new LoopbackADIN1320();
            FrameGenChecker = new FrameGenCheckerADIN1320();
            //ClockPinControl = new ClockPinControlADIN1320();
            //TestMode = new TestModeADIN1320();

            GetInitialValuesLinkProperties();
            //GetInitialValuesClockPinControl();
            GetInitialValuesLoopback();
            GetInitialValuesFrameGenChecker();
            //GetInitialValuesTestMode();
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }

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

        private void GetInitialValuesLinkProperties()
        {
            LinkProperties.DownSpeedRetries = Convert.ToUInt32(_fwAPI.RegisterRead("NumSpeedRetry"));
            LinkProperties.IsAdvertise_1000BASE_T_FD = _fwAPI.RegisterRead("Fd1000Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_1000BASE_T_HD = _fwAPI.RegisterRead("Hd1000Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_100BASE_TX_FD = _fwAPI.RegisterRead("Fd100Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_100BASE_TX_HD = _fwAPI.RegisterRead("Hd100Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_10BASE_T_FD = _fwAPI.RegisterRead("Fd10Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_10BASE_T_HD = _fwAPI.RegisterRead("Hd10Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_EEE_1000BASE_T = _fwAPI.RegisterRead("Eee1000Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_EEE_100BASE_TX = _fwAPI.RegisterRead("Eee100Adv") == "1" ? true : false;
            LinkProperties.IsDownSpeed_100BASE_TX_HD = _fwAPI.RegisterRead("DnSpeedTo100En") == "1" ? true : false;
            LinkProperties.IsDownSpeed_10BASE_T_HD = _fwAPI.RegisterRead("DnSpeedTo10En") == "1" ? true : false;
            LinkProperties.SpeedMode = _fwAPI.RegisterRead("AutonegEn") == "1" ? LinkProperties.SpeedModes[0] : LinkProperties.SpeedModes[1];

            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_1000BASE_T_FD ? "SPEED_1000BASE_T_FD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_1000BASE_T_HD ? "SPEED_1000BASE_T_HD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_100BASE_TX_FD ? "SPEED_100BASE_TX_FD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_100BASE_TX_HD ? "SPEED_100BASE_TX_HD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_10BASE_T_FD ? "SPEED_10BASE_T_FD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_10BASE_T_HD ? "SPEED_10BASE_T_HD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_EEE_1000BASE_T ? "SPEED_1000BASE_EEE_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_EEE_100BASE_TX ? "SPEED_100BASE_EEE_SPEED" : "");

            var NrgPdEn = _fwAPI.RegisterRead("NrgPdEn") == "1" ? true : false;
            var NrgPdTxEn = _fwAPI.RegisterRead("NrgPdTxEn") == "1" ? true : false;

            if (!NrgPdEn)
                LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[0];

            if (NrgPdEn)
                if (!NrgPdTxEn)
                    LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[1];

            if (NrgPdEn)
                if (NrgPdTxEn)
                    LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[2];

            var SpeedSelLsb = _fwAPI.RegisterRead("SpeedSelLsb");
            var SpeedSelMsb = _fwAPI.RegisterRead("SpeedSelMsb");
            var DplxMode = _fwAPI.RegisterRead("DplxMode") == "1" ? true : false;

            if (SpeedSelLsb == "2")
                if (SpeedSelMsb == "2")
                    if (DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[4];

            if (SpeedSelLsb == "1")
                if (SpeedSelMsb == "1")
                    if (DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[3];

            if (SpeedSelLsb == "1")
                if (SpeedSelMsb == "1")
                    if (!DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[2];

            if (SpeedSelLsb == "0")
                if (SpeedSelMsb == "0")
                    if (DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[1];

            if (SpeedSelLsb == "0")
                if (SpeedSelMsb == "0")
                    if (!DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[0];

            var AutoMdiEn = _fwAPI.RegisterRead("AutoMdiEn") == "1" ? true : false;
            var ManMdix = _fwAPI.RegisterRead("ManMdix") == "1" ? true : false;

            if (AutoMdiEn)
                LinkProperties.MDIX = LinkProperties.MDIXs[0];

            if (!AutoMdiEn)
                if (!ManMdix)
                    LinkProperties.MDIX = LinkProperties.MDIXs[1];

            if (!AutoMdiEn)
                if (ManMdix)
                    LinkProperties.MDIX = LinkProperties.MDIXs[2];

            var PrefMstrAdv = _fwAPI.RegisterRead("PrefMstrAdv") == "1" ? true : false;

            if (PrefMstrAdv)
                LinkProperties.MasterSlaveAdvertise = LinkProperties.MasterSlaveAdvertises[0];

            if (!PrefMstrAdv)
                LinkProperties.MasterSlaveAdvertise = LinkProperties.MasterSlaveAdvertises[1];
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
    }
}
