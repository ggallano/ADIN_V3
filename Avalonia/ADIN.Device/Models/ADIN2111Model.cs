using ADIN.Device.Models.ADIN1100;
using ADIN.Device.Services;
using ADIN.Register.Services;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Models
{
    public class ADIN2111Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;

        public ADIN2111Model(IFTDIServices ftdiService, IRegisterService registerService, int portNum, int phyAddress, object mainLock)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;
            PhyAddress = 0;
            PortNum = portNum;

            FirmwareAPI = new ADIN2111FirmwareAPI(_ftdiService, PhyAddress, mainLock);

            //switch (ADIN2111FirmwareAPI.GetRevNum(0x1E0003))
            //{
            //    case BoardRevision.Rev0:
            //    case BoardRevision.Rev1:
            //        Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1100_S2.json"));
            //        Registers = registerService.GetAdditionalRegisterSetRev1(Registers);
            //        BoardRev = BoardRevision.Rev1;
            //        DeviceType = BoardType.ADIN2111;
            //        break;
            //    default:
            //        break;
            //}

            Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1100_S2.json"));
            Registers = registerService.GetAdditionalRegisterSetRev1(Registers);
            BoardRev = BoardRevision.Rev1;
            DeviceType = BoardType.ADIN2111;

            FirmwareAPI = new ADIN2111FirmwareAPI(_ftdiService, Registers, PhyAddress, mainLock);
            ((ADIN2111FirmwareAPI)FirmwareAPI).SetPortNum(portNum);
            ((ADIN2111FirmwareAPI)FirmwareAPI).boardRev = BoardRev;

            LinkProperties = new LinkPropertiesADIN1100();
            GetLinkPropertiesValue();

            TestMode = new TestModeADIN1100();
            GetTestModeValue();

            FrameGenChecker = new FrameGenCheckerADIN1100();

            Loopback = new LoopbackADIN1100();
            GetLoopbackValue();

            TimeDomainReflectometry = new TimeDomainReflectometryADIN1100();
            GetTDRValue();
            //TimeDomainReflectometryPort2 = new TimeDomainReflectometryADIN1100();
            //GetTDRValuePort2();
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }

        private void GetLinkPropertiesValue()
        {
            var AN_ADV_MST = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("AN_ADV_MST") == "1" ? true : false;
            var AN_ADV_FORCE_MS = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("AN_ADV_FORCE_MS") == "1" ? true : false;

            if (AN_ADV_MST)
                if (!AN_ADV_FORCE_MS)
                    LinkProperties.MasterSlaveAdvertise = "Prefer_Master";

            if (!AN_ADV_MST)
                if (!AN_ADV_FORCE_MS)
                    LinkProperties.MasterSlaveAdvertise = "Prefer_Slave";

            if (AN_ADV_MST)
                if (AN_ADV_FORCE_MS)
                    LinkProperties.MasterSlaveAdvertise = "Forced_Master";

            if (!AN_ADV_MST)
                if (AN_ADV_FORCE_MS)
                    LinkProperties.MasterSlaveAdvertise = "Forced_Slave";

            var TX_LVL_HI_ABL = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("AN_ADV_B10L_TX_LVL_HI_ABL") == "1" ? true : false;
            var TX_LVL_HI_REQ = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("AN_ADV_B10L_TX_LVL_HI_REQ") == "1" ? true : false;

            if (TX_LVL_HI_ABL)
                if (TX_LVL_HI_REQ)
                    LinkProperties.TxAdvertise = "Capable2p4Volts_Requested2p4Volts";

            if (TX_LVL_HI_ABL)
                if (!TX_LVL_HI_REQ)
                    LinkProperties.TxAdvertise = "Capable2p4Volts_Requested1Volt";

            if (!TX_LVL_HI_ABL)
                if (!TX_LVL_HI_REQ)
                    LinkProperties.TxAdvertise = "Capable1Volt";
        }

        private void GetLoopbackValue()
        {
            var AN_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("AN_EN") == "1" ? true : false;
            var AN_FRC_MODE_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("AN_FRC_MODE_EN") == "1" ? true : false;
            var B10L_LB_PMA_LOC_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("B10L_LB_PMA_LOC_EN") == "1" ? true : false;
            var B10L_LB_PCS_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("B10L_LB_PCS_EN") == "1" ? true : false;
            var MAC_IF_LB_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("MAC_IF_LB_EN") == "1" ? true : false;
            var MAC_IF_REM_LB_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("MAC_IF_REM_LB_EN") == "1" ? true : false;
            var RMII_TXD_CHK_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("RMII_TXD_CHK_EN") == "1" ? true : false;

            LoopbackModel result = null;

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = Loopback.Loopbacks[3];

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = Loopback.Loopbacks[4];

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (RMII_TXD_CHK_EN)
                                        result = Loopback.Loopbacks[5];

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = Loopback.Loopbacks[1];

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = Loopback.Loopbacks[2];

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = Loopback.Loopbacks[0];

            Loopback.SelectedLoopback = result;
            Loopback.TxSuppression = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("MAC_IF_LB_TX_SUP_EN") == "1" ? true : false;
            Loopback.RxSuppression = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("MAC_IF_REM_LB_RX_SUP_EN") == "1" ? true : false;
        }

        private void GetTDRValue()
        {
            TimeDomainReflectometry.TimeDomainReflectometry.CableOffset = decimal.Parse(((ADIN2111FirmwareAPI)FirmwareAPI).GetOffset(), CultureInfo.InvariantCulture);
            TimeDomainReflectometry.TimeDomainReflectometry.NVP = decimal.Parse(((ADIN2111FirmwareAPI)FirmwareAPI).GetNvp(), CultureInfo.InvariantCulture);
        }

        private void GetTestModeValue()
        {
            var B10L_TX_TEST_MODE = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("CRSM_SFT_PD");
            var B10L_TX_DIS_MODE_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("B10L_TX_TEST_MODE") == "1" ? true : false;
            var AN_FRC_MODE_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("AN_FRC_MODE_EN") == "1" ? true : false;
            var AN_EN = ((ADIN2111FirmwareAPI)FirmwareAPI).RegisterRead("AN_EN") == "1" ? true : false;

            if (B10L_TX_TEST_MODE == "0")
                if (!B10L_TX_DIS_MODE_EN)
                    if (!AN_FRC_MODE_EN)
                        if (AN_EN)
                            TestMode.TestMode = TestMode.TestModes[0];

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "1")
                        if (!B10L_TX_DIS_MODE_EN)
                            TestMode.TestMode = TestMode.TestModes[1];

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "2")
                        if (!B10L_TX_DIS_MODE_EN)
                            TestMode.TestMode = TestMode.TestModes[2];

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "3")
                        if (!B10L_TX_DIS_MODE_EN)
                            TestMode.TestMode = TestMode.TestModes[3];

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "0")
                        if (!B10L_TX_DIS_MODE_EN)
                            TestMode.TestMode = TestMode.TestModes[4];
        }
    }
}
