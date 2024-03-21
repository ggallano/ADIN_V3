using ADI.Register.Services;
using ADIN.Device.Models;
using FTDIChip.Driver.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace ADI.ADIN.Test
{
    [TestClass]
    public class RegisterTest
    {
        ADINDeviceModel _device;
        private readonly IRegisterService _registerService;
        private readonly IFTDIServices _ftdiService;

        public RegisterTest()
        {
            _registerService = new RegisterService();
            _ftdiService = new FTDIServices();
            _device = new ADINDeviceModel("AU6F1TCD", "ADIN1100DIZ", _ftdiService, _registerService);
            _device.Registers = _registerService.GetRegisterSet(Path.Combine("Registers", _device.RegisterJsonFile));
        }

        [TestMethod]
        public void SetBitFieldsValueTest()
        {
            _device.Registers[0].Value = 4096.ToString("X");
        }
    }
}
