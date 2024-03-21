using ADI.Register.Models;
using ADI.Register.Services;
using ADIN.Device.Services;
using FTDIChip.Driver.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADI.ADIN.Test
{
    [TestClass]
    public class ADIN1100FirmwareAPITest
    {
        private IRegisterService _registerService;
        private IFTDIServices _ftdiService;
        private List<RegisterModel> _registers;
        private ADIN1100FirmwareAPI _firwmareAPI;

        public ADIN1100FirmwareAPITest()
        {
            _registerService = new RegisterService();
            _ftdiService = new FTDIServices();
            _registers = _registerService.GetRegisterSet(Path.Combine("Registers", "registers_adin1100_S2.json"));
            _firwmareAPI = new ADIN1100FirmwareAPI(_ftdiService, _registers);

        }

        [TestMethod]
        public void GetRegisterTest()
        {
            RegisterModel register = new RegisterModel();
            register = _firwmareAPI.GetRegister("CRSM_SFT_PD");
            _firwmareAPI.SetValue(register, "CRSM_SFT_PD", 1);


        }
    }
}
