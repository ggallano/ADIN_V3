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
        private ADIN1300FirmwareAPI _fwAPI;
        private uint _phyAddress;
        private IRegisterService _registerService;
        private string registerJsonFile;

        public ADIN1300Model(IFTDIServices ftdiService, IRegisterService registerService, object mainLock, uint phyAddress = 0)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;
            PhyAddress = phyAddress;
            DeviceType = BoardType.ADIN1300;

            //Retrieve Registers
            Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1300.json"));
            Registers = registerService.GetAdditionalRegisterSet_ADIN1200_ADIN1300(Registers);

            DeviceStatus = new DeviceStatusADIN1300();

            FirmwareAPI = new ADIN1300FirmwareAPI(_ftdiService, Registers, PhyAddress, mainLock);
            _fwAPI = FirmwareAPI as ADIN1300FirmwareAPI;

            LinkProperties = new LinkPropertiesADIN1300();
            Loopback = new LoopbackADIN1300();
            FrameGenChecker = new FrameGenCheckerADIN1300();
            ClockPinControl = new ClockPinControlADIN1300();
            TestMode = new TestModeADIN1300();

            GetInitialValuesLinkProperties();
            GetInitialValuesClockPinControl();
            GetInitialValuesLoopback();
            GetInitialValuesFrameGenChecker();
            GetInitialValuesTestMode();
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }

        private void GetInitialValuesClockPinControl()
        {
            var GeClkRcvr125En = _fwAPI.RegisterRead("GeClkRcvr125En") == "1" ? true : false;
            var GeClkFree125En = _fwAPI.RegisterRead("GeClkFree125En") == "1" ? true : false;
            var GeClkHrtRcvrEn = _fwAPI.RegisterRead("GeClkHrtRcvrEn") == "1" ? true : false;
            var GeClkHrtFreeEn = _fwAPI.RegisterRead("GeClkHrtFreeEn") == "1" ? true : false;
            var GeClk25En = _fwAPI.RegisterRead("GeClk25En") == "1" ? true : false;

            if (!GeClkRcvr125En)
                if (!GeClkFree125En)
                    if (!GeClkHrtRcvrEn)
                        if (!GeClkHrtFreeEn)
                            if (!GeClk25En)
                                ClockPinControl.GpClkPinControl = ClockPinControl.GpClkPinControls[0];

            if (GeClkRcvr125En)
                if (!GeClkFree125En)
                    if (!GeClkHrtRcvrEn)
                        if (!GeClkHrtFreeEn)
                            if (!GeClk25En)
                                ClockPinControl.GpClkPinControl = ClockPinControl.GpClkPinControls[1];

            if (!GeClkRcvr125En)
                if (GeClkFree125En)
                    if (GeClkHrtRcvrEn)
                        if (GeClkHrtFreeEn)
                            if (GeClk25En)
                                ClockPinControl.GpClkPinControl = ClockPinControl.GpClkPinControls[2];

            if (!GeClkRcvr125En)
                if (!GeClkFree125En)
                    if (GeClkHrtRcvrEn)
                        if (!GeClkHrtFreeEn)
                            if (!GeClk25En)
                                ClockPinControl.GpClkPinControl = ClockPinControl.GpClkPinControls[3];

            if (!GeClkRcvr125En)
                if (!GeClkFree125En)
                    if (!GeClkHrtRcvrEn)
                        if (GeClkHrtFreeEn)
                            if (!GeClk25En)
                                ClockPinControl.GpClkPinControl = ClockPinControl.GpClkPinControls[4];

            if (!GeClkRcvr125En)
                if (!GeClkFree125En)
                    if (!GeClkHrtRcvrEn)
                        if (!GeClkHrtFreeEn)
                            if (GeClk25En)
                                ClockPinControl.GpClkPinControl = ClockPinControl.GpClkPinControls[5];

            var GeRefClkEn = _fwAPI.RegisterRead("GeRefClkEn") == "1" ? true : false;

            if (GeRefClkEn)
                ClockPinControl.Clk25RefPnCtrl = ClockPinControl.Clk25RefPinControls[1];
            else
                ClockPinControl.Clk25RefPnCtrl = ClockPinControl.Clk25RefPinControls[0];
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
        private void GetInitialValuesTestMode()
        {
            var AutonegEn = _fwAPI.RegisterRead("AutonegEn") == "1" ? true : false;
            var SpeedSelMsb = _fwAPI.RegisterRead("SpeedSelMsb") == "1" ? true : false;
            var SpeedSelLsb = _fwAPI.RegisterRead("SpeedSelLsb") == "1" ? true : false;
            var AutoMdiEn = _fwAPI.RegisterRead("AutoMdiEn") == "1" ? true : false;
            var ManMdix = _fwAPI.RegisterRead("ManMdix") == "1" ? true : false;
            var LbTxSup = _fwAPI.RegisterRead("LbTxSup") == "1" ? true : false;
            var LoopbackEn = _fwAPI.RegisterRead("Loopback") == "1" ? true : false;
            var LinkEn = _fwAPI.RegisterRead("LinkEn") == "1" ? true : false;
            var DiagClkEn = _fwAPI.RegisterRead("DiagClkEn") == "1" ? true : false;
            var FgFrmLen = _fwAPI.RegisterRead("FgFrmLen");
            var FgContModeEn = _fwAPI.RegisterRead("FgContModeEn") == "1" ? true : false;
            var FgCntrl = _fwAPI.RegisterRead("FgCntrl");
            var FgNoHdr = _fwAPI.RegisterRead("FgNoHdr") == "1" ? true : false;
            var FgNoFcs = _fwAPI.RegisterRead("FgNoFcs") == "1" ? true : false;
            var FgEn = _fwAPI.RegisterRead("FgEn") == "1" ? true : false;
            var B10TxTstMode = _fwAPI.RegisterRead("B10TxTstMode");

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
    }
}
