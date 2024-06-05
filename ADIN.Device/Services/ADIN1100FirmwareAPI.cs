using ADI.Register.Models;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using FTDIChip.Driver.Services;
using Helper.Feedback;
using Helper.RegularExpression;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace ADIN.Device.Services
{
    public class ADIN1100FirmwareAPI : IADIN1100API
    {
        private bool _autoNegotiationStatus = false;
        private string _feedbackMessage;
        private IFTDIServices _ftdiService;
        private object _mainLock;
        private uint _phyAddress;
        private EthPhyState _phyState;
        private ObservableCollection<RegisterModel> _registers;
        private BoardRevision _boardRev;

        public ADIN1100FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, uint phyAddress, object mainLock)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
            _mainLock = mainLock;
        }

        public BoardRevision GetRevNum()
        {
            int i;
            for (i = 0; i < 8; i++)
            {
                var value = MdioReadCl45(Convert.ToUInt32(0x1E0003));
                var revNum = Convert.ToUInt32(value, 16) & 0x03;

                switch (revNum)
                {
                    case 1:
                        _boardRev = BoardRevision.Rev1;
                        break;

                    case 0:
                        _boardRev = BoardRevision.Rev0;
                        break;

                    default:
                        _boardRev = BoardRevision.Rev1;
                        break;
                }
                break;
            }
            return _boardRev;
        }

        public event EventHandler<FrameType> FrameContentChanged;

        public event EventHandler<string> FrameGenCheckerTextStatusChanged;

        public event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        public event EventHandler<FeedbackModel> WriteProcessCompleted;

        public bool isFrameGenCheckerOngoing { get; set; }
        public void DisableLinking(bool isDisabledLinking)
        {
            throw new NotImplementedException();
        }

        public string GetAnStatus()
        {
            if (ReadYodaRg("AN_EN") == "1")
            {
                _autoNegotiationStatus = true;
                return "Enabled";
            }
            else
            {
                _autoNegotiationStatus = false;
                return "Disabled";
            }
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

        public string GetMasterSlaveStatus()
        {
            string masterSlaveStatus = "Configuration fault";

            if (_autoNegotiationStatus)
            {
                switch (ReadYodaRg("AN_MS_CONFIG_RSLTN"))
                {
                    case "0":
                    case "1":
                        masterSlaveStatus = "Configuration fault";
                        break;

                    case "2":
                        masterSlaveStatus = "Slave";
                        break;

                    case "3":
                        masterSlaveStatus = "Master";
                        break;

                    default:
                        masterSlaveStatus = "Undetermine";
                        break;
                }
            }
            else
            {
                masterSlaveStatus = ReadYodaRg("CFG_MST") == "1" ? "Master" : "Slave";
            }

            return masterSlaveStatus;
        }

        public string GetMseValue()
        {
            //if (_boardRev == BoardRevision.Rev0)
            //    return "N/A";

            if (_phyState != EthPhyState.LinkUp)
                return "N/A";

            // Formula:
            // where mse is the value from the register, and sym_pwr_exp is a constant 0.64423.
            // mse_db = 10 * log10((mse / 218) / sym_pwr_exp)
            double mse = Convert.ToUInt32(ReadYodaRg("MSE_VAL"), 16);
            double sym_pwr_exp = 0.64423;
            double mse_db = 10 * Math.Log10((mse / Math.Pow(2, 18)) / sym_pwr_exp);

            //OnMseValueChanged(mse_db.ToString("0.00") + " dB");
            return $"{mse_db.ToString("0.00")} dB";
        }

        public EthPhyState GetPhyState()
        {
            if (ReadYodaRg("CRSM_SFT_PD") == "1")
                return _phyState = EthPhyState.Powerdown;

            if (!(ReadYodaRg("AN_LINK_STATUS") == "1"))
                return _phyState = EthPhyState.LinkDown;

            return _phyState = EthPhyState.LinkUp;
        }

        public string GetSpeedMode()
        {
            throw new NotImplementedException();
        }

        public string GetTxLevelStatus()
        {
            if (_boardRev == BoardRevision.Rev1)
            {
                switch (ReadYodaRg("AN_TX_LVL_RSLTN"))
                {
                    case "0":
                        return "N/A";

                    case "2":
                        return $"{1.0.ToString()} Vpk-pk";

                    case "3":
                        return $"{2.4.ToString()} Vpk-pk";

                    default:
                        return "-";
                }
            }
            else /*if (_boardRev == BoardRevision.Rev0)*/
            {
                string hi_req = ReadYodaRg("AN_ADV_B10L_TX_LVL_HI_REQ");
                string hi_abl = ReadYodaRg("AN_ADV_B10L_TX_LVL_HI_ABL");

                string lp_hi_req = ReadYodaRg("AN_LP_ADV_B10L_TX_LVL_HI_REQ");
                string lp_hi_abl = ReadYodaRg("AN_LP_ADV_B10L_TX_LVL_HI_ABL");

                if ((hi_abl != "1") || (lp_hi_abl != "1"))
                {
                    /* One or both sides cannot do HI, therfore must be low*/
                    return "1.0 Vpk-pk";
                }
                else
                if ((hi_req == "0") && (lp_hi_req == "0"))
                {
                    // Both can manage HI, but neither are requesting it
                    return "1.0 Vpk-pk";
                }
                else
                {
                    // Both can manage HI, and one or both are requesting it
                    return "2.4 Vpk-pk";
                }
            }

            //throw new NotImplementedException();
        }

        public List<string> LocalAdvertisedSpeedList()
        {
            throw new NotImplementedException();
        }

        public string MdioReadCl22(uint regAddress)
        {
            throw new NotImplementedException();
        }

        public string MdioReadCl45(uint regAddress)
        {
            lock (_mainLock)
            {
                string response = string.Empty;
                string command = string.Empty;
                string command2 = string.Empty;

                command = $"mdiord_cl45 {_phyAddress},{regAddress.ToString("X")}\n";

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
            throw new NotImplementedException();
        }

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            lock (_mainLock)
            {
                string response = string.Empty;
                string command = string.Empty;

                command = $"mdiowr_cl45 {_phyAddress},{regAddress.ToString("X")},{data.ToString("X")}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);

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

        public string RegisterRead(string register)
        {
            return ReadYodaRg(register);
        }

        public string RegisterWrite(uint regAddress, uint data)
        {
            return WriteYodaRg(regAddress, data);
        }

        public List<string> RemoteAdvertisedSpeedList()
        {
            throw new NotImplementedException();
        }

        public void ResetFrameGenCheckerStatistics()
        {
            throw new NotImplementedException();
        }

        public void ResetPhy(ResetType reset)
        {
            throw new NotImplementedException();
        }

        public void RestartAutoNegotiation()
        {
            throw new NotImplementedException();
        }

        public void SetFrameCheckerSetting(FrameGenCheckerModel frameContent)
        {
            throw new NotImplementedException();
        }

        public void SetLoopbackSetting(LoopbackListingModel loopback)
        {
            throw new NotImplementedException();
        }

        public void SetMasterSlave(string masterSlaveAdvertise)
        {
            switch (masterSlaveAdvertise)
            {
                case "Prefer_Master":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_ADV_MST", 1);
                    WriteYodaRg("AN_ADV_FORCE_MS", 0);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    break;
                case "Prefer_Slave":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_ADV_MST", 0);
                    WriteYodaRg("AN_ADV_FORCE_MS", 0);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    break;
                case "Forced_Master":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_ADV_MST", 1);
                    WriteYodaRg("AN_ADV_FORCE_MS", 1);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    break;
                case "Forced_Slave":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_ADV_MST", 0);
                    WriteYodaRg("AN_ADV_FORCE_MS", 1);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    break;
                default:
                    break;
            }
        }

        public void SetTestMode(TestModeListingModel testMode, uint framelength)
        {
            throw new NotImplementedException();
        }

        public void SetTxLevel(string txLevel)
        {
            switch (txLevel)
            {
                case "Capable2p4Volts_Requested2p4Volts":
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_ABL", 1);
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_REQ", 1);
                    break;
                case "Capable2p4Volts_Requested1Volt":
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_ABL", 1);
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_REQ", 0);
                    break;
                case "Capable1Volt":
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_ABL", 0);
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_REQ", 0);
                    break;
                default:
                    break;
            }

            WriteYodaRg("AN_RESTART", 1);
            FeedbackLog("restart auto negotiation", FeedbackType.Info);
        }

        public void SoftwarePowerdown(bool isSoftwarePowerdown)
        {
            throw new NotImplementedException();
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

        private string ReadYodaRg(string name)
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