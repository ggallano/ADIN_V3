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
        private uint phyAddress;

        public ADIN1200Model(IFTDIServices ftdiService, IRegisterService registerService)
        {
            _ftdiService = ftdiService;
            _registerService = registerService;
            phyAddress = 4;

            //Retrieve Registers
            Registers = registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1200.json"));

            FirmwareAPI = new ADIN1200FirmwareAPI(_ftdiService, Registers, phyAddress);

            LinkProperties = new LinkPropertiesADIN1200();
            ClockPinControl = new ClockPinControlADIN1200();
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
