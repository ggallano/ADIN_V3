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
        private string _feedbackMessage;
        private IFTDIServices _ftdiService;
        private uint _phyAddress;
        private EthPhyState _phyState;
        private ObservableCollection<RegisterModel> _registers;
        private IRegisterService _registerService;
        private object _thisLock = new object();
        private string command = string.Empty;
        private string command2 = string.Empty;
        private string response = string.Empty;

        public ADIN1300FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, uint phyAddress, object mainLock)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
            //_thisLock = mainLock;
        }

        public event EventHandler<FrameType> FrameContentChanged;

        public event EventHandler<string> FrameGenCheckerTextStatusChanged;

        public event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        public event EventHandler<FeedbackModel> WriteProcessCompleted;
        public bool isFrameGenCheckerOngoing { get; set; } = false;

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

        public void DisableLinking(bool isDisabledLinking)
        {
            if (isDisabledLinking)
            {
                WriteYodaRg("LinkEn", 0);
                FeedbackLog("disable Linking", FeedbackType.Info);
            }
            else
            {
                WriteYodaRg("LinkEn", 1);
                FeedbackLog("enable Linking", FeedbackType.Info);
            }
        }

        public void DownSpeed100Hd(bool dwnSpd100Hd)
        {
            throw new NotImplementedException();
        }

        public void DownSpeed10Hd(bool dwnSpd10Hd)
        {
            if (dwnSpd10Hd)
            {
                FeedbackLog("enable downspeed to 10BASE-T", FeedbackType.Info);
                this.WriteYodaRg("DnSpeedTo10En", 1);
            }
            else
            {
                FeedbackLog("disable downspeed to 10BASE-T", FeedbackType.Info);
                this.WriteYodaRg("DnSpeedTo10En", 0);
            }
        }

        public void DownSpeedRetriesSetVal(uint dwnSpdRtryVal)
        {
            this.WriteYodaRg("NumSpeedRetry", dwnSpdRtryVal);
        }

        public void EnableEnergyDetectPowerDown(string enEnergyDetect)
        {
            if (enEnergyDetect == "Disabled")
            {
                this.WriteYodaRg("NrgPdEn", 0);
                _feedbackMessage = "disable EDPD";
            }
            else if (enEnergyDetect == "Enabled")
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

        public void GetFrameCheckerStatus()
        {
            throw new NotImplementedException();
        }

        public string GetFrameGeneratorStatus()
        {
            throw new NotImplementedException();
        }

        public string GetLinkStatus()
        {
            return GetPhyState().ToString();
        }

        public string GetMseValue()
        {
            throw new NotImplementedException();
        }

        public EthPhyState GetPhyState()
        {
            if (ReadYogaRg("SftPd") == "1")
            {
                return _phyState = EthPhyState.Powerdown;
            }

            if (ReadYogaRg("LinkEn") == "0")
            {
                return _phyState = EthPhyState.Standby;
            }

            if (!(ReadYogaRg("LinkStatLat") == "1"))
            {
                return _phyState = EthPhyState.LinkDown;
            }

            return _phyState = EthPhyState.LinkUp;
        }

        public string MdioReadCl22(uint regAddress)
        {
            response = string.Empty;
            command = string.Empty;

            lock (_thisLock)
            {
                command = $"mdioread {_phyAddress},{regAddress.ToString("X")}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();

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
            response = string.Empty;
            command = string.Empty;
            command2 = string.Empty;

            lock (_thisLock)
            {
                MdioWriteCl22(0x10, (regAddress & 0xFFFF));
                command = $"mdioread {_phyAddress},11\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();

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
            response = string.Empty;
            command = string.Empty;

            lock (_thisLock)
            {
                command = $"mdiowrite {_phyAddress},{regAddress.ToString("X")},{data.ToString("X")}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();

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

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            response = string.Empty;
            command = string.Empty;
            command2 = string.Empty;

            lock (_thisLock)
            {
                command = $"mdiowrite {_phyAddress},10,{regAddress.ToString("X")}\n";
                command2 = $"mdiowrite {_phyAddress},11,{data.ToString("X")}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
                _ftdiService.SendData(command2);
                response = _ftdiService.ReadCommandResponse().Trim();

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

        public ObservableCollection<RegisterModel> ReadRegsiters()
        {
            foreach (var register in _registers)
            {
                register.Value = ReadYodaRg(register.Address);
            }
            Debug.WriteLine("ReadRegisters Done");

            return _registers;
        }

        public string RegisterRead(uint regAddress)
        {
            return ReadYodaRg(regAddress);
        }

        public string RegisterWrite(uint regAddress, uint data)
        {
            return WriteYodaRg(regAddress, data);
        }

        public void ResetFrameGenCheckerStatistics()
        {
            throw new NotImplementedException();
        }

        public void RestartAutoNegotiation()
        {
            throw new NotImplementedException();
        }

        public void SetForcedSpeed(string setFrcdSpd)
        {
            throw new NotImplementedException();
        }

        public void SetFrameCheckerSetting(FrameGenCheckerModel frameContent)
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

        public void SetLoopbackSetting(LoopbackListingModel loopback)
        {
            throw new NotImplementedException();
        }

        public void SetTestMode(TestModeListingModel testMode, uint framelength)
        {
            throw new NotImplementedException();
        }

        public void SoftwarePowerdown(bool isSoftwarePowerdown)
        {
            if (isSoftwarePowerdown)
            {
                WriteYodaRg("SftPd", 1);
            }
            else
            {
                WriteYodaRg("SftPd", 0);
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

        protected virtual void OnWriteProcessCompleted(FeedbackModel feedback)
        {
            WriteProcessCompleted?.Invoke(this, feedback);
        }
        private void FeedbackLog(string message, FeedbackType feedbackType)
        {
            FeedbackModel feedback = new FeedbackModel();
            feedback.Message = message;
            feedback.FeedBackType = feedbackType;
            OnWriteProcessCompleted(feedback);
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

        private string WriteYodaRg(uint registerAddress, uint value)
        {
            uint pageNumber = registerAddress >> 16;
            if (pageNumber == 0)
            {
                return MdioWriteCl22(registerAddress, value);
            }
            else
            {
                return MdioWriteCl45(registerAddress, value);
            }
        }
    }
}
