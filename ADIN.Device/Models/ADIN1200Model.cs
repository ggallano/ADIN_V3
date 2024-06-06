using ADI.Register.Models;
using ADI.Register.Services;
using ADIN.Device.Models.ADIN1200;
using ADIN.Device.Models.ADIN1300;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class ADIN1200Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;
        private string registerJsonFile;
        //private ObservableCollection<RegisterModel> registers;
        private uint _phyAddress;
        private object _mainLock;

        public ADIN1200Model(IFTDIServices ftdiService, IRegisterService registerService, object mainLock)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;
            _mainLock = mainLock;
            _phyAddress = 4;
            PhyAddress = _phyAddress;
            DeviceType = BoardType.ADIN1200;

            //Retrieve Registers
            Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1200.json"));
            Registers = registerService.GetAdditionalRegisterSet_ADIN1200_ADIN1300(Registers);

            DeviceStatus = new DeviceStatusADIN1200();

            FirmwareAPI = new ADIN1200FirmwareAPI(_ftdiService, Registers, PhyAddress, _mainLock);

            LinkProperties = new LinkPropertiesADIN1200();
            Loopback = new LoopbackADIN1200();
            FrameGenChecker = new FrameGenCheckerADIN1200();
            ClockPinControl = new ClockPinControlADIN1200();
            TestMode = new TestModeADIN1200();

            GetInitialValuesLinkProperties();
            GetInitialValuesLoopback();
            GetInitialValuesFrameGenChecker();
        }

        private void GetInitialValuesLinkProperties()
        {
            LinkProperties.DownSpeedRetries = Convert.ToUInt32(((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("NumSpeedRetry"));
            LinkProperties.IsAdvertise_100BASE_TX_FD = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("Fd100Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_100BASE_TX_HD = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("Hd100Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_10BASE_T_FD = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("Fd10Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_10BASE_T_HD = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("Hd10Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_EEE_100BASE_TX = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("Eee100Adv") == "1" ? true : false;
            LinkProperties.IsDownSpeed_10BASE_T_HD = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("DnSpeedTo10En") == "1" ? true : false;
            LinkProperties.SpeedMode = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("AutonegEn") == "1" ? LinkProperties.SpeedModes[0] : LinkProperties.SpeedModes[1];

            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_100BASE_TX_FD ? "SPEED_100BASE_TX_FD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_100BASE_TX_HD ? "SPEED_100BASE_TX_HD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_10BASE_T_FD ? "SPEED_10BASE_T_FD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_10BASE_T_HD ? "SPEED_10BASE_T_HD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_EEE_100BASE_TX ? "SPEED_100BASE_EEE_SPEED" : "");

            var NrgPdEn = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("NrgPdEn") == "1" ? true : false;
            var NrgPdTxEn = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("NrgPdTxEn") == "1" ? true : false;

            if (!NrgPdEn)
                LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[0];

            if (NrgPdEn)
                if (!NrgPdTxEn)
                    LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[1];

            if (NrgPdEn)
                if (NrgPdTxEn)
                    LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[2];

            var SpeedSelLsb = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("SpeedSelLsb") == "1" ? true : false;
            var SpeedSelMsb = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("SpeedSelMsb") == "1" ? true : false;
            var DplxMode = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("DplxMode") == "1" ? true : false;

            if (SpeedSelLsb)
                if (SpeedSelMsb)
                    if (DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[3];

            if (SpeedSelLsb)
                if (SpeedSelMsb)
                    if (!DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[2];

            if (!SpeedSelLsb)
                if (!SpeedSelMsb)
                    if (DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[1];

            if (!SpeedSelLsb)
                if (!SpeedSelMsb)
                    if (!DplxMode)
                        LinkProperties.ForcedSpeed = LinkProperties.ForcedSpeeds[0];

            var AutoMdiEn = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("AutoMdiEn") == "1" ? true : false;
            var ManMdix = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("ManMdix") == "1" ? true : false;

            if (AutoMdiEn)
                LinkProperties.MDIX = LinkProperties.MDIXs[0];

            if (!AutoMdiEn)
                if (!ManMdix)
                    LinkProperties.MDIX = LinkProperties.MDIXs[1];

            if (!AutoMdiEn)
                if (ManMdix)
                    LinkProperties.MDIX = LinkProperties.MDIXs[2];
        }

        private void GetInitialValuesLoopback()
        {
            var LoopbackEn = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("Loopback") == "1" ? true : false;

            if (!LoopbackEn)
                Loopback.SelectedLoopback = Loopback.Loopbacks[0];

            if (LoopbackEn)
                if (((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("LbAllDigSel") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[1];

            if (LoopbackEn)
                if (((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("LbLdSel") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[2];
            
            if (LoopbackEn)
                if (((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("LbExtEn") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[3];

            if (LoopbackEn)
                if (((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("LbRemoteEn") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[4];

            Loopback.SelectedLoopback.RxSuppression = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("IsolateRx") == "1" ? true : false;
            Loopback.SelectedLoopback.TxSuppression = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("LbTxSup") == "1" ? true : false;
        }

        private void GetInitialValuesFrameGenChecker()
        {
            FrameGenChecker.EnableContinuousMode = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("FgContModeEn") == "1" ? true : false;
            uint frameBurstH = Convert.ToUInt32(((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("FgNfrmH")) * 65536;
            uint frameBurstL = Convert.ToUInt32(((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("FgNfrmL"));
            FrameGenChecker.FrameBurst = frameBurstH + frameBurstL;
            FrameGenChecker.FrameLength = Convert.ToUInt32(((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("FgFrmLen"));
            var FgCntrl = ((ADIN1200FirmwareAPI)FirmwareAPI).RegisterRead("FgCntrl");

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

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
