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
using System.Threading;

namespace ADIN.Device.Services
{
    public class ADIN1300FirmwareAPI : IFirmwareAPI
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

        public event EventHandler<FeedbackModel> WriteProcessCompleted;
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

        public void GetFrameCheckerStatus()
        {
            uint fcEn_st = Convert.ToUInt32(ReadYogaRg("FcEn"));
            uint fcTxSel_st = Convert.ToUInt32(ReadYogaRg("FcTxSel"));

            if (fcEn_st == 0)
            {
                OnResetFrameGenCheckerStatisticsChanged("Disabled");
                return;
            }

            uint errCnt = Convert.ToUInt32(ReadYogaRg("RxErrCnt"));
            uint fCntL = Convert.ToUInt32(ReadYogaRg("FcFrmCntL"));
            uint fCntH = Convert.ToUInt32(ReadYogaRg("FcFrmCntH"));
            uint fCnt = (65536 * fCntH) + fCntL;

            if (fCnt == 0)
            {
                OnResetFrameGenCheckerStatisticsChanged("-");
                //return;
            }

            checkedFrames += fCnt;
            checkedFramesErrors += errCnt;

            if (fcTxSel_st == 0)
            {
                OnResetFrameGenCheckerStatisticsChanged($"{checkedFrames} frames, {checkedFramesErrors} errors");
                return;
            }

            OnResetFrameGenCheckerStatisticsChanged($"{checkedFrames} Tx Side with {checkedFramesErrors} errors");
        }

        public string GetFrameGeneratorStatus()
        {
            uint fgEn_st = Convert.ToUInt32(ReadYogaRg("FgEn"), 16);
            //uint fcTxSel_st = Convert.ToUInt32(ReadYodaRg("FC_TX_SEL"), 16);
            uint fgContModeEn_st = Convert.ToUInt32(ReadYogaRg("FgContModeEn"), 16);

            if (fgEn_st == 0)
                return "Not Enabled";

            if (fgContModeEn_st == 1)
            {
                isFrameGenCheckerOngoing = true;
                OnFrameGenCheckerStatusChanged("Terminate");
                return "Frame Transmission in progress";
            }

            uint fgDone_st = Convert.ToUInt32(ReadYogaRg("FgDone"), 16);
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

        public string GetMseValue()
        {
            //if (_boardRev == BoardRevision.Rev0)
            //    return "N/A";

            if (_phyState != EthPhyState.LinkUp)
                return "N/A";

            // Formula:
            // where mse is the value from the register, and sym_pwr_exp is a constant 0.64423.
            // mse_db = 10 * log10((mse / 218) / sym_pwr_exp)
            double mse = Convert.ToUInt32(ReadYogaRg("MseA"), 16);
            double sym_pwr_exp = 0.64423;
            double mse_db = 10 * Math.Log10((mse / Math.Pow(2, 18)) / sym_pwr_exp);

            //OnMseValueChanged(mse_db.ToString("0.00") + " dB");
            return $"{mse_db.ToString("0.00")} dB";
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

        public string GetSpeedMode()
        {
            if (ReadYogaRg("AutonegEn") == "1")
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

            if (this.ReadYogaRg("Fd1000Adv") == "1")
            {
                localSpeeds.Add("SPEED_1000BASE_TX_FD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("Hd1000Adv") == "1")
            {
                localSpeeds.Add("SPEED_1000BASE_TX_HD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("Eee1000Adv") == "1")
            {
                localSpeeds.Add("EEE_1000BASE_T");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("Fd100Adv") == "1")
            {
                localSpeeds.Add("SPEED_100BASE_TX_FD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("Hd100Adv") == "1")
            {
                localSpeeds.Add("SPEED_100BASE_TX_HD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("Eee100Adv") == "1")
            {
                localSpeeds.Add("EEE_100BASE_T");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("Fd10Adv") == "1")
            {
                localSpeeds.Add("SPEED_10BASE_T_FD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("Hd10Adv") == "1")
            {
                localSpeeds.Add("SPEED_10BASE_T_HD_SPEED");
            }
            else
            {
                localSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("EeeAdv") == "1")
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
            lock (_mainLock)
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
                    //throw new ApplicationException(response);
                }

                Debug.WriteLine($"Command:{command.TrimEnd()}");
                Debug.WriteLine($"Response:{response}");

                return response;
            }
        }

        public string MdioReadCl45(uint regAddress)
        {
            lock (_mainLock)
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
                    //throw new ApplicationException(response);
                }

                Debug.WriteLine($"Command:{command.TrimEnd()}");
                Debug.WriteLine($"Response:{response}");

                return response;
            }
        }

        public string MdioWriteCl22(uint regAddress, uint data)
        {
            lock (_mainLock)
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
            lock (_mainLock)
            {
                string response = string.Empty;
                string command = string.Empty;
                string command2 = string.Empty;

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

        public List<string> RemoteAdvertisedSpeedList()
        {
            List<string> remoteSpeeds = new List<string>();

            if (this.ReadYogaRg("LpFd1000Able") == "1")
            {
                remoteSpeeds.Add("SPEED_1000BASE_T_FD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("LpHd1000Able") == "1")
            {
                remoteSpeeds.Add("SPEED_1000BASE_T_HD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("LpEee1000Able") == "1")
            {
                remoteSpeeds.Add("SPEED_1000BASE_EEE_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("LpFd100Able") == "1")
            {
                remoteSpeeds.Add("SPEED_100BASE_TX_FD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("LpHd100Able") == "1")
            {
                remoteSpeeds.Add("SPEED_100BASE_TX_HD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("LpEee100Able") == "1")
            {
                remoteSpeeds.Add("SPEED_100BASE_EEE_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("LpFd10Able") == "1")
            {
                remoteSpeeds.Add("SPEED_10BASE_T_FD_SPEED");
            }
            else
            {
                remoteSpeeds.Add(string.Empty);
            }

            if (this.ReadYogaRg("LpHd10Able") == "1")
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

            OnResetFrameGenCheckerStatisticsChanged($"{checkedFrames} frames, {checkedFramesErrors} errors");
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

            bool fgEn_st = ReadYogaRg("FgEn") == "1" ? true : false;

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

        public void SetLoopbackSetting(LoopbackListingModel loopback)
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
                    if (loopback.TxSuppression)
                    {
                        this.WriteYodaRg("LbTxSup", 1);
                        FeedbackLog("PHY Loopback configured as Digital loopback - Tx suppressed", FeedbackType.Info);
                    }
                    else
                    {
                        this.WriteYodaRg("LbTxSup", 0);
                        FeedbackLog("PHY Loopback configured as Digital loopback - Tx not suppressed", FeedbackType.Info);
                    }
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

            if (loopback.EnumLoopbackType != LoopBackMode.MacRemote && loopback.RxSuppression)
            {
                this.WriteYodaRg("IsolateRx", 1);
                FeedbackLog("Rx data suppressed", FeedbackType.Info);
            }
            else if (loopback.EnumLoopbackType != LoopBackMode.MacRemote)
            {
                this.WriteYodaRg("IsolateRx", 0);
                FeedbackLog("Rx data forwarded to MAC IF", FeedbackType.Info);
            }
            else
            {
                // Do nothing/skip
            }
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
