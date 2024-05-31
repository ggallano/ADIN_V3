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
        }

        public override IFirmwareAPI FirmwareAPI { get; set; }
    }
}
