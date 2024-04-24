using ADI.Register.Models;
using ADI.Register.Services;
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
    public class ADIN1300Model : AbstractADINFactory
    {
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;
        private string registerJsonFile;
        private ObservableCollection<RegisterModel> registers { get; set; }

        public ADIN1300Model(IFTDIServices ftdiService, IRegisterService registerService)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;

            //Retrieve Register
            registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1300.json"));

            FirmwareAPI = new ADIN1300FirmwareAPI(_ftdiService, registers);

            LinkProperties = new LinkPropertiesADIN1300();
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
