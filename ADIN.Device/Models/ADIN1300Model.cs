// <copyright file="ADIN1300Model.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADI.Register.Services;
using ADIN.Device.Models.ADIN1300;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System;
using System.IO;

namespace ADIN.Device.Models
{
    public class ADIN1300Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;
        private string registerJsonFile;
        //private ObservableCollection<RegisterModel> registers;
        private uint _phyAddress;

        public ADIN1300Model(IFTDIServices ftdiService, IRegisterService registerService, object mainLock)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;
            PhyAddress = 0;
            DeviceType = BoardType.ADIN1300;

            //Retrieve Registers
            Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1300.json"));
            Registers = registerService.GetAdditionalRegisterSet_ADIN1200_ADIN1300(Registers);

            DeviceStatus = new DeviceStatusADIN1300();

            FirmwareAPI = new ADIN1300FirmwareAPI(_ftdiService, Registers, PhyAddress, mainLock);
            Loopback = new LoopbackADIN1300();

            LinkProperties = new LinkPropertiesADIN1300();
            Loopback = new LoopbackADIN1300();
            FrameGenChecker = new FrameGenCheckerADIN1300();
            ClockPinControl = new ClockPinControlADIN1300();
            TestMode = new TestModeADIN1300();

            GetInitialValuesLinkProperties();
            GetInitialValuesLoopback();
            GetInitialValuesFrameGenChecker();
            GetInitialValuesTestMode();
        }

        private void GetInitialValuesLinkProperties()
        {
            LinkProperties.DownSpeedRetries = Convert.ToUInt32(((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("NumSpeedRetry"));
            LinkProperties.IsAdvertise_1000BASE_T_FD = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Fd1000Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_1000BASE_T_HD = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Hd1000Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_100BASE_TX_FD = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Fd100Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_100BASE_TX_HD = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Hd100Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_10BASE_T_FD = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Fd10Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_10BASE_T_HD = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Hd10Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_EEE_1000BASE_T = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Eee1000Adv") == "1" ? true : false;
            LinkProperties.IsAdvertise_EEE_100BASE_TX = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Eee100Adv") == "1" ? true : false;
            LinkProperties.IsDownSpeed_100BASE_TX_HD = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("DnSpeedTo100En") == "1" ? true : false;
            LinkProperties.IsDownSpeed_10BASE_T_HD = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("DnSpeedTo10En") == "1" ? true : false;
            LinkProperties.SpeedMode = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("AutonegEn") == "1" ? LinkProperties.SpeedModes[0] : LinkProperties.SpeedModes[1];

            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_1000BASE_T_FD ? "SPEED_1000BASE_T_FD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_1000BASE_T_HD ? "SPEED_1000BASE_T_HD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_100BASE_TX_FD ? "SPEED_100BASE_TX_FD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_100BASE_TX_HD ? "SPEED_100BASE_TX_HD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_10BASE_T_FD ? "SPEED_10BASE_T_FD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_10BASE_T_HD ? "SPEED_10BASE_T_HD_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_EEE_1000BASE_T ? "SPEED_1000BASE_EEE_SPEED" : "");
            LinkProperties.AdvertisedSpeeds.Add(LinkProperties.IsAdvertise_EEE_100BASE_TX ? "SPEED_100BASE_EEE_SPEED" : "");

            var NrgPdEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("NrgPdEn") == "1" ? true : false;
            var NrgPdTxEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("NrgPdTxEn") == "1" ? true : false;

            if (!NrgPdEn)
                LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[0];

            if (NrgPdEn)
                if (!NrgPdTxEn)
                    LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[1];

            if (NrgPdEn)
                if (NrgPdTxEn)
                    LinkProperties.EnergyDetectPowerDownMode = LinkProperties.EnergyDetectPowerDownModes[2];

            var SpeedSelLsb = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("SpeedSelLsb");
            var SpeedSelMsb = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("SpeedSelMsb");
            var DplxMode = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("DplxMode") == "1" ? true : false;

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

            var AutoMdiEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("AutoMdiEn") == "1" ? true : false;
            var ManMdix = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("ManMdix") == "1" ? true : false;

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
            var LoopbackEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Loopback") == "1" ? true : false;

            if (!LoopbackEn)
                Loopback.SelectedLoopback = Loopback.Loopbacks[0];

            if (LoopbackEn)
                if (((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("LbAllDigSel") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[1];

            if (LoopbackEn)
                if (((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("LbLdSel") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[2];

            if (LoopbackEn)
                if (((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("LbExtEn") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[3];

            if (LoopbackEn)
                if (((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("LbRemoteEn") == "1")
                    Loopback.SelectedLoopback = Loopback.Loopbacks[4];

            Loopback.RxSuppression = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("IsolateRx") == "1" ? true : false;
            Loopback.TxSuppression = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("LbTxSup") == "1" ? true : false;
        }

        private void GetInitialValuesFrameGenChecker()
        {
            FrameGenChecker.EnableContinuousMode = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgContModeEn") == "1" ? true : false;
            uint frameBurstH = Convert.ToUInt32(((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgNfrmH")) * 65536;
            uint frameBurstL = Convert.ToUInt32(((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgNfrmL"));
            FrameGenChecker.FrameBurst = frameBurstH + frameBurstL;
            FrameGenChecker.FrameLength = Convert.ToUInt32(((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgFrmLen"));
            var FgCntrl = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgCntrl");

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

        private void GetInitialValuesTestMode()
        {
            var AutonegEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("AutonegEn") == "1" ? true : false;
            var SpeedSelMsb = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("SpeedSelMsb") == "1" ? true : false;
            var SpeedSelLsb = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("SpeedSelLsb") == "1" ? true : false;
            var AutoMdiEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("AutoMdiEn") == "1" ? true : false;
            var ManMdix = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("ManMdix") == "1" ? true : false;
            var LbTxSup = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("LbTxSup") == "1" ? true : false;
            var LoopbackEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("Loopback") == "1" ? true : false;
            var LinkEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("LinkEn") == "1" ? true : false;
            var DiagClkEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("DiagClkEn") == "1" ? true : false;
            var FgFrmLen = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgFrmLen");
            var FgContModeEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgContModeEn") == "1" ? true : false;
            var FgCntrl = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgCntrl");
            var FgNoHdr = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgNoHdr") == "1" ? true : false;
            var FgNoFcs = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgNoFcs") == "1" ? true : false;
            var FgEn = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("FgEn") == "1" ? true : false;
            var B10TxTstMode = ((ADIN1300FirmwareAPI)FirmwareAPI).RegisterRead("B10TxTstMode");

            if (!AutonegEn)
                if (!SpeedSelMsb)
                    if (SpeedSelLsb)
                        if (!AutoMdiEn)
                            if (!ManMdix)
                                if (LinkEn)
                                    TestMode.TestMode = TestMode.TestModes[0];

            if (!AutonegEn)
                if (!SpeedSelMsb)
                    if (!SpeedSelLsb)
                        if (!AutoMdiEn)
                            if (!ManMdix)
                                if (!LbTxSup)
                                    if (LoopbackEn)
                                        if (LinkEn)
                                            TestMode.TestMode = TestMode.TestModes[1];

            if (!AutonegEn)
                if (!SpeedSelMsb)
                    if (!SpeedSelLsb)
                        if (!AutoMdiEn)
                            if (!ManMdix)
                                if (!LbTxSup)
                                    if (LoopbackEn)
                                        if (LinkEn)
                                            if (DiagClkEn)
                                                if (FgContModeEn)
                                                    if (FgEn)
                                                        TestMode.TestMode = TestMode.TestModes[2];

            if (!AutonegEn)
                if (!SpeedSelMsb)
                    if (!SpeedSelLsb)
                        if (!AutoMdiEn)
                            if (!ManMdix)
                                if (!LbTxSup)
                                    if (LoopbackEn)
                                        if (LinkEn)
                                            if (DiagClkEn)
                                                if (FgContModeEn)
                                                    if (FgCntrl == "3")
                                                        if (FgNoHdr)
                                                            if (FgNoFcs)
                                                                if (FgEn)
                                                                    TestMode.TestMode = TestMode.TestModes[3];

            if (!AutonegEn)
                if (!SpeedSelMsb)
                    if (!SpeedSelLsb)
                        if (!AutoMdiEn)
                            if (!ManMdix)
                                if (!LbTxSup)
                                    if (LoopbackEn)
                                        if (LinkEn)
                                            if (DiagClkEn)
                                                if (FgContModeEn)
                                                    if (FgCntrl == "1")
                                                        if (FgNoHdr)
                                                            if (FgNoFcs)
                                                                if (FgEn)
                                                                    TestMode.TestMode = TestMode.TestModes[4];

            if (B10TxTstMode == "4")
                TestMode.TestMode = TestMode.TestModes[5];

            if (B10TxTstMode == "2")
                TestMode.TestMode = TestMode.TestModes[6];

            if (B10TxTstMode == "3")
                TestMode.TestMode = TestMode.TestModes[7];

            if (B10TxTstMode == "1")
                TestMode.TestMode = TestMode.TestModes[8];

            TestMode.TestModeFrameLength = Convert.ToUInt32(FgFrmLen);
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
