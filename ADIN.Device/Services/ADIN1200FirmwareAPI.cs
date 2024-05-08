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
        private string _feedbackMessage;

        public ADIN1200FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, ObservableCollection<RegisterModel> registersBG, uint phyAddress)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _registersBG = registersBG;
            _phyAddress = phyAddress;
        }

        public event EventHandler<FeedbackModel> WriteProcessCompleted;

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
                        {
                            bitField.Value = value;
                            _feedbackMessage = "BitField \"" + bitField.Name + "\" = " + bitField.Value;
                            FeedbackLog(_feedbackMessage, FeedbackType.Verbose);
                        }
                    }
                }
            }
            else
            {
                register = res[0];
                register.Value = value.ToString("X");
                _feedbackMessage = "Register \"" + register.Name + "\" = " + register.Value;
                FeedbackLog(_feedbackMessage, FeedbackType.Verbose);
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
            _feedbackMessage = "Locally Advertised Speeds:";

            if (listAdvSpd.Contains("SPEED_1000BASE_T_FD_SPEED"))
            {
                _feedbackMessage = _feedbackMessage + " SPEED_1000BASE_T_FD_SPEED";
            }
            if (listAdvSpd.Contains("SPEED_1000BASE_T_HD_SPEED"))
            {
                _feedbackMessage = _feedbackMessage + " SPEED_1000BASE_T_HD_SPEED";
            }
            if (listAdvSpd.Contains("SPEED_100BASE_TX_FD_SPEED"))
            {
                _feedbackMessage = _feedbackMessage + " SPEED_100BASE_TX_FD_SPEED";
            }
            if (listAdvSpd.Contains("SPEED_100BASE_TX_HD_SPEED"))
            {
                _feedbackMessage = _feedbackMessage + " SPEED_100BASE_TX_HD_SPEED";
            }
            if (listAdvSpd.Contains("SPEED_10BASE_T_FD_SPEED"))
            {
                _feedbackMessage = _feedbackMessage + " SPEED_10BASE_T_FD_SPEED";
            }
            if (listAdvSpd.Contains("SPEED_10BASE_T_HD_SPEED"))
            {
                _feedbackMessage = _feedbackMessage + " SPEED_10BASE_T_HD_SPEED";
            }
            if (listAdvSpd.Contains("SPEED_1000BASE_EEE_SPEED"))
            {
                _feedbackMessage = _feedbackMessage + " SPEED_1000BASE_EEE_SPEED";
            }
            if (listAdvSpd.Contains("SPEED_100BASE_EEE_SPEED"))
            {
                _feedbackMessage = _feedbackMessage + " SPEED_100BASE_EEE_SPEED";
            }

            FeedbackLog(_feedbackMessage, FeedbackType.Info);
        }

        public void AdvertisedForcedSpeed(string advFrcSpd)
        {
            if (advFrcSpd == "Advertised")
            {
                this.WriteYodaRg("AutonegEn", 1);
                _feedbackMessage = "enable Auto-Negotiation";
            }
            else
            {
                this.WriteYodaRg("AutonegEn", 0);
                _feedbackMessage = "disable Auto-Negotiation";
            }

            FeedbackLog(_feedbackMessage, FeedbackType.Info);
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
                _feedbackMessage = "disable EDPD";
            }
            else if(enEnergyDetect == "Enabled")
            {
                this.WriteYodaRg("NrgPdEn", 1);
                this.WriteYodaRg("NrgPdTxEn", 0);
                _feedbackMessage = "enable EDPD - no transmission of pulse";
            }
            else
            {
                this.WriteYodaRg("NrgPdEn", 1);
                this.WriteYodaRg("NrgPdTxEn", 1);
                _feedbackMessage = "enable EDPD - with periodic transmission of pulse";
            }

            FeedbackLog(_feedbackMessage, FeedbackType.Info);
        }
        public void SetGpClkPinControl(string gpClkPinCtrl)
        {
            this.WriteYodaRg("GeClkCfg", 0);

            switch(gpClkPinCtrl)
            {
                case "125 MHz PHY Recovered":
                    this.WriteYodaRg("GeClkRcvr125En", 1);
                    _feedbackMessage = "GE PHY 125 MHz recovered clock output on GP_CLK pin";
                    break;
                case "125 MHz PHY Free Running":
                    this.WriteYodaRg("GeClkFree125En", 1);
                    _feedbackMessage = "GE PHY 125 MHz free-running clock output on GP_CLK pin";
                    break;
                case "Recovered HeartBeat":
                    this.WriteYodaRg("GeClkHrtRcvrEn", 1);
                    _feedbackMessage = "GE PHY recovered heartbeat clock output on GP_CLK pin";
                    break;
                case "Free Running HeartBeat":
                    this.WriteYodaRg("GeClkHrtFreeEn", 1);
                    _feedbackMessage = "GE PHY free-running heartbeat clock output on GP_CLK pin";
                    break;
                case "25 MHz Reference":
                    this.WriteYodaRg("GeClk25En", 1);
                    _feedbackMessage = "GE PHY 25 MHz clock output on GP_CLK pin";
                    break;
                default:
                    _feedbackMessage = "No clock output on GP_CLK pin";
                    break;
            }

            FeedbackLog(_feedbackMessage, FeedbackType.Info);
        }
        public void SetTestMode(TestModeListingModel testMode)
        {
            //FeedbackLog("GESubsys software reset", FeedbackType.Info);
            //WriteYodaRg("GeSftRst", 1);
            //FeedbackLog("GE PHY enter software reset, stays in software powerdown", FeedbackType.Info);
            //WriteYodaRg("GePhySftPdCfg", 1);
            //WriteYodaRg("GePhyRst", 1);
            //FeedbackLog("Apply base settings for UNH-IOL testing", FeedbackType.Info);
            //WriteYodaRg("LnkWdEn", 0);
            //FeedbackLog("disable energy detect power-down", FeedbackType.Info);
            //WriteYodaRg("NrgPdEn", 0);
            //FeedbackLog("disable automatic speed down-shift", FeedbackType.Info);
            //WriteYodaRg("DnSpeedTo10En", 0);
            //WriteYodaRg("ArbWdEn", 0);
            //WriteYodaRg("B10LpTxEn", 1);
            //FeedbackLog("disable Energy Efficient Ethernet", FeedbackType.Info);
            //WriteYodaRg("EeeAdv", 0);
            //FeedbackLog("disable extended next pages", FeedbackType.Info);
            //WriteYodaRg("ExtNextPageAdv", 0);
            //WriteYodaRg("GeFifoDpth", 0);
            //WriteYodaRg("DpthMiiByte", 0);

            //switch (testMode.Name1)
            //{
            //    case "100BASE-TX VOD":
            //        FeedbackLog("configure for auto-negotiation disable, 100BASE-TX, forced MDI, linking enabled", FeedbackType.Info);
            //        WriteYodaRg("AutonegEn", 0);
            //        WriteYodaRg("SpeedSelMsb", 0);
            //        WriteYodaRg("SpeedSelLsb", 1);
            //        WriteYodaRg("AutoMdiEn", 0);
            //        WriteYodaRg("ManMdiEn", 0);
            //        WriteYodaRg("LinkEn", 1);
            //        FeedbackLog("exit software powerdown", FeedbackType.Info);
            //        WriteYodaRg("SftPd", 0);
            //        FeedbackLog("Device configured for 100BASE-TX VOD measurement", FeedbackType.Info);
            //        break;

            //    case "10BASE-T Link Pulse":
            //        FeedbackLog("configure for auto-negotiation disabled, 10BASE-T", FeedbackType.Info);
            //        FeedbackLog("forced MDI, loopback enabled, Tx suppression disabled, linking enabled", FeedbackType.Info);
            //        WriteYodaRg("AutonegEn", 0);
            //        WriteYodaRg("SpeedSelMsb", 0);
            //        WriteYodaRg("SpeedSelLsb", 0);
            //        WriteYodaRg("AutoMdiEn", 0);
            //        WriteYodaRg("ManMdiEn", 0);
            //        WriteYodaRg("LbTxSup", 0);
            //        WriteYodaRg("Loopback", 1);
            //        WriteYodaRg("LinkEn", 1);
            //        FeedbackLog("exit software powerdown", FeedbackType.Info);
            //        WriteYodaRg("SftPd", 0);
            //        FeedbackLog("Device configured for 10BASE-TX forced mode link pulse measurement", FeedbackType.Info);
            //        break;

            //    case "10BASE-T TX 5 MHz DIM 1":
            //        FeedbackLog("10BASE-T transmit 5 MHz square wave test mode transmission on dim 1", FeedbackType.Info);
            //        WriteYodaRg("B10TxTstMode", 4);
            //        FeedbackLog("exit software powerdown", FeedbackType.Info);
            //        WriteYodaRg("SftPd", 0);
            //        FeedbackLog("Device configured for 10BASE-T test mode transmission (5 MHz)", FeedbackType.Info);
            //        break;

            //    case "10BASE-T TX 10 MHz DIM 1":
            //        FeedbackLog("10BASE-T transmit 10 MHz square wave test mode transmission on dim 1", FeedbackType.Info);
            //        WriteYodaRg("B10TxTstMode", 3);
            //        FeedbackLog("exit software powerdown", FeedbackType.Info);
            //        WriteYodaRg("SftPd", 0);
            //        FeedbackLog("Device configured for 10BASE-T test mode transmission (10 MHz)", FeedbackType.Info);
            //        break;

            //    case "10BASE-T TX 5 MHz DIM 0":
            //        FeedbackLog("10BASE-T transmit 5 MHz square wave test mode transmission on dim 0", FeedbackType.Info);
            //        WriteYodaRg("B10TxTstMode", 2);
            //        FeedbackLog("exit software powerdown", FeedbackType.Info);
            //        WriteYodaRg("SftPd", 0);
            //        FeedbackLog("Device configured for 10BASE-T test mode transmission (5 MHz)", FeedbackType.Info);
            //        break;

            //    case "10BASE-T TX 10 MHz DIM 0":
            //        FeedbackLog("10BASE-T transmit 10 MHz square wave test mode transmission on dim 0", FeedbackType.Info);
            //        WriteYodaRg("B10TxTstMode", 1);
            //        FeedbackLog("exit software powerdown", FeedbackType.Info);
            //        WriteYodaRg("SftPd", 0);
            //        FeedbackLog("Device configured for 10BASE-T test mode transmission (10 MHz)", FeedbackType.Info);
            //        break;

            //    default:
            //        throw new NotImplementedException();
            //}
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

        protected virtual void OnWriteProcessCompleted(FeedbackModel feedback)
        {
            WriteProcessCompleted?.Invoke(this, feedback);
        }

        private void FeedbackLog(string Message, FeedbackType feedbackType)
        {
            FeedbackModel feedback = new FeedbackModel();
            feedback.Message = Message;
            feedback.FeedBackType = feedbackType;
            OnWriteProcessCompleted(feedback);
        }
    }
}
