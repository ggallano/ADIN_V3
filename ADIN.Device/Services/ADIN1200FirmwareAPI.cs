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
    public class ADIN1200FirmwareAPI : IFirmwareAPI
    {
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;
        private ObservableCollection<RegisterModel> _registers;
        private ObservableCollection<RegisterModel> _registersBG;
        private uint _phyAddress;
        private object _ftdiLock = new object();

        public ADIN1200FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, ObservableCollection<RegisterModel> registersBG,uint phyAddress)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _registersBG = registersBG;
            _phyAddress = phyAddress;
        }

        public string MdioReadCl22(uint regAddress)
        {
            lock(_ftdiLock)
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
        }

        public string MdioReadCl45(uint regAddress)
        {
            lock (_ftdiLock)
            {
                string response = string.Empty;
                string command = string.Empty;

                command = $"mdiord_cl45 {_phyAddress},{regAddress.ToString("X")}\n";
                //lock (thisLock)
                {
                    _ftdiService.Purge();
                    _ftdiService.SendData(command);
                    //_ftdiService.SendData(command);

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
        }

        public string MdioWriteCl22(uint regAddress, uint data)
        {
            lock (_ftdiLock)
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
                    throw new ApplicationException(response);
                }

                Debug.WriteLine($"Command:{command.TrimEnd()}");
                Debug.WriteLine($"Response:{response}");

                return response;
            } 
        }

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            lock (_ftdiLock)
            {
                string response = string.Empty;
                string command = string.Empty;
                string command2 = string.Empty;

                command = $"mdiowr_cl45 {_phyAddress},{regAddress.ToString("X")},{data.ToString("X")}\n";
                //lock (thisLock)
                {
                    _ftdiService.Purge();
                    _ftdiService.SendData(command);
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
        }

        private string ReadYogaRg(string name)
        {
            RegisterModel register = null;
            string value = string.Empty;

            register = GetRegister(name);
            if (register == null)
                throw new ApplicationException("Invalid Register");

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
            string value;

            uint pageNumber = registerAddress >> 16;
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
                //MdioReadCl22(register.Address); // Temporary: for register change debug purpose
                MdioWriteCl22(register.Address, UInt32.Parse(register.Value, NumberStyles.HexNumber));
                //MdioReadCl22(register.Address); // Temporary: for register change debug purpose
            }
            else
            {
                //MdioReadCl45(register.Address); // Temporary: for register change debug purpose
                MdioWriteCl45(register.Address, UInt32.Parse(register.Value, NumberStyles.HexNumber));
                //MdioReadCl45(register.Address); // Temporary: for register change debug purpose
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

        public void AdvertisedForcedSpeed(string advFrcSpd)
        {
            if (advFrcSpd == "Advertised")
            {
                this.WriteYodaRg("AutonegEn", 1);
            }
            else
            {
                this.WriteYodaRg("AutonegEn", 0);
            }
        }
        public void DownSpeed10Hd(bool dwnSpd10Hd)
        {
            if (dwnSpd10Hd)
            {
                this.WriteYodaRg("DnSpeedTo10En", 1);
            }
            else
            {
                this.WriteYodaRg("DnSpeedTo10En", 0);
            }
        }
        public void DownSpeedRetriesSetVal(uint dwnSpdRtryVal)
        {
            this.WriteYodaRg("NumSpeedRetry", dwnSpdRtryVal);
        }
        public void AutoMDIXMode(string autoMDIXmod)
        {
            if (autoMDIXmod == "Auto MDIX")
            {
                this.WriteYodaRg("AutoMdiEn", 1);
            }
            else if (autoMDIXmod == "Fixed MDI")
            {
                this.WriteYodaRg("AutoMdiEn", 0);
                this.WriteYodaRg("ManMdix", 0);
            }
            else
            {
                this.WriteYodaRg("AutoMdiEn", 0);
                this.WriteYodaRg("ManMdix", 1);
            }
        }
        public void EnableEnergyDetectPowerDown(string enEnergyDetect)
        {
            if (enEnergyDetect == "Disabled")
            {
                this.WriteYodaRg("NrgPdEn", 0);
            }
            else if(enEnergyDetect == "Enabled")
            {
                this.WriteYodaRg("NrgPdEn", 1);
                this.WriteYodaRg("NrgPdTxEn", 0);
            }
            else
            {
                this.WriteYodaRg("NrgPdEn", 1);
                this.WriteYodaRg("NrgPdTxEn", 1);
            }
        }
        public void SetGpClkPinControl(string gpClkPinCtrl)
        {
            this.WriteYodaRg("GeClkCfg", 0);

            switch(gpClkPinCtrl)
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
        public void SetTestMode(TestModeListingModel testMode)
        {
            //WriteYodaRg("GeSftRst", 1);
            //WriteYodaRg("GePhySftPdCfg", 1);
            //WriteYodaRg("GePhyRst", 1);
            //WriteYodaRg("LnkWdEn", 0);
            //WriteYodaRg("NrgPdEn", 0);
            //WriteYodaRg("DnSpeedTo10En", 0);
            //WriteYodaRg("ArbWdEn", 0);
            //WriteYodaRg("B10LpTxEn", 1);
            //WriteYodaRg("EeeAdv", 0);
            //WriteYodaRg("ExtNextPageAdv", 0);
            //WriteYodaRg("GeFifoDpth", 0);
            //WriteYodaRg("DpthMiiByte", 0);

            //if (testMode.Name1 == "100BASE-TX VOD")
            //{
            //    WriteYodaRg("AutonegEn", 0);
            //    WriteYodaRg("SpeedSelMsb", 0);
            //    WriteYodaRg("SpeedSelLsb", 1);
            //    WriteYodaRg("AutoMdiEn", 0);
            //    WriteYodaRg("ManMdiEn", 0);
            //    WriteYodaRg("LinkEn", 1);
            //}
            //else if(testMode.Name1 == "10BASE-T Link Pulse")
            //{
            //    WriteYodaRg("AutonegEn", 0);
            //    WriteYodaRg("SpeedSelMsb", 0);
            //    WriteYodaRg("SpeedSelLsb", 0);
            //    WriteYodaRg("AutoMdiEn", 0);
            //    WriteYodaRg("ManMdiEn", 0);
            //    WriteYodaRg("LbTxSup", 0);
            //    WriteYodaRg("Loopback", 1);
            //    WriteYodaRg("LinkEn", 1);
            //}
            //else
            //{
            //    switch (testMode.Name1)
            //    {
            //        case "10BASE-T TX 5 MHz DIM 1":
            //            WriteYodaRg("B10TxTstMode", 4);
            //            break;
            //        case "10BASE-T TX 10 MHz DIM 1":
            //            WriteYodaRg("B10TxTstMode", 3);
            //            break;
            //        case "10BASE-T TX 5 MHz DIM 0":
            //            WriteYodaRg("B10TxTstMode", 2);
            //            break;
            //        case "10BASE-T TX 10 MHz DIM 0":
            //            WriteYodaRg("B10TxTstMode", 1);
            //            break;
            //        default:
            //            // None assigned
            //            break;
            //    }
            //}

            //WriteYodaRg("SftPd", 0);

            throw new NotImplementedException();
        }

        public void DownSpeed100Hd(bool dwnSpd100Hd)
        {
            throw new NotImplementedException();
        }

        public void ReadRegsiters()
        {
            foreach (var register in _registersBG)
            {
                register.Value = ReadYodaRg(register.Address);
            }
            Debug.WriteLine("ReadRegisters Done");
        }
    }
}
