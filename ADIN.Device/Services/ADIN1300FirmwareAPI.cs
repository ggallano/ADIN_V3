 using ADI.Register.Models;
using ADI.Register.Services;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using FTDIChip.Driver.Services;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public class ADIN1300FirmwareAPI : IFirmwareAPI
    {
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;
        private ObservableCollection<RegisterModel> _registers;
        private uint _phyAddress;

        public ADIN1300FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, uint phyAddress)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
        }

        public event EventHandler<FeedbackModel> WriteProcessCompleted;

        public string MdioReadCl22(uint regAddress)
        {
            string response = string.Empty;
            string command = string.Empty;

            command = $"mdioread {_phyAddress},{regAddress.ToString("X")}\n";
            //lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();
            }

            if (response.Contains("ERROR"))
            {
                //OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                //throw new ApplicationException(response);
            }

            Debug.WriteLine($"Command:{command.TrimEnd()}");
            Debug.WriteLine($"Response:{response}");

            return response;
        }

        public string MdioReadCl45(uint regAddress)
        {
            string response = string.Empty;
            string command = string.Empty;
            string command2 = string.Empty;

            MdioWriteCl22(0x10, (regAddress & 0xFFFF));
            command = $"mdioread {_phyAddress},11\n";
            //lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();
            }

            if (response.Contains("ERROR"))
            {
                //OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                //throw new ApplicationException(response);
            }

            Debug.WriteLine($"Command:{command.TrimEnd()}");
            Debug.WriteLine($"Response:{response}");

            return response;
        }

        public string MdioWriteCl22(uint regAddress, uint data)
        {
            string response = string.Empty;
            string command = string.Empty;

            command = $"mdiowrite {_phyAddress},{regAddress.ToString("X")},{data.ToString("X")}\n";
            //lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();
            }

            if (response.Contains("ERROR"))
            {
                //OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                //throw new ApplicationException(response);
            }

            Debug.WriteLine($"Command:{command.TrimEnd()}");
            Debug.WriteLine($"Response:{response}");

            return response;
        }

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            string response = string.Empty;
            string command = string.Empty;
            string command2 = string.Empty;

            command = $"mdiowrite {_phyAddress},10,{regAddress.ToString("X")}\n";
            command2 = $"mdiowrite {_phyAddress},11,{data.ToString("X")}\n";
            //lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
                _ftdiService.SendData(command2);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            if (response.Contains("ERROR"))
            {
                //OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            Debug.WriteLine($"Command:{command.TrimEnd()}");
            Debug.WriteLine($"Response:{response}");

            return response;
        }

        private string ReadYogaRg(string name)
        {
            RegisterModel register = null;
            string value = string.Empty;

            register = GetRegister(name);
            if (register == null)
                throw new ApplicationException("Invalid Register");

            //lock (thisLock)

            uint pageNumber = register.Address >> 16;
            uint pageAddr = register.Address & 0xFFFF;
            if (pageNumber == 0)
            {
                register.Value = MdioReadCl22(register.Address);
            }
            else
            {
                register.Value = MdioReadCl45(register.Address);
            }

            foreach (var bitfield in register.BitFields)
            {
                if (bitfield.Name == name)
                    value = bitfield.Value.ToString();
            }

            return value;
        }

        private string ReadYodaRg(uint registerAddress)
        {
            string value = string.Empty;

            uint pageNumber = registerAddress >> 16;
            uint pageAddr = registerAddress & 0xFFFF;
            if (pageNumber == 0)
            {
                value = MdioReadCl22(registerAddress);
            }
            else
            {
                value = MdioReadCl45(registerAddress);
            }

            return value;
        }

        private RegisterModel GetRegister(string name)
        {
            RegisterModel register = new RegisterModel();

            var res = _registers.Where(x => x.Name == name).ToList();
            if (res.Count == 0)
            {
                res = _registers.Where(r => r.BitFields.Any(b => b.Name == name)).ToList();
                if (res.Count == 1)
                {
                    register = res[0];
                }
            }
            else
            {
                register = res[0];
            }

            return register;
        }

        private RegisterModel SetRegisterValue(string name, uint value)
        {
            RegisterModel register = new RegisterModel();

            var res = _registers.Where(x => x.Name == name).ToList();
            if (res.Count == 0)
            {
                res = _registers.Where(r => r.BitFields.Any(b => b.Name == name)).ToList();
                if (res.Count == 1)
                {
                    register = res[0];
                    foreach (var bitField in res[0].BitFields)
                    {
                        if (bitField.Name == name)
                            bitField.Value = value;
                    }
                }
            }
            else
            {
                register = res[0];
                register.Value = value.ToString("X");
            }

            return register;
        }

        private void WriteYodaRg(string name, uint value)
        {
            RegisterModel register = null;

            register = SetRegisterValue(name, value);

            uint pageNumber = register.Address >> 16;
            uint pageAddr = register.Address & 0xFFFF;
            if (pageNumber == 0)
            {
                MdioWriteCl22(register.Address, UInt32.Parse(register.Value, NumberStyles.HexNumber));
            }
            else
            {
                MdioWriteCl45(register.Address, UInt32.Parse(register.Value, NumberStyles.HexNumber));
            }
        }

        private void WriteYodaRg(uint registerAddress, uint value)
        {
            MdioWriteCl45(registerAddress, value);
            //OnWriteProcessCompleted(new FeedbackModel() { Message = $"[{_ftdiService.GetSerialNumber()}] [Write] Address: 0x{registerAddress.ToString("X")}, Value: {value.ToString("X")}", FeedBackType = FeedbackType.Info });
        }

        public void Speed1000FdAdvertisement(bool spd1000FdAdv_st)
        {
            if (spd1000FdAdv_st)
            {
                this.WriteYodaRg("Fd1000Adv", 1);
            }
            else
            {
                this.WriteYodaRg("Fd1000Adv", 0);
            }
        }

        public void Speed1000HdAdvertisement(bool spd1000HdAdv_st)
        {
            if (spd1000HdAdv_st)
            {
                this.WriteYodaRg("Hd1000Adv", 1);
            }
            else
            {
                this.WriteYodaRg("Hd1000Adv", 0);
            }
        }

        public void Speed100FdAdvertisement(bool spd100FdAdv_st)
        {
            if (spd100FdAdv_st)
            {
                this.WriteYodaRg("Fd100Adv", 1);
            }
            else
            {
                this.WriteYodaRg("Fd100Adv", 0);
            }
        }
        public void Speed100HdAdvertisement(bool spd100HdAdv_st)
        {
            if (spd100HdAdv_st)
            {
                this.WriteYodaRg("Hd100Adv", 1);
            }
            else
            {
                this.WriteYodaRg("Hd100Adv", 0);
            }
        }
        public void Speed10FdAdvertisement(bool spd10FdAdv_st)
        {
            if (spd10FdAdv_st)
            {
                this.WriteYodaRg("Fd10Adv", 1);
            }
            else
            {
                this.WriteYodaRg("Fd10Adv", 0);
            }
        }
        public void Speed10HdAdvertisement(bool spd10HdAdv_st)
        {
            if (spd10HdAdv_st)
            {
                this.WriteYodaRg("Hd10Adv", 1);
            }
            else
            {
                this.WriteYodaRg("Hd10Adv", 0);
            }
        }
        public void Speed1000EEEAdvertisement(bool spd1000EEEAdv_st)
        {
            if (spd1000EEEAdv_st)
            {
                this.WriteYodaRg("Eee1000Adv", 1);
            }
            else
            {
                this.WriteYodaRg("Eee1000Adv", 0);
            }
        }
        public void Speed100EEEAdvertisement(bool spd100EEEAdv_st)
        {
            if (spd100EEEAdv_st)
            {
                this.WriteYodaRg("Eee100Adv", 1);
            }
            else
            {
                this.WriteYodaRg("Eee100Adv", 0);
            }
        }
        public void CheckAdvertisedSpeed(List<string> listAdvSpd)
        {
            throw new NotImplementedException();
        }
        public void AdvertisedForcedSpeed(string advFrcSpd)
        {
            throw new NotImplementedException();
        }
        public void DownSpeed100Hd(bool dwnSpd100Hd)
        {
            throw new NotImplementedException();
        }
        public void DownSpeed10Hd(bool dwnSpd10Hd)
        {
            throw new NotImplementedException();
        }
        public void DownSpeedRetriesSetVal(uint dwnSpdRtryVal)
        {
            throw new NotImplementedException();
        }
        public void AutoMDIXMode(string autoMDIXmod)
        {
            throw new NotImplementedException();
        }
        public void EnableEnergyDetectPowerDown(string enEnergyDetect)
        {
            throw new NotImplementedException();
        }
        public void SetGpClkPinControl(string gpClkPinCtrl)
        {
            this.WriteYodaRg("GeClkCfg", 0);

            switch (gpClkPinCtrl)
            {
                case "125 MHz PHY Recovered":
                    this.WriteYodaRg("GeClkRcvr125En", 1);
                    break;
                case "125 MHz PHY Free Running":
                    this.WriteYodaRg("GeClkFree125En", 1);
                    break;
                case "Recovered HeartBeat":
                    this.WriteYodaRg("GeClkHrtRcvrEn", 1);
                    break;
                case "Free Running HeartBeat":
                    this.WriteYodaRg("GeClkHrtFreeEn", 1);
                    break;
                case "25 MHz Reference":
                    this.WriteYodaRg("GeClk25En", 1);
                    break;
                default:
                    // Not enable any register
                    break;

            }
        }
        public void SetTestMode(TestModeListingModel testMode, uint framelength)
        {
            throw new NotImplementedException();
        }

        public void ReadRegsiters()
        {
            foreach (var register in _registers)
            {
                register.Value = ReadYodaRg(register.Address);
            }
            Debug.WriteLine("ReadRegisters Done");
        }
    }
}
