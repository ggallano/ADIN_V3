using ADI.Register.Services;
using ADIN.Device.Models.ADIN1100;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System;
using System.IO;

namespace ADIN.Device.Models
{
    public class ADIN1100Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;

        public ADIN1100Model(IFTDIServices ftdiService, IRegisterService registerService, object mainLock)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;
            PhyAddress = 0;
            DeviceType = BoardType.ADIN1100;

            FirmwareAPI = new ADIN1100FirmwareAPI(_ftdiService, PhyAddress, mainLock);
            BoardRev = ((ADIN1100FirmwareAPI)FirmwareAPI).GetRevNum();

            switch (BoardRev)
            {
                case BoardRevision.Rev0:
                    Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1100_S1.json"));
                    Registers = registerService.GetAdditionalRegisterSetRev0(Registers);
                    break;
                case BoardRevision.Rev1:
                    Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1100_S2.json"));
                    Registers = registerService.GetAdditionalRegisterSetRev1(Registers);
                    break;
                default:
                    break;
            }

            FirmwareAPI = new ADIN1100FirmwareAPI(_ftdiService, Registers, PhyAddress, mainLock);

            LinkProperties = new LinkPropertiesADIN1100();
            GetLinkPropertiesValue();

            TestMode = new TestModeADIN1100();
            //GetTestModeValue();

            FrameGenChecker = new FrameGenCheckerADIN1100();
            Loopback = new LoopbackADIN1100();
        }

        private void GetTestModeValue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// retrieves the value of the eval board for link properties
        /// </summary>
        private void GetLinkPropertiesValue()
        {
            var AN_ADV_MST = ((ADIN1100FirmwareAPI)FirmwareAPI).RegisterRead("AN_ADV_MST") == "1" ? true : false;
            var AN_ADV_FORCE_MS = ((ADIN1100FirmwareAPI)FirmwareAPI).RegisterRead("AN_ADV_FORCE_MS") == "1" ? true : false;

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

            var TX_LVL_HI_ABL = ((ADIN1100FirmwareAPI)FirmwareAPI).RegisterRead("AN_ADV_B10L_TX_LVL_HI_ABL") == "1" ? true : false;
            var TX_LVL_HI_REQ = ((ADIN1100FirmwareAPI)FirmwareAPI).RegisterRead("AN_ADV_B10L_TX_LVL_HI_REQ") == "1" ? true : false;

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

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
