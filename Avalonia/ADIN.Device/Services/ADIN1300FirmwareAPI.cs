using ADIN.Device.Models;
using ADIN.Register.Models;
using ADIN.Register.Services;
using FTDIChip.Driver.Services;
using Helper.Feedback;
using Helper.SignalToNoiseRatio;
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
    public class ADIN1300FirmwareAPI : IADIN1300API
    {
        private BoardRevision _boardRev;
        private string _feedbackMessage;
        private IFTDIServices _ftdiService;
        private object _mainLock = new object();
        private uint _phyAddress;
        private EthPhyState _phyState;
        private ObservableCollection<RegisterModel> _registers;
        private IRegisterService _registerService;
        private uint checkedFrames = 0;
        private uint checkedFramesErrors = 0;
        private MseModel _mse = new MseModel("-");
        private uint fcEn_st;

        public ADIN1300FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, uint phyAddress, object mainLock)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
            _mainLock = mainLock;
        }

        public event EventHandler<FrameType> FrameContentChanged;

        public event EventHandler<string> FrameGenCheckerTextStatusChanged;

        public event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        public event EventHandler<string> ResetFrameGenCheckerErrorStatisticsChanged;

        public event EventHandler<FeedbackModel> WriteProcessCompleted;

        public event EventHandler<List<string>> GigabitCableDiagCompleted;

        public bool isFrameGenCheckerOngoing { get; set; } = false;

        public void AdvertisedForcedSpeed(string advFrcSpd)
        {
            if (advFrcSpd == "Advertised")
            {
                this.WriteYodaRg("AutonegEn", 1);
                _feedbackMessage = "enabled Auto-Negotiation";
            }
            else
            {
                this.WriteYodaRg("AutonegEn", 0);
                _feedbackMessage = "disabled Auto-Negotiation";
            }

            FeedbackLog(_feedbackMessage, FeedbackType.Info);
        }

        public void AutoMDIXMode(string autoMDIXmod)
        {
            if (autoMDIXmod == "Auto MDIX")
            {
                this.WriteYodaRg("AutoMdiEn", 1);
                this.WriteYodaRg("ManMdix", 0);
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

        public void DisableLinking(bool isDisabledLinking)
        {
            if (isDisabledLinking)
            {
                WriteYodaRg("LinkEn", 0);
                FeedbackLog("Disable linking", FeedbackType.Info);
            }
            else
            {
                WriteYodaRg("LinkEn", 1);
                FeedbackLog("Enable linking", FeedbackType.Info);
            }
        }

        public void DownSpeed100Hd(bool dwnSpd100Hd)
        {
            if (dwnSpd100Hd)
            {
                this.WriteYodaRg("DnSpeedTo100En", 1);
                FeedbackLog("enabled downspeed to 10BASE-T", FeedbackType.Info);
            }
            else
            {
                this.WriteYodaRg("DnSpeedTo100En", 0);
                FeedbackLog("disabled downspeed to 10BASE-T", FeedbackType.Info);
            }
        }

        public void DownSpeed10Hd(bool dwnSpd10Hd)
        {
            if (dwnSpd10Hd)
            {
                this.WriteYodaRg("DnSpeedTo10En", 1);
                FeedbackLog("enabled downspeed to 10BASE-T", FeedbackType.Info);
            }
            else
            {
                this.WriteYodaRg("DnSpeedTo10En", 0);
                FeedbackLog("disabled downspeed to 10BASE-T", FeedbackType.Info);
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
                this.WriteYodaRg("NrgPdTxEn", 0);
                _feedbackMessage = "disabled EDPD";
            }
            else if (enEnergyDetect == "Enabled")
            {
                this.WriteYodaRg("NrgPdEn", 1);
                this.WriteYodaRg("NrgPdTxEn", 0);
                _feedbackMessage = "enabled EDPD - no transmission of pulse";
            }
            else
            {
                this.WriteYodaRg("NrgPdEn", 1);
                this.WriteYodaRg("NrgPdTxEn", 1);
                _feedbackMessage = "enabled EDPD - with periodic transmission of pulse";
            }

            FeedbackLog(_feedbackMessage, FeedbackType.Info);
        }

        public string GetCableLength()
        {
            var CdiagCblLenEst = this.ReadYodaRg("CdiagCblLenEst");

            if (CdiagCblLenEst == "255")
                return "Unknown Length";

            return CdiagCblLenEst + " m";
        }

        public void GetFrameCheckerStatus()
        {
            fcEn_st = Convert.ToUInt32(ReadYodaRg("FcEn"));
            uint fcTxSel_st = Convert.ToUInt32(ReadYodaRg("FcTxSel"));

            if (fcEn_st == 0)
            {
                OnResetFrameGenCheckerStatisticsChanged("Disabled");
                return;
            }

            uint errCnt = Convert.ToUInt32(ReadYodaRg("RxErrCnt"));
            uint fCntL = Convert.ToUInt32(ReadYodaRg("FcFrmCntL"));
            uint fCntH = Convert.ToUInt32(ReadYodaRg("FcFrmCntH"));
            uint fCnt = (65536 * fCntH) + fCntL;

            if (fCnt == 0)
            {
                OnResetFrameGenCheckerStatisticsChanged("-");
                OnResetFrameGenCheckerErrorStatisticsChanged("-");
                //return;
            }

            checkedFrames += fCnt;
            checkedFramesErrors += errCnt;

            if (fcTxSel_st == 0)
            {
                OnResetFrameGenCheckerStatisticsChanged($"{checkedFrames}");
                OnResetFrameGenCheckerErrorStatisticsChanged($"{checkedFramesErrors}");
                return;
            }

            OnResetFrameGenCheckerStatisticsChanged($"{checkedFrames}");
            OnResetFrameGenCheckerErrorStatisticsChanged($"{checkedFramesErrors}");
        }

        public string GetFrameGeneratorStatus()
        {
            uint fgEn_st = Convert.ToUInt32(ReadYodaRg("FgEn"), 16);
            //uint fcTxSel_st = Convert.ToUInt32(ReadYodaRg("FC_TX_SEL"), 16);
            uint fgContModeEn_st = Convert.ToUInt32(ReadYodaRg("FgContModeEn"), 16);

            if (fgEn_st == 0)
            {
                OnFrameGenCheckerStatusChanged("Generate");
                return "Not Enabled";
            }

            if (fgContModeEn_st == 1)
            {
                isFrameGenCheckerOngoing = true;
                OnFrameGenCheckerStatusChanged("Terminate");
                return "Frame Transmission in progress";
            }

            uint fgDone_st = Convert.ToUInt32(ReadYodaRg("FgDone"), 16);
            if (fgDone_st != 0)
            {
                WriteYodaRg("FgEn", 0);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Transmission completed", FeedBackType = FeedbackType.Info });
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Generator disabled", FeedBackType = FeedbackType.Info });
                isFrameGenCheckerOngoing = false;
                OnFrameGenCheckerStatusChanged("Generate");
                return "Frame Transmission completed";
            }

            isFrameGenCheckerOngoing = true;
            OnFrameGenCheckerStatusChanged("Terminate");
            return "Frame Transmission in progress";
        }

        public string GetLinkStatus()
        {
            return GetPhyState().ToString();
        }

        public MseModel GetMseValue()
        {
            if (_phyState != EthPhyState.LinkUp)
                return new MseModel("-");

            // Formula:
            // where mse is the value from the register, and sym_pwr_exp is a constant 0.64423.
            // mse_db = 10 * log10((mse / 218) / sym_pwr_exp)

            _mse.MseA_Raw = ReadYodaRg("MseA");
            if (_mse.MseA_Max == "-" || Convert.ToUInt32(_mse.MseA_Max) < Convert.ToUInt32(_mse.MseA_Raw))
                _mse.MseA_Max = _mse.MseA_Raw;
            _mse.MseA_Combined = _mse.MseA_Raw + ", " + _mse.MseA_Max;
            _mse.MseA_dB = SignalToNoiseRatio.GigabitCompute(Convert.ToDouble(_mse.MseA_Raw)).ToString("0.00") + " dB";

            var resolvedHCD = (EthernetSpeeds)Convert.ToUInt32(ReadYodaRg("HcdTech"));

            if ((resolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_HD)
             || (resolvedHCD == EthernetSpeeds.SPEED_1000BASE_T_FD))
            {
                _mse.MseB_Raw = ReadYodaRg("MseB");
                _mse.MseC_Raw = ReadYodaRg("MseC");
                _mse.MseD_Raw = ReadYodaRg("MseD");

                if (_mse.MseB_Max == "-" || Convert.ToUInt32(_mse.MseB_Max) < Convert.ToUInt32(_mse.MseB_Raw))
                    _mse.MseB_Max = _mse.MseB_Raw;
                if (_mse.MseC_Max == "-" || Convert.ToUInt32(_mse.MseC_Max) < Convert.ToUInt32(_mse.MseC_Raw))
                    _mse.MseC_Max = _mse.MseC_Raw;
                if (_mse.MseD_Max == "-" || Convert.ToUInt32(_mse.MseD_Max) < Convert.ToUInt32(_mse.MseD_Raw))
                    _mse.MseD_Max = _mse.MseD_Raw;

                _mse.MseB_Combined = _mse.MseB_Raw + ", " + _mse.MseB_Max;
                _mse.MseC_Combined = _mse.MseC_Raw + ", " + _mse.MseC_Max;
                _mse.MseD_Combined = _mse.MseD_Raw + ", " + _mse.MseD_Max;

                _mse.MseB_dB = SignalToNoiseRatio.GigabitCompute(Convert.ToDouble(_mse.MseB_Raw)).ToString("0.00") + " dB";
                _mse.MseC_dB = SignalToNoiseRatio.GigabitCompute(Convert.ToDouble(_mse.MseC_Raw)).ToString("0.00") + " dB";
                _mse.MseD_dB = SignalToNoiseRatio.GigabitCompute(Convert.ToDouble(_mse.MseD_Raw)).ToString("0.00") + " dB";
            }
            else
            {
                _mse.MseB_Combined = "-";
                _mse.MseC_Combined = "-";
                _mse.MseD_Combined = "-";
                _mse.MseB_dB = "-";
                _mse.MseC_dB = "-";
                _mse.MseD_dB = "-";
            }

            return _mse;
        }

        public MseModel GetMseValue(BoardRevision boardRev)
        {
            throw new NotImplementedException();
        }

        public EthPhyState GetPhyState()
        {
            if (ReadYodaRg("SftPd") == "1")
            {
                return _phyState = EthPhyState.Powerdown;
            }

            if (ReadYodaRg("LinkEn") == "0")
            {
                return _phyState = EthPhyState.Standby;
            }

            if (!(ReadYodaRg("LinkStatLat") == "1"))
            {
                return _phyState = EthPhyState.LinkDown;
            }

            return _phyState = EthPhyState.LinkUp;
        }

        public string GetSpeedMode()
        {
            if (ReadYodaRg("AutonegEn") == "1")
            {
                return "Advertised";
            }
            else
            {
                return "Forced";
            }
        }

        public List<string> LocalAdvertisedSpeedList()
        {
            List<string> localSpeeds = new List<string>();

            if (this.ReadYodaRg("Fd1000Adv") == "1")
            {
                localSpeeds.Add("SPEED_1000BASE_T_FD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("Hd1000Adv") == "1")
            {
                localSpeeds.Add("SPEED_1000BASE_T_HD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("Eee1000Adv") == "1")
            {
                localSpeeds.Add("SPEED_1000BASE_EEE_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("Fd100Adv") == "1")
            {
                localSpeeds.Add("SPEED_100BASE_TX_FD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("Hd100Adv") == "1")
            {
                localSpeeds.Add("SPEED_100BASE_TX_HD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("Eee100Adv") == "1")
            {
                localSpeeds.Add("SPEED_100BASE_EEE_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("Fd10Adv") == "1")
            {
                localSpeeds.Add("SPEED_10BASE_T_FD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("Hd10Adv") == "1")
            {
                localSpeeds.Add("SPEED_10BASE_T_HD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("EeeAdv") == "1")
            {
                localSpeeds.Add("SPEED_100BASE_EEE_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            return localSpeeds;
        }

        public void LogAdvertisedSpeed(List<string> listAdvSpd)
        {
            _feedbackMessage = "Locally Advertised Speeds:";

            if (listAdvSpd.Contains("SPEED_1000BASE_T_FD_SPEED"))
            {
                _feedbackMessage += " SPEED_1000BASE_T_FD_SPEED,";
            }
            if (listAdvSpd.Contains("SPEED_1000BASE_T_HD_SPEED"))
            {
                _feedbackMessage += " SPEED_1000BASE_T_HD_SPEED,";
            }
            if (listAdvSpd.Contains("SPEED_100BASE_TX_FD_SPEED"))
            {
                _feedbackMessage += " SPEED_100BASE_TX_FD_SPEED,";
            }
            if (listAdvSpd.Contains("SPEED_100BASE_TX_HD_SPEED"))
            {
                _feedbackMessage += " SPEED_100BASE_TX_HD_SPEED,";
            }
            if (listAdvSpd.Contains("SPEED_10BASE_T_FD_SPEED"))
            {
                _feedbackMessage += " SPEED_10BASE_T_FD_SPEED,";
            }
            if (listAdvSpd.Contains("SPEED_10BASE_T_HD_SPEED"))
            {
                _feedbackMessage += " SPEED_10BASE_T_HD_SPEED,";
            }
            if (listAdvSpd.Contains("SPEED_1000BASE_EEE_SPEED"))
            {
                _feedbackMessage += " SPEED_1000BASE_EEE_SPEED,";
            }
            if (listAdvSpd.Contains("SPEED_100BASE_EEE_SPEED"))
            {
                _feedbackMessage += " SPEED_100BASE_EEE_SPEED,";
            }

            if (_feedbackMessage.EndsWith(","))
            {
                _feedbackMessage = _feedbackMessage.Remove(_feedbackMessage.Length - 1);
            }
            else
            {
                _feedbackMessage += " No advertised speed/s";
            }

            FeedbackLog(_feedbackMessage, FeedbackType.Info);
        }
        public string MdioReadCl22(uint regAddress)
        {
            //lock (_mainLock)
            {
                string response = string.Empty;
                string command = string.Empty;

                command = $"mdioread {_phyAddress},{regAddress.ToString("X")}\n";

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

        public string MdioReadCl45(uint regAddress)
        {
            //lock (_mainLock)
            {
                string response = string.Empty;
                string command = string.Empty;
                string command2 = string.Empty;

                MdioWriteCl22(0x10, (regAddress & 0xFFFF));
                command = $"mdioread {_phyAddress},11\n";

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

        public string MdioWriteCl22(uint regAddress, uint data)
        {
            //lock (_mainLock)
            {
                string response = string.Empty;
                string command = string.Empty;

                command = $"mdiowrite {_phyAddress},{regAddress.ToString("X")},{data.ToString("X")}\n";

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

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            //lock (_mainLock)
            {
                string response = string.Empty;
                string command = string.Empty;
                string command2 = string.Empty;

                command = $"mdiowrite {_phyAddress},10,{regAddress.ToString("X")}\n";
                command2 = $"mdiowrite {_phyAddress},11,{data.ToString("X")}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
                _ftdiService.Purge();
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

        public void PollEqYodaRg(string name, uint expected, double timeout)
        {
            string regContent;
            RegisterModel register;
            register = GetRegister(name);
            long timeout_ms = (long)(timeout * 1000);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            do
            {
                regContent = ReadYodaRg(register.Address);
                if (sw.ElapsedMilliseconds > timeout_ms)
                {
                    throw new TimeoutException(string.Format("Gave up waiting for \"{0}\" to contain \"{1}\" within {2} seconds.", name, expected, timeout));
                }
            }
            while (ExtractFieldValue(regContent, register, name) != expected);

            return;

        }

        public ObservableCollection<RegisterModel> ReadRegsiters()
        {
            foreach (var register in _registers)
            {
                // This condition will skip reading the value for FG_DONE due to conflict in 
                // FrameGenChecker operation it does not terminate properly because the flag was already at zero value.
                if (register.Name == "FgDone")
                    continue;
                if (fcEn_st != 0 && (register.Name == "FcFrmCntL" || register.Name == "FcFrmCntH" || register.Name == "RxErrCnt"))
                    continue;

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
            List<string> remoteSpeeds = new List<string>();

            if (this.ReadYodaRg("LpFd1000Able") == "1")
            {
                remoteSpeeds.Add("SPEED_1000BASE_T_FD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("LpHd1000Able") == "1")
            {
                remoteSpeeds.Add("SPEED_1000BASE_T_HD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("LpEee1000Able") == "1")
            {
                remoteSpeeds.Add("SPEED_1000BASE_EEE_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("LpFd100Able") == "1")
            {
                remoteSpeeds.Add("SPEED_100BASE_TX_FD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("LpHd100Able") == "1")
            {
                remoteSpeeds.Add("SPEED_100BASE_TX_HD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("LpEee100Able") == "1")
            {
                remoteSpeeds.Add("SPEED_100BASE_EEE_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("LpFd10Able") == "1")
            {
                remoteSpeeds.Add("SPEED_10BASE_T_FD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYodaRg("LpHd10Able") == "1")
            {
                remoteSpeeds.Add("SPEED_10BASE_T_HD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            return remoteSpeeds;
        }
        public void ResetFrameGenCheckerStatistics()
        {
            checkedFrames = 0;
            checkedFramesErrors = 0;

            OnResetFrameGenCheckerStatisticsChanged($"{checkedFrames}");
            OnResetFrameGenCheckerErrorStatisticsChanged($"{checkedFramesErrors}");
        }

        public void ResetPhy(ResetType reset)
        {
            switch (reset)
            {
                case ResetType.SubSysPin:
                    WriteYodaRg("GeSftRstCfgEn", 1);
                    WriteYodaRg("GeSftRst", 1);
                    FeedbackLog("SubSys (Pin) reset", FeedbackType.Info);
                    break;

                case ResetType.SubSys:
                    WriteYodaRg("GeSftRstCfgEn", 0);
                    WriteYodaRg("GeSftRst", 1);
                    FeedbackLog("SubSys reset", FeedbackType.Info);
                    break;

                case ResetType.Phy:
                    WriteYodaRg("SftRst", 1);
                    FeedbackLog("Phy reset", FeedbackType.Info);
                    break;

                default:
                    break;
            }
        }

        public void RestartAutoNegotiation()
        {
            WriteYodaRg("RestartAneg", 1);
            FeedbackLog("Restarting auto negotiation...", FeedbackType.Info);
            Debug.WriteLine("Restart Auto Negotiation");
        }

        public void SetForcedSpeed(string setFrcdSpd)
        {
            switch (setFrcdSpd)
            {
                case "SPEED_1000BASE_T_FD":
                    this.WriteYodaRg("SpeedSelMsb", 1);
                    this.WriteYodaRg("SpeedSelLsb", 0);
                    this.WriteYodaRg("DplxMode", 1);
                    this.FeedbackLog("10BASE-T full duplex forced speed selected", FeedbackType.Info);
                    break;
                case "SPEED_100BASE_TX_FD":
                    this.WriteYodaRg("SpeedSelMsb", 0);
                    this.WriteYodaRg("SpeedSelLsb", 1);
                    this.WriteYodaRg("DplxMode", 1);
                    this.FeedbackLog("100BASE-TX full duplex forced speed selected", FeedbackType.Info);
                    break;
                case "SPEED_100BASE_TX_HD":
                    this.WriteYodaRg("SpeedSelMsb", 0);
                    this.WriteYodaRg("SpeedSelLsb", 1);
                    this.WriteYodaRg("DplxMode", 0);
                    this.FeedbackLog("100BASE-TX half duplex forced speed selected", FeedbackType.Info);
                    break;
                case "SPEED_10BASE_T_FD":
                    this.WriteYodaRg("SpeedSelMsb", 0);
                    this.WriteYodaRg("SpeedSelLsb", 0);
                    this.WriteYodaRg("DplxMode", 1);
                    this.FeedbackLog("10BASE-T full duplex forced speed selected", FeedbackType.Info);
                    break;
                case "SPEED_10BASE_T_HD":
                    this.WriteYodaRg("SpeedSelMsb", 0);
                    this.WriteYodaRg("SpeedSelLsb", 0);
                    this.WriteYodaRg("DplxMode", 0);
                    this.FeedbackLog("10BASE-T half duplex forced speed selected", FeedbackType.Info);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void SetFrameCheckerSetting(FrameGenCheckerModel frameContent)
        {
            checkedFrames = 0;
            checkedFramesErrors = 0;

            WriteYodaRg("FcMaxFrmSize", 0xFFFF);
            WriteYodaRg("FcTxSel", 0);

            bool fgEn_st = ReadYodaRg("FgEn") == "1" ? true : false;

            if (fgEn_st)
            {
                WriteYodaRg("FgEn", 0);
                //OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Generator disabled", FeedBackType = FeedbackType.Info });
                isFrameGenCheckerOngoing = false;
                OnFrameGenCheckerStatusChanged("Generate");
            }
            else
            {
                WriteYodaRg("DiagClkEn", 1);
                SetFrameLength(frameContent.FrameLength);
                SetContinuousMode(frameContent.EnableContinuousMode, frameContent.FrameBurst);
                SetFrameContent(frameContent.SelectedFrameContent);
                //SetMacAddresses(frameContent.EnableMacAddress, frameContent.SrcOctet, frameContent.DestOctet);

                WriteYodaRg("FgEn", 1);
                OnFrameGenCheckerStatusChanged("Terminate");
                isFrameGenCheckerOngoing = true;
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"- Started transmission of {frameContent.FrameBurst} frames -", FeedBackType = FeedbackType.Info });
            }
        }

        public void SetGpClkPinControl(string gpClkPinCtrl)
        {
            this.WriteYodaRg("GeClkCfg", 0);

            switch (gpClkPinCtrl)
            {
                case "125 MHz PHY Recovered":
                    this.WriteYodaRg("GeClkRcvr125En", 1);
                    _feedbackMessage = "PHY 125 MHz recovered clock output on GP_CLK pin";
                    break;
                case "125 MHz PHY Free Running":
                    this.WriteYodaRg("GeClkFree125En", 1);
                    _feedbackMessage = "PHY 125 MHz free-running clock output on GP_CLK pin";
                    break;
                case "Recovered HeartBeat":
                    this.WriteYodaRg("GeClkHrtRcvrEn", 1);
                    _feedbackMessage = "PHY recovered heartbeat clock output on GP_CLK pin";
                    break;
                case "Free Running HeartBeat":
                    this.WriteYodaRg("GeClkHrtFreeEn", 1);
                    _feedbackMessage = "PHY free-running heartbeat clock output on GP_CLK pin";
                    break;
                case "25 MHz Reference":
                    this.WriteYodaRg("GeClk25En", 1);
                    _feedbackMessage = "PHY 25 MHz clock output on GP_CLK pin";
                    break;
                default:
                    _feedbackMessage = "No clock output on GP_CLK pin";
                    break;
            }

            FeedbackLog(_feedbackMessage, FeedbackType.Info);
        }

        public void SetLoopbackSetting(LoopbackModel loopback)
        {
            switch (loopback.EnumLoopbackType)
            {
                case LoopBackMode.OFF:
                    this.WriteYodaRg("LbAllDigSel", 0);
                    this.WriteYodaRg("LbLdSel", 0);
                    this.WriteYodaRg("LbExtEn", 0);
                    this.WriteYodaRg("Loopback", 0);
                    FeedbackLog("PHY Loopback disabled", FeedbackType.Info);
                    break;
                case LoopBackMode.Digital:
                    this.WriteYodaRg("LbAllDigSel", 1);
                    this.WriteYodaRg("LbLdSel", 0);
                    this.WriteYodaRg("LbExtEn", 0);
                    this.WriteYodaRg("Loopback", 1);
                    //if (txSuppression)
                    //{
                    //    this.WriteYodaRg("LbTxSup", 1);
                    //    FeedbackLog("PHY Loopback configured as Digital loopback - Tx suppressed", FeedbackType.Info);
                    //}
                    //else
                    //{
                    //    this.WriteYodaRg("LbTxSup", 0);
                    //    FeedbackLog("PHY Loopback configured as Digital loopback - Tx not suppressed", FeedbackType.Info);
                    //}
                    break;
                case LoopBackMode.LineDriver:
                    this.WriteYodaRg("LbLdSel", 1);
                    this.WriteYodaRg("LbAllDigSel", 0);
                    this.WriteYodaRg("LbExtEn", 0);
                    this.WriteYodaRg("Loopback", 1);
                    FeedbackLog("PHY Loopback configured as LineDriver loopback", FeedbackType.Info);
                    break;
                case LoopBackMode.ExtCable:
                    this.WriteYodaRg("LbExtEn", 1);
                    this.WriteYodaRg("LbAllDigSel", 0);
                    this.WriteYodaRg("LbLdSel", 0);
                    this.WriteYodaRg("Loopback", 0);
                    FeedbackLog("PHY Loopback configured as ExtCable loopback", FeedbackType.Info);
                    break;
                case LoopBackMode.MacRemote:
                    this.FeedbackLog("GESubsys software reset", FeedbackType.Info);
                    this.WriteYodaRg("GeSftRst", 1);
                    Thread.Sleep(100);
                    this.FeedbackLog("GE PHY enters software reset, stays in software powerdown", FeedbackType.Info);
                    this.WriteYodaRg("GePhySftPdCfg", 1);
                    this.WriteYodaRg("GePhyRst", 1);
                    Thread.Sleep(100);
                    this.FeedbackLog("Apply base settings for UNH-IOL testing", FeedbackType.Info);
                    this.ApplyIOLBaseSettings();
                    this.FeedbackLog("configure for remote loopback,", FeedbackType.Info);
                    this.FeedbackLog("enable remote loopback", FeedbackType.Info);
                    this.WriteYodaRg("LbRemoteEn", 1);
                    this.FeedbackLog("exit software powerdown", FeedbackType.Info);
                    this.WriteYodaRg("SftPd", 0);
                    this.FeedbackLog("Device configured for remote loopback", FeedbackType.Info);
                    break;
                default:
                    throw new NotImplementedException();
            }

            //if (loopback.EnumLoopbackType != LoopBackMode.MacRemote && rxSuppression)
            //{
            //    this.WriteYodaRg("IsolateRx", 1);
            //    FeedbackLog("Rx data suppressed", FeedbackType.Info);
            //}
            //else if (loopback.EnumLoopbackType != LoopBackMode.MacRemote)
            //{
            //    this.WriteYodaRg("IsolateRx", 0);
            //    FeedbackLog("Rx data forwarded to MAC IF", FeedbackType.Info);
            //}
            //else
            //{
            //    // Do nothing/skip
            //}
        }

        public void SetTxSuppression(bool isTxSuppressed)
        {
            if (isTxSuppressed)
                this.WriteYodaRg("LbTxSup", 1);
            else
                this.WriteYodaRg("LbTxSup", 0);
        }

        public void SetRxSuppression(bool isRxSuppressed)
        {
            if (isRxSuppressed)
                this.WriteYodaRg("IsolateRx", 1);
            else
                this.WriteYodaRg("IsolateRx", 0);
        }

        public void SetTestMode(TestModeListingModel testMode, uint framelength)
        {
            FeedbackLog("Subsys software reset", FeedbackType.Info);
            WriteYodaRg("GeSftRst", 1);
            FeedbackLog("PHY enter software reset, stays in software powerdown", FeedbackType.Info);
            WriteYodaRg("GePhySftPdCfg", 1);
            WriteYodaRg("GePhyRst", 1);
            FeedbackLog("Apply base settings for UNH-IOL testing", FeedbackType.Info);
            ApplyIOLBaseSettings();

            switch (testMode.Name1)
            {
                case "100BASE-TX VOD":
                    FeedbackLog("configure for auto-negotiation disable, 100BASE-TX, forced MDI, linking enabled", FeedbackType.Info);
                    WriteYodaRg("AutonegEn", 0);
                    WriteYodaRg("SpeedSelMsb", 0);
                    WriteYodaRg("SpeedSelLsb", 1);
                    WriteYodaRg("AutoMdiEn", 0);
                    WriteYodaRg("ManMdix", 0);
                    WriteYodaRg("LinkEn", 1);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 100BASE-TX VOD measurement", FeedbackType.Info);
                    break;

                case "1000BASE-T Test mode 1":
                    FeedbackLog("GESubsys software reset", FeedbackType.Info);
                    WriteYodaRg("GeSftRst", 1);
                    FeedbackLog("GE PHY enters software reset, stays in software powerdown", FeedbackType.Info);
                    WriteYodaRg("GePhySftPdCfg", 1);
                    WriteYodaRg("GePhyRst", 1);

                    FeedbackLog("Apply base settings for UNH-IOL testing", FeedbackType.Info);
                    ApplyIOLBaseSettings();

                    FeedbackLog(" exit software powerdown, configure for 1000BASE-T test mode 1", FeedbackType.Info);
                    WriteYodaRg("TstMode", 1);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 1000BASE-T test mode 1 measurement", FeedbackType.Info);
                    break;

                case "1000BASE-T Test mode 2":
                    FeedbackLog("GESubsys software reset", FeedbackType.Info);
                    WriteYodaRg("GeSftRst", 1);
                    FeedbackLog("GE PHY enters software reset, stays in software powerdown", FeedbackType.Info);
                    WriteYodaRg("GePhySftPdCfg", 1);
                    WriteYodaRg("GePhyRst", 1);

                    FeedbackLog("Apply base settings for UNH-IOL testing", FeedbackType.Info);
                    ApplyIOLBaseSettings();

                    FeedbackLog(" exit software powerdown, configure for 1000BASE-T test mode 2", FeedbackType.Info);
                    WriteYodaRg("TstMode", 2);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 1000BASE-T test mode 2 measurement", FeedbackType.Info);
                    break;

                case "1000BASE-T Test mode 3":
                    FeedbackLog("GESubsys software reset", FeedbackType.Info);
                    WriteYodaRg("GeSftRst", 1);
                    FeedbackLog("GE PHY enters software reset, stays in software powerdown", FeedbackType.Info);
                    WriteYodaRg("GePhySftPdCfg", 1);
                    WriteYodaRg("GePhyRst", 1);

                    FeedbackLog("Apply base settings for UNH-IOL testing", FeedbackType.Info);
                    ApplyIOLBaseSettings();

                    FeedbackLog(" exit software powerdown, configure for 1000BASE-T test mode 3", FeedbackType.Info);
                    WriteYodaRg("TstMode", 3);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 1000BASE-T test mode 3 measurement", FeedbackType.Info);
                    break;

                case "1000BASE-T Test mode 4":
                    FeedbackLog("GESubsys software reset", FeedbackType.Info);
                    WriteYodaRg("GeSftRst", 1);
                    FeedbackLog("GE PHY enters software reset, stays in software powerdown", FeedbackType.Info);
                    WriteYodaRg("GePhySftPdCfg", 1);
                    WriteYodaRg("GePhyRst", 1);

                    FeedbackLog("Apply base settings for UNH-IOL testing", FeedbackType.Info);
                    ApplyIOLBaseSettings();

                    FeedbackLog(" exit software powerdown, configure for 1000BASE-T test mode 4", FeedbackType.Info);
                    WriteYodaRg("TstMode", 4);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 1000BASE-T test mode 4 measurement", FeedbackType.Info);
                    break;

                case "10BASE-T Link Pulse":
                    FeedbackLog("configure for auto-negotiation disabled, 10BASE-T", FeedbackType.Info);
                    FeedbackLog("forced MDI, loopback enabled, Tx suppression disabled, linking enabled", FeedbackType.Info);
                    WriteYodaRg("AutonegEn", 0);
                    WriteYodaRg("SpeedSelMsb", 0);
                    WriteYodaRg("SpeedSelLsb", 0);
                    WriteYodaRg("AutoMdiEn", 0);
                    WriteYodaRg("ManMdix", 0);
                    WriteYodaRg("LbTxSup", 0);
                    WriteYodaRg("Loopback", 1);
                    WriteYodaRg("LinkEn", 1);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 10BASE-TX forced mode link pulse measurement", FeedbackType.Info);
                    break;

                case "10BASE-T TX Random Frames":
                    FeedbackLog("configure for auto-negotiation disabled, 10BASE-T", FeedbackType.Info);
                    FeedbackLog("forced MDI, loopback enabled, Tx suppression disabled, linking enabled", FeedbackType.Info);
                    WriteYodaRg("AutonegEn", 0);
                    WriteYodaRg("SpeedSelMsb", 0);
                    WriteYodaRg("SpeedSelLsb", 0);
                    WriteYodaRg("AutoMdiEn", 0);
                    WriteYodaRg("ManMdix", 0);
                    WriteYodaRg("LbTxSup", 0);
                    WriteYodaRg("Loopback", 1);
                    WriteYodaRg("LinkEn", 1);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("poll for link up", FeedbackType.Info);
                    PollEqYodaRg("LinkStat", 1, 2.0);
                    FeedbackLog("configure for transmission of frames of length " + framelength + " bytes, random payload", FeedbackType.Info);
                    WriteYodaRg("DiagClkEn", 1);
                    WriteYodaRg("FgFrmLen", framelength);
                    WriteYodaRg("FgContModeEn", 1);
                    WriteYodaRg("FgEn", 1);
                    FeedbackLog("Device configured for 10BASE-T forced mode, with random payload frame transmission", FeedbackType.Info);
                    break;

                case "10BASE-T TX 0xFF Frames":
                    FeedbackLog("configure for auto-negotiation disabled, 10BASE-T", FeedbackType.Info);
                    FeedbackLog("forced MDI, loopback enabled, Tx suppression disabled, linking enabled", FeedbackType.Info);
                    WriteYodaRg("AutonegEn", 0);
                    WriteYodaRg("SpeedSelMsb", 0);
                    WriteYodaRg("SpeedSelLsb", 0);
                    WriteYodaRg("AutoMdiEn", 0);
                    WriteYodaRg("ManMdix", 0);
                    WriteYodaRg("LbTxSup", 0);
                    WriteYodaRg("Loopback", 1);
                    WriteYodaRg("LinkEn", 1);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("poll for link up", FeedbackType.Info);
                    PollEqYodaRg("LinkStat", 1, 2.0);
                    FeedbackLog("configure for transmission of frames of length " + framelength + " bytes, 0xFF repeating payload", FeedbackType.Info);
                    WriteYodaRg("DiagClkEn", 1);
                    WriteYodaRg("FgFrmLen", framelength);
                    WriteYodaRg("FgContModeEn", 1);
                    WriteYodaRg("FgCntrl", 3);
                    WriteYodaRg("FgNoHdr", 1);
                    WriteYodaRg("FgNoFcs", 1);
                    WriteYodaRg("FgEn", 1);
                    FeedbackLog("Device configured for 10BASE-T forced mode, with 0xFF repeating payload frame transmission", FeedbackType.Info);
                    break;

                case "10BASE-T TX 0x00 Frames":
                    FeedbackLog("configure for auto-negotiation disabled, 10BASE-T", FeedbackType.Info);
                    FeedbackLog("forced MDI, loopback enabled, Tx suppression disabled, linking enabled", FeedbackType.Info);
                    WriteYodaRg("AutonegEn", 0);
                    WriteYodaRg("SpeedSelMsb", 0);
                    WriteYodaRg("SpeedSelLsb", 0);
                    WriteYodaRg("AutoMdiEn", 0);
                    WriteYodaRg("ManMdix", 0);
                    WriteYodaRg("LbTxSup", 0);
                    WriteYodaRg("Loopback", 1);
                    WriteYodaRg("LinkEn", 1);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("poll for link up", FeedbackType.Info);
                    PollEqYodaRg("LinkStat", 1, 2.0);
                    FeedbackLog("configure for transmission of frames of length " + framelength + " bytes, 0x00 repeating payload", FeedbackType.Info);
                    WriteYodaRg("DiagClkEn", 1);
                    WriteYodaRg("FgFrmLen", framelength);
                    WriteYodaRg("FgContModeEn", 1);
                    WriteYodaRg("FgCntrl", 1);
                    WriteYodaRg("FgNoHdr", 1);
                    WriteYodaRg("FgNoFcs", 1);
                    WriteYodaRg("FgEn", 1);
                    FeedbackLog("Device configured for 10BASE-T forced mode, with 0x00 repeating payload frame transmission", FeedbackType.Info);
                    break;

                case "10BASE-T TX 5 MHz DIM 1":
                    FeedbackLog("configure for 10BASE-T transmit 5 MHz square wave test mode transmission on dim 1", FeedbackType.Info);
                    WriteYodaRg("B10TxTstMode", 4);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 10BASE-T test mode transmission (5 MHz)", FeedbackType.Info);
                    break;

                case "10BASE-T TX 10 MHz DIM 1":
                    FeedbackLog("configure for 10BASE-T transmit 10 MHz square wave test mode transmission on dim 1", FeedbackType.Info);
                    WriteYodaRg("B10TxTstMode", 2);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 10BASE-T test mode transmission (10 MHz)", FeedbackType.Info);
                    break;

                case "10BASE-T TX 5 MHz DIM 0":
                    FeedbackLog("configure for 10BASE-T transmit 5 MHz square wave test mode transmission on dim 0", FeedbackType.Info);
                    WriteYodaRg("B10TxTstMode", 3);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 10BASE-T test mode transmission (5 MHz)", FeedbackType.Info);
                    break;

                case "10BASE-T TX 10 MHz DIM 0":
                    FeedbackLog("configure for 10BASE-T transmit 10 MHz square wave test mode transmission on dim 0", FeedbackType.Info);
                    WriteYodaRg("B10TxTstMode", 1);
                    FeedbackLog("exit software powerdown", FeedbackType.Info);
                    WriteYodaRg("SftPd", 0);
                    FeedbackLog("Device configured for 10BASE-T test mode transmission (10 MHz)", FeedbackType.Info);
                    break;

                default:
                    throw new NotImplementedException();
            }
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

        protected virtual void OnFrameGenCheckerStatusChanged(string status)
        {
            FrameGenCheckerTextStatusChanged?.Invoke(this, status);
        }

        protected virtual void OnResetFrameGenCheckerStatisticsChanged(string status)
        {
            ResetFrameGenCheckerStatisticsChanged?.Invoke(this, status);
        }

        protected virtual void OnResetFrameGenCheckerErrorStatisticsChanged(string status)
        {
            ResetFrameGenCheckerErrorStatisticsChanged?.Invoke(this, status);
        }

        protected virtual void OnWriteProcessCompleted(FeedbackModel feedback)
        {
            WriteProcessCompleted?.Invoke(this, feedback);
        }

        private void ApplyIOLBaseSettings()
        {
            WriteYodaRg("LnkWdEn", 0);
            FeedbackLog("disable energy detect power-down", FeedbackType.Info);
            WriteYodaRg("NrgPdEn", 0);
            FeedbackLog("disable automatic speed down-shift", FeedbackType.Info);
            WriteYodaRg("DnSpeedTo10En", 0);
            WriteYodaRg("ArbWdEn", 0);
            WriteYodaRg("B10LpTxEn", 0);
            FeedbackLog("disable Energy Efficient Ethernet", FeedbackType.Info);
            WriteYodaRg("EeeAdv", 0);
            FeedbackLog("disable extended next pages", FeedbackType.Info);
            WriteYodaRg("ExtNextPageAdv", 0);
            WriteYodaRg("GeFifoDpth", 0);
            WriteYodaRg("DpthMiiByte", 0);
        }

        private uint ExtractFieldValue(string full_reg_contents, RegisterModel register, string name)
        {
            uint value = 0;
            register.Value = full_reg_contents;

            foreach (var bitfield in register.BitFields)
            {
                if (bitfield.Name == name)
                {
                    value = bitfield.Value;
                }
            }

            return value;
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

            lock (_mainLock)
            {
                uint pageNumber = registerAddress >> 16;
                if (pageNumber == 0)
                {
                    value = MdioReadCl22(registerAddress);
                }
                else
                {
                    value = MdioReadCl45(registerAddress);
                }
            }

            return value;
        }

        private string ReadYodaRg(string name)
        {
            RegisterModel register = null;
            string value = string.Empty;

            lock (_mainLock)
            {
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
            }

            return value;
        }

        private void SetContinuousMode(bool isEnable, uint frameBurst)
        {
            if (isEnable)
            {
                WriteYodaRg("FgContModeEn", 1);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frames will be sent continuously until terminated.", FeedBackType = FeedbackType.Info });
            }
            else
            {
                uint numFramesH = frameBurst / 65536;
                uint numFramesL = frameBurst - (numFramesH * 65536);
                WriteYodaRg("FgNfrmL", numFramesL);
                WriteYodaRg("FgNfrmH", numFramesH);
                WriteYodaRg("FgContModeEn", 0);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Num Frames set to {frameBurst}", FeedBackType = FeedbackType.Info });
            }
        }

        private void SetFrameContent(FrameType frameContent)
        {
            switch (frameContent)
            {
                case FrameType.Random:
                    WriteYodaRg("FgCntrl", 1);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as random", FeedBackType = FeedbackType.Info });
                    break;

                case FrameType.All0s:
                    WriteYodaRg("FgCntrl", 2);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as all zeros", FeedBackType = FeedbackType.Info });
                    break;

                case FrameType.All1s:
                    WriteYodaRg("FgCntrl", 3);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as all ones", FeedBackType = FeedbackType.Info });
                    break;

                case FrameType.Alt10s:
                    WriteYodaRg("FgCntrl", 4);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as alternating 1 0", FeedBackType = FeedbackType.Info });
                    break;

                case FrameType.Decrement:
                    WriteYodaRg("FgCntrl", 5);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as decrementing byte", FeedBackType = FeedbackType.Info });
                    break;

                default:
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type Not Configured - Use one of  Random / A000s / A111s / Alt10", FeedBackType = FeedbackType.Info });
                    break;
            }
        }

        private void SetFrameLength(uint framLength)
        {
            if (framLength <= 0xFFFF)
            {
                WriteYodaRg("FgFrmLen", framLength);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Length set to {framLength}", FeedBackType = FeedbackType.Info });
            }
            else
            {
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Length Not set - value must be less than 65535", FeedBackType = FeedbackType.Info });
            }
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
                else
                {
                    throw new ApplicationException($"No register/bitfield named {name}.");
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

            lock (_mainLock)
            {
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
        }

        private string WriteYodaRg(uint registerAddress, uint value)
        {
            lock (_mainLock)
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

        public void ExecuteSript(ScriptModel script)
        {
            try
            {
                foreach (var register in script.RegisterAccesses)
                {
                    if (register.RegisterName != null)
                    {
                        WriteYodaRg(register.RegisterName, uint.Parse(register.Value));
                        continue;
                    }

                    if (register.RegisterAddress != null)
                    {
                        uint regAddress = uint.Parse(register.RegisterAddress);
                        uint regValue = uint.Parse(register.Value);
                        WriteYodaRg(regAddress, regValue);
                        FeedbackLog($"Register 0x{regAddress.ToString("X")} = {regValue.ToString("X")}", FeedbackType.Verbose);
                        continue;
                    }
                }
            }
            catch (ApplicationException ex)
            {
                FeedbackLog(ex.Message, FeedbackType.Error);
            }
            catch (NullReferenceException)
            {
                FeedbackLog("Script is empty/has invalid register address/input value.", FeedbackType.Error);
            }
        }

        private bool cablediagnosticsRunning;

        public void RunCableDiagnostics(bool enablecrosspairfaultchecking)
        {
            if (enablecrosspairfaultchecking)
            {
                this.FeedbackLog("Cross Pair Checking enabled.", FeedbackType.Info);
                this.WriteYodaRg("CdiagXpairDis", 0);
            }
            else
            {
                this.FeedbackLog("Cross Pair Checking disabled.", FeedbackType.Info);
                this.WriteYodaRg("CdiagXpairDis", 1);
            }

            this.WriteYodaRg("CdiagRun", 1);
            this.FeedbackLog("Running automated cable diagnostics", FeedbackType.Info);
            this.cablediagnosticsRunning = true;
        }

        public void CableDiagnosticsStatus()
        {
            uint cdi_st = uint.Parse(this.ReadYodaRg("CdiagRun"));
            if (this.cablediagnosticsRunning && (cdi_st == 0))
            {
                this.cablediagnosticsRunning = false;
                this.FeedbackLog("Cable Diagnostics have completed", FeedbackType.Info);

                var diagInfoRegisters = new List<string>() { "CdiagRslt0Gd", "CdiagRslt1Gd", "CdiagRslt2Gd", "CdiagRslt3Gd" };
                var pairs = new List<string>() { "0", "1", "2", "3" };

                List<string> cableDiagnosticsStatus = new List<string>();

                int idx = 0;
                foreach (var bitField in diagInfoRegisters)
                {
                    uint bitFieldValue = uint.Parse(this.ReadYodaRg(bitField));

                    if (bitFieldValue == 0x01)
                    {
                        cableDiagnosticsStatus.Add($"Pair{idx} is well terminated.");
                    }
                    idx++;
                }

                uint distance;
                foreach (var pair in pairs)
                {

                    try
                    {
                        distance = uint.Parse(this.ReadYodaRg($"CdiagFltDist{pair}"));
                        if (distance != 0xFF)
                        {
                            cableDiagnosticsStatus.Add(string.Format("Distance to fault on pair {0} is {1} m.", pair, distance));
                        }
                    }
                    catch (ArgumentException e)
                    {
                        /* This register does not exist in this device */
                    }
                }
                GigabitCableDiagCompleted?.Invoke(this, cableDiagnosticsStatus);
            }
        }

        public void SetClk25RefPinControl(string clk25RefPinCtrl)
        {
            switch (clk25RefPinCtrl)
            {
                case "25 MHz Reference":
                    this.WriteYodaRg("GeRefClkEn", 1);
                    _feedbackMessage = "PHY 25 MHz reference clock output on REF_CLK pin";
                    break;
                default:
                    this.WriteYodaRg("GeRefClkEn", 0);
                    _feedbackMessage = "clock output on REF_CLK disabled";
                    break;
            }
        }

        public string AdvertisedSpeed()
        {
            switch (ReadYodaRg("HcdTech"))
            {
                case "0":
                    return "SPEED_10BASE_T_HD";
                case "1":
                    return "SPEED_10BASE_T_FD";
                case "2":
                    return "SPEED_100BASE_TX_HD";
                case "3":
                    return "SPEED_100BASE_TX_FD";
                case "4":
                    return "SPEED_1000BASE_T_HD";
                case "5":
                    return "SPEED_1000BASE_T_FD";
                default:
                    return "-";
            }
        }

        public void SetMasterSlave(string input)
        {
            switch (input)
            {
                case "Prefer_Master":
                    this.WriteYodaRg("ManMstrSlvEnAdv", 0);
                    this.WriteYodaRg("PrefMstrAdv", 1);
                    break;
                case "Prefer_Slave":
                    this.WriteYodaRg("ManMstrSlvEnAdv", 0);
                    this.WriteYodaRg("PrefMstrAdv", 0);
                    break;
                case "Forced_Master":
                    WriteYodaRg("ManMstrSlvEnAdv", 1);
                    this.WriteYodaRg("PrefMstrAdv", 1);
                    break;
                case "Forced_Slave":
                    WriteYodaRg("ManMstrSlvEnAdv", 1);
                    this.WriteYodaRg("PrefMstrAdv", 0);
                    break;
                default:
                    break;
            }
        }
    }
}
