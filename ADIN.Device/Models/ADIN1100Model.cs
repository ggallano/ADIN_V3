using ADI.Register.Services;
using ADIN.Device.Models.ADIN1100;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1100_S2.json"));

            FirmwareAPI = new ADIN1100FirmwareAPI(_ftdiService, Registers, PhyAddress, mainLock);

            LinkProperties = new LinkPropertiesADIN1100();
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
