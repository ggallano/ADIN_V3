using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ADI.Register.Models;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using FTDIChip.Driver.Services;
using System.Diagnostics;
using System.Globalization;
using Helper.RegularExpression;

namespace ADIN.Device.Services
{
    public class ADIN2111FirmwareAPI : IADIN2111API
    {
        private const string EXTRACTNUMBER_REGEX = @"(?<=\=)(\d+\.?\d*)";
        private static ADIN2111FirmwareAPI fwAPI;
        private bool _autoNegotiationStatus = false;
        private decimal _faultDistance;
        private string _feedbackMessage;
        private IFTDIServices _ftdiService;
        private object _mainLock;
        private uint _phyAddress;
        private EthPhyState _phyState;
        private ObservableCollection<RegisterModel> _registers;
        private TestModeType _testmodeState = TestModeType.Normal;
        private uint checkedFrames = 0;
        private uint checkedFramesErrors = 0;
        private uint portNumber = 0;

        public ADIN2111FirmwareAPI(IFTDIServices ftdiService, uint phyAddress, object mainLock)
        {
            _ftdiService = ftdiService;
            _phyAddress = phyAddress;
            _mainLock = mainLock;

            fwAPI = this;
        }

        public ADIN2111FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, uint phyAddress, object mainLock)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
            _mainLock = mainLock;
        }

        public event EventHandler<FeedbackModel> ErrorOccured;

        public event EventHandler<FrameType> FrameContentChanged;

        public event EventHandler<string> FrameGenCheckerTextStatusChanged;

        public event EventHandler<string> LinkLengthChanged;

        public event EventHandler<LoopBackMode> LoopbackChanged;

        public event EventHandler<string> MseValueChanged;

        public event EventHandler<AutoNegMasterSlaveAdvertisementItem> NegotiationMasterSlaveChanged;

        public event EventHandler<PeakVoltageAdvertisementItem> PeakVoltageChanged;

        public event EventHandler<FeedbackModel> ReadProcessCompleted;

        public event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        public event EventHandler<TestModeType> TestModeChanged;

        public event EventHandler<FeedbackModel> WriteProcessCompleted;

        public BoardRevision boardRev { get; set; }

        public bool isFrameGenCheckerOngoing { get; set; }

        public static BoardRevision GetRevNum(uint register)
        {
            var value = fwAPI.ReadYodaRg(Convert.ToUInt32(register));
            var revNum = Convert.ToUInt32(value, 16) & 0x03;

            switch (revNum)
            {
                case 1:
                    return BoardRevision.Rev1;

                case 0:
                    return BoardRevision.Rev0;

                default:
                    return BoardRevision.Rev1;
            }
        }

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

        public List<string> GetCoeff()
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;
                List<string> coeffs = new List<string>();

                command = $"tdrgetcoeff {this.portNumber}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                var res = RegexService.ExtractNumberData(response);
                if (res.Count >= 1)
                {
                    coeffs.Add(res[0].ToString("f6", CultureInfo.InvariantCulture));
                    coeffs.Add(res[1].ToString("f6", CultureInfo.InvariantCulture));
                    coeffs.Add(res[2].ToString("f6", CultureInfo.InvariantCulture));
                }

                if (response == "" || res.Count == 0)
                {
                    //OnErrorOccured(new FeedbackModel() { Message = $"[tdrgetcoeff] {response}", FeedBackType = FeedbackType.Error });
                    throw new ApplicationException(response);
                }

                FeedbackLog($"[tdrgetcoeff] {response}", FeedbackType.Info);
                return coeffs;
            }
        }

        public decimal GetFaultDistance()
        {
            return _faultDistance;
        }

        public void GetFrameCheckerStatus()
        {
            uint fcEn_st = Convert.ToUInt32(ReadYodaRg("FC_EN"));
            uint fcTxSel_st = Convert.ToUInt32(ReadYodaRg("FC_TX_SEL"));

            if (fcEn_st == 0)
            {
                OnResetFrameGenCheckerStatisticsChanged("Disabled");
                return;
            }

            uint errCnt = Convert.ToUInt32(ReadYodaRg("RX_ERR_CNT"));
            uint fCntL = Convert.ToUInt32(ReadYodaRg("FC_FRM_CNT_L"));
            uint fCntH = Convert.ToUInt32(ReadYodaRg("FC_FRM_CNT_H"));
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
            uint fgEn_st = Convert.ToUInt32(ReadYodaRg("FG_EN"), 16);
            //uint fcTxSel_st = Convert.ToUInt32(ReadYodaRg("FC_TX_SEL"), 16);
            uint fgContModeEn_st = Convert.ToUInt32(ReadYodaRg("FG_CONT_MODE_EN"), 16);

            if (fgEn_st == 0)
                return "Not Enabled";

            if (fgContModeEn_st == 1)
            {
                isFrameGenCheckerOngoing = true;
                OnFrameGenCheckerStatusChanged("Terminate");
                return "Frame Transmission in progress";
            }

            uint fgDone_st = Convert.ToUInt32(ReadYodaRg("FG_DONE"), 16);
            if (fgDone_st != 0)
            {
                WriteYodaRg("FG_EN", 0);
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

        public string GetMseValue(BoardRevision boardRev)
        {
            switch (boardRev)
            {
                case BoardRevision.Rev0:
                    return "N/A";
                case BoardRevision.Rev1:
                    // Formula:
                    // where mse is the value from the register, and sym_pwr_exp is a constant 0.64423.
                    // mse_db = 10 * log10((mse / 218) / sym_pwr_exp)
                    double mse = Convert.ToUInt32(ReadYodaRg("MSE_VAL"), 16);
                    double sym_pwr_exp = 0.64423;
                    double mse_db = 10 * Math.Log10((mse / Math.Pow(2, 18)) / sym_pwr_exp);

                    //OnMseValueChanged(mse_db.ToString("0.00") + " dB");
                    return $"{mse_db.ToString("0.00")} dB";
                default:
                    return "N/A";
            }
        }

        public string GetMseValue()
        {
            throw new NotImplementedException();
        }

        public string GetNvp()
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;

                command = $"tdrgetnvp {this.portNumber}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                if (response.Contains("ERROR"))
                {
                    //OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                    throw new ApplicationException(response);
                }

                FeedbackLog($"[tdrgetnvp] {response}", FeedbackType.Info);
                return response;
            }
        }

        public string GetOffset()
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;
                command = $"tdrgetoffset {this.portNumber}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                if (response.Contains("ERROR"))
                {
                    //OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                    throw new ApplicationException(response);
                }

                FeedbackLog($"[tdrgetoffset] {response}", FeedbackType.Info);
                return response;
            }
        }

        public EthPhyState GetPhyState()
        {
            if (ReadYodaRg("CRSM_SFT_PD") == "1")
                return _phyState = EthPhyState.Powerdown;

            if (!(ReadYodaRg("AN_LINK_STATUS") == "1"))
                return _phyState = EthPhyState.LinkDown;

            return _phyState = EthPhyState.LinkUp;
        }

        public uint GetPortNum()
        {
            throw new NotImplementedException();
        }

        public string GetSpeedMode()
        {
            throw new NotImplementedException();
        }

        public string GetTxLevelStatus()
        {
            if (boardRev == BoardRevision.Rev1)
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
                    return $"{1.0.ToString()} Vpk-pk";
                }
                else
                if ((hi_req == "0") && (lp_hi_req == "0"))
                {
                    // Both can manage HI, but neither are requesting it
                    return $"{1.0.ToString()} Vpk-pk";
                }
                else
                {
                    // Both can manage HI, and one or both are requesting it
                    return $"{2.4.ToString()} Vpk-pk";
                }
            }
        }

        public List<string> LocalAdvertisedSpeedList()
        {
            return new List<string>() { "10Base-T1L" };
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

                command = $"phyread {this.portNumber},{regAddress.ToString("X")}\n";

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
            throw new NotImplementedException();
        }

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            lock (_mainLock)
            {
                string response = string.Empty;
                string command = string.Empty;

                command = $"phywrite {this.portNumber},{regAddress.ToString("X")},{data.ToString("X")}\n";

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

        public string PerformCableCalibration(decimal length)
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;

                command = $"tdrcablecal {this.portNumber},{length}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                var res = RegexService.ExtractNumberData(response);
                if (res.Count == 1)
                    response = res[0].ToString();

                if (response == "" || res.Count == 0 || response.Contains("ERROR"))
                {
                    //OnErrorOccured(new FeedbackModel() { Message = $"[tdrcablecal] {response}", FeedBackType = FeedbackType.Error });
                    throw new ApplicationException(response);
                }

                FeedbackLog($"[tdrcablecal] NVP={response}", FeedbackType.Info);
                return response;
            }
        }

        public FaultType PerformFaultDetection()
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;
                string faultMessage = string.Empty;
                FaultType fault = FaultType.None;

                command = $"tdrfaultdet {this.portNumber}\n";
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                var res = RegexService.ExtractNumberData(response);
                if (res.Count == 1)
                {
                    _faultDistance = res[0];
                    var faultResult = RegexService.ExtractFaultType(response);
                    if (faultResult == "open")
                    {
                        fault = FaultType.Open;
                    }
                    else
                    {
                        fault = FaultType.Short;
                    }
                    faultMessage = $"[tdrfaultdet] Fault = {fault.ToString()} : Fault Distance = {_faultDistance}";
                }
                else
                {
                    fault = FaultType.None;
                    faultMessage = $"[tdrfaultdet] Fault = {fault.ToString()}";
                }

                OnWriteProcessCompleted(new FeedbackModel() { Message = faultMessage, FeedBackType = FeedbackType.Info });
                return fault;
            }
        }

        public string PerformOffsetCalibration()
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;

                command = $"tdroffsetcal {this.portNumber}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                var res = RegexService.ExtractNumberData(response);
                if (res.Count == 1)
                    response = res[0].ToString();

                if (response == "" || res.Count == 0 || response.Contains("ERROR"))
                {
                    //OnErrorOccured(new FeedbackModel() { Message = $"[tdroffsetcal] {response}", FeedBackType = FeedbackType.Error });
                    throw new ApplicationException(response);
                }

                FeedbackLog($"[tdroffsetcal] Offset={response}", FeedbackType.Info);
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
            checkedFrames = 0;
            checkedFramesErrors = 0;

            OnResetFrameGenCheckerStatisticsChanged($"{checkedFrames} frames, {checkedFramesErrors} errors");
        }

        public void ResetPhy(ResetType reset)
        {
            switch (reset)
            {
                case ResetType.SubSys:
                    WriteYodaRg("CRSM_PHY_SUBSYS_RST", 1);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "SubSys reset", FeedBackType = FeedbackType.Info });
                    break;

                case ResetType.Phy:
                    WriteYodaRg("CRSM_SFT_RST", 1);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Phy reset", FeedBackType = FeedbackType.Info });
                    break;

                default:
                    break;
            }
        }

        public void RestartAutoNegotiation()
        {
            WriteYodaRg("AN_RESTART", 1);
            OnWriteProcessCompleted(new FeedbackModel() { Message = "Restart auto negotiation", FeedBackType = FeedbackType.Info });
            Debug.WriteLine("Restart Auto Negotiation");
        }

        public List<string> SetCoeff(decimal nvp, decimal coeff0, decimal coeffi)
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;
                List<string> coeffs = new List<string>();

                command = $"tdrsetcoeff {this.portNumber},{nvp},{coeff0},{coeffi}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                var res = RegexService.ExtractNumberData(response, EXTRACTNUMBER_REGEX);
                if (res.Count >= 1)
                {
                    coeffs.Add(res[0].ToString("f6", CultureInfo.InvariantCulture));
                    coeffs.Add(res[1].ToString("f6", CultureInfo.InvariantCulture));
                    coeffs.Add(res[2].ToString("f6", CultureInfo.InvariantCulture));
                }

                if (response == "" || res.Count == 0)
                {
                    //OnErrorOccured(new FeedbackModel() { Message = $"[Cable Calibration] {response}", FeedBackType = FeedbackType.Error });
                    throw new ApplicationException($"[Cable Calibration] {response}");
                }

                var res1 = RegexService.ExtractNVP(response);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"[Cable Calibration] {res1}", FeedBackType = FeedbackType.Info });
                return coeffs;
            }
        }

        public void SetFrameCheckerSetting(FrameGenCheckerModel frameContent)
        {
            checkedFrames = 0;
            checkedFramesErrors = 0;

            bool fgEn_st = ReadYodaRg("FG_EN") == "1" ? true : false;

            if (fgEn_st)
            {
                WriteYodaRg("FG_EN", 0);
                //OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Generator disabled", FeedBackType = FeedbackType.Info });
                isFrameGenCheckerOngoing = false;
                OnFrameGenCheckerStatusChanged("Generate");
            }
            else
            {
                WriteYodaRg("CRSM_FRM_GEN_DIAG_CLK_EN", 1);
                SetFrameLength(frameContent.FrameLength);
                SetContinuousMode(frameContent.EnableContinuousMode, frameContent.FrameBurst);
                SetFrameContent(frameContent.SelectedFrameContent);
                SetMacAddresses(frameContent.EnableMacAddress, frameContent.SrcOctet, frameContent.DestOctet);

                WriteYodaRg("FG_EN", 1);
                isFrameGenCheckerOngoing = true;
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"- Started transmission of {frameContent.FrameBurst} frames -", FeedBackType = FeedbackType.Info });
            }
        }

        public void SetLoopbackSetting(LoopbackModel loopback, bool txSuppression, bool rxSuppression)
        {
            switch (loopback.EnumLoopbackType)
            {
                // PCS
                case LoopBackMode.Digital:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 0);
                    WriteYodaRg("AN_FRC_MODE_EN", 1);
                    WriteYodaRg("B10L_LB_PMA_LOC_EN", 0);
                    WriteYodaRg("B10L_LB_PCS_EN", 1);
                    WriteYodaRg("MAC_IF_LB_EN", 0);
                    WriteYodaRg("MAC_IF_REM_LB_EN", 0);
                    WriteYodaRg("RMII_TXD_CHK_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    FeedbackLog($"Loopback Mode: {loopback.Name} Loopback Mode", FeedbackType.Info);
                    break;

                //PMA
                case LoopBackMode.LineDriver:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 0);
                    WriteYodaRg("AN_FRC_MODE_EN", 1);
                    WriteYodaRg("B10L_LB_PMA_LOC_EN", 1);
                    WriteYodaRg("B10L_LB_PCS_EN", 0);
                    WriteYodaRg("MAC_IF_LB_EN", 0);
                    WriteYodaRg("MAC_IF_REM_LB_EN", 0);
                    WriteYodaRg("RMII_TXD_CHK_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    FeedbackLog($"Loopback Mode: {loopback.Name} Loopback Mode", FeedbackType.Info);
                    break;

                //ExtMII,RMII
                case LoopBackMode.ExtCable:
                    WriteYodaRg("CRSM_SFT_PD", 1);

                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("AN_FRC_MODE_EN", 0);

                    WriteYodaRg("B10L_LB_PMA_LOC_EN", 0);
                    WriteYodaRg("B10L_LB_PCS_EN", 0);
                    WriteYodaRg("MAC_IF_LB_EN", 0);
                    WriteYodaRg("MAC_IF_REM_LB_EN", 0);
                    WriteYodaRg("RMII_TXD_CHK_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    FeedbackLog($"Loopback Mode: {loopback.Name} Loopback Mode", FeedbackType.Info);
                    break;

                //MAC IF Remote
                case LoopBackMode.MacRemote:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("AN_FRC_MODE_EN", 0);
                    WriteYodaRg("B10L_LB_PMA_LOC_EN", 0);
                    WriteYodaRg("B10L_LB_PCS_EN", 0);
                    WriteYodaRg("MAC_IF_LB_EN", 0);
                    WriteYodaRg("MAC_IF_REM_LB_EN", 1);
                    WriteYodaRg("RMII_TXD_CHK_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    FeedbackLog($"Loopback Mode: {loopback.Name} Loopback Mode", FeedbackType.Info);
                    break;

                case LoopBackMode.MAC:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("AN_FRC_MODE_EN", 0);
                    WriteYodaRg("B10L_LB_PMA_LOC_EN", 0);
                    WriteYodaRg("B10L_LB_PCS_EN", 0);
                    WriteYodaRg("MAC_IF_LB_EN", 1);
                    WriteYodaRg("MAC_IF_REM_LB_EN", 0);
                    WriteYodaRg("RMII_TXD_CHK_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    FeedbackLog($"Loopback Mode: {loopback.Name} Loopback Mode", FeedbackType.Info);
                    break;

                case LoopBackMode.OFF:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("AN_FRC_MODE_EN", 0);
                    WriteYodaRg("B10L_LB_PMA_LOC_EN", 0);
                    WriteYodaRg("B10L_LB_PCS_EN", 0);
                    WriteYodaRg("MAC_IF_LB_EN", 0);
                    WriteYodaRg("MAC_IF_REM_LB_EN", 0);
                    WriteYodaRg("RMII_TXD_CHK_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    FeedbackLog($"Loopback Mode: {loopback.Name} Loopback Mode", FeedbackType.Info);
                    break;

                default:
                    //this.Info("    SPE PHY Loopback NOT configured - use one of PMA / PCS / MAC Interface / MAC Interface Remote / External MII/RMII");
                    break;
            }

            WriteYodaRg("MAC_IF_LB_TX_SUP_EN", txSuppression ? (uint)1 : (uint)0);
            WriteYodaRg("MAC_IF_REM_LB_RX_SUP_EN", rxSuppression ? (uint)1 : (uint)0);
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

        public void SetMode(CalibrationMode mode)
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;

                command = $"tdrsetmode {this.portNumber},{(int)mode}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                var res = RegexService.ExtractNumberData(response);
                if (res.Count == 1)
                    response = res[0].ToString();

                if (response == "" || res.Count == 0)
                {
                    //OnErrorOccured(new FeedbackModel() { Message = $"[tdrsetmode] {response}", FeedBackType = FeedbackType.Error });
                    throw new ApplicationException(response);
                }

                //OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrsetmode] {response}", FeedBackType = FeedbackType.Info });
            }
        }

        public List<string> SetNvp(decimal nvpValue)
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;
                List<string> resList = new List<string>();
                command = $"tdrsetnvp {this.portNumber},{nvpValue.ToString(CultureInfo.InvariantCulture)}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                var res = RegexService.ExtractNumberData(response);
                if (res.Count >= 1)
                {
                    resList.Add(res[0].ToString(CultureInfo.InvariantCulture));
                    resList.Add(res[1].ToString(CultureInfo.InvariantCulture));
                }

                if (response == "" || res.Count == 0 || response.Contains("ERROR"))
                {
                    //OnErrorOccured(new FeedbackModel() { Message = $"[tdrsetnvp] {response}", FeedBackType = FeedbackType.Error });
                    throw new ApplicationException(response);
                }

                FeedbackLog($"[tdrsetnvp] NVP={res[0]}, Mode={res[1]}", FeedbackType.Info);
                return resList;
            }
        }

        public string SetOffset(decimal offset)
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;

                command = $"tdrsetoffset {this.portNumber},{offset.ToString(CultureInfo.InvariantCulture)}\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();

                var res = RegexService.ExtractNumberData(response);
                if (res.Count == 1)
                    response = res[0].ToString(CultureInfo.InvariantCulture);

                if (response == "" || res.Count == 0 || response.Contains("ERROR"))
                {
                    //OnErrorOccured(new FeedbackModel() { Message = $"[tdrsetoffset] {response}", FeedBackType = FeedbackType.Error });
                    throw new ApplicationException("[Offset Calibration]" + response);
                }

                FeedbackLog($"[Offset Calibration] Offset={response}", FeedbackType.Info);
                return response;
            }
        }

        public void SetPortNum(uint portNum)
        {
            this.portNumber = portNum;
        }

        public void SetTestMode(TestModeListingModel testMode, uint framelength)
        {
            switch (testMode.Name1)
            {
                case "10BASE-T1L Normal Mode":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("B10L_TX_TEST_MODE", 0);
                    WriteYodaRg("B10L_TX_DIS_MODE_EN", 0);
                    WriteYodaRg("AN_FRC_MODE_EN", 0);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _testmodeState = TestModeType.Normal;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Test Mode: 10BASE-T1L Normal Mode", FeedBackType = FeedbackType.Info });
                    Debug.WriteLine($"Test Mode Selected, {testMode.Name1}:{testMode.Name2}");
                    break;

                case "10BASE-T1L Test Mode 1:":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 0);
                    WriteYodaRg("AN_FRC_MODE_EN", 1);
                    WriteYodaRg("B10L_TX_TEST_MODE", 1);
                    WriteYodaRg("B10L_TX_DIS_MODE_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _testmodeState = TestModeType.Test1;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Test Mode: 10BASE-T1L Test Mode 1", FeedBackType = FeedbackType.Info });
                    Debug.WriteLine($"Test Mode Selected, {testMode.Name1}:{testMode.Name2}");
                    break;

                case "10BASE-T1L Test Mode 2:":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 0);
                    WriteYodaRg("AN_FRC_MODE_EN", 1);
                    WriteYodaRg("B10L_TX_TEST_MODE", 2);
                    WriteYodaRg("B10L_TX_DIS_MODE_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _testmodeState = TestModeType.Test2;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Test Mode: 10BASE-T1L Test Mode 2", FeedBackType = FeedbackType.Info });
                    Debug.WriteLine($"Test Mode Selected, {testMode.Name1} : {testMode.Name2}");
                    break;

                case "10BASE-T1L Test Mode 3:":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 0);
                    WriteYodaRg("AN_FRC_MODE_EN", 1);
                    WriteYodaRg("B10L_TX_TEST_MODE", 3);
                    WriteYodaRg("B10L_TX_DIS_MODE_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _testmodeState = TestModeType.Test3;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Test Mode: 10BASE-T1L Test Mode 3", FeedBackType = FeedbackType.Info });
                    Debug.WriteLine($"Test Mode Selected, {testMode.Name1} : {testMode.Name2}");
                    break;

                case "10BASE-T1L Transmit Disable:":
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_EN", 0);
                    WriteYodaRg("AN_FRC_MODE_EN", 1);
                    WriteYodaRg("B10L_TX_TEST_MODE", 0);
                    WriteYodaRg("B10L_TX_DIS_MODE_EN", 0);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _testmodeState = TestModeType.Transmit;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Test Mode: 10BASE-T1L Transmit Disable", FeedBackType = FeedbackType.Info });
                    Debug.WriteLine($"Test Mode Selected, {testMode.Name1} : {testMode.Name2}");
                    break;

                default:
                    break;
            }
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
            if (isSoftwarePowerdown)
                WriteYodaRg("CRSM_SFT_PD", 1);
            else
                WriteYodaRg("CRSM_SFT_PD", 0);
        }

        public void TDRInit()
        {
            lock (_mainLock)
            {
                string command = string.Empty;
                string response = string.Empty;
                try
                {
                    command = $"tdrinit {this.portNumber}\n";

                    _ftdiService.Purge();
                    _ftdiService.SendData(command);
                    response = _ftdiService.ReadCommandResponse().Trim();

                    if (response.Contains("ERROR"))
                        throw new ApplicationException(response);

                    FeedbackLog($"[tdrinit] TDR Initialized={response}", FeedbackType.Info);
                }
                catch (ApplicationException ex)
                {
                    //OnErrorOccured(new FeedbackModel() { Message = ex.Message, FeedBackType = FeedbackType.Error });
                }
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

        private void FeedbackLog(string message, FeedbackType feedbackType)
        {
            FeedbackModel feedback = new FeedbackModel();
            feedback.Message = "Port" + this.portNumber.ToString() + " " + message;
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

        private void SetContinuousMode(bool isEnable, uint frameBurst)
        {
            if (isEnable)
            {
                WriteYodaRg("FG_CONT_MODE_EN", 1);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frames will be sent continuously until terminated.", FeedBackType = FeedbackType.Info });
            }
            else
            {
                uint numFramesH = frameBurst / 65536;
                uint numFramesL = frameBurst - (numFramesH * 65536);
                WriteYodaRg("FG_NFRM_L", numFramesL);
                WriteYodaRg("FG_NFRM_H", numFramesH);
                WriteYodaRg("FG_CONT_MODE_EN", 0);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Num Frames set to {frameBurst}", FeedBackType = FeedbackType.Info });
            }
        }

        private void SetFrameContent(FrameType frameContent)
        {
            switch (frameContent)
            {
                case FrameType.Random:
                    WriteYodaRg("FG_CNTRL", 1);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as random", FeedBackType = FeedbackType.Info });
                    break;

                case FrameType.All0s:
                    WriteYodaRg("FG_CNTRL", 2);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as all zeros", FeedBackType = FeedbackType.Info });
                    break;

                case FrameType.All1s:
                    WriteYodaRg("FG_CNTRL", 3);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as all ones", FeedBackType = FeedbackType.Info });
                    break;

                case FrameType.Alt10s:
                    WriteYodaRg("FG_CNTRL", 4);
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Type configured as alternating 1 0", FeedBackType = FeedbackType.Info });
                    break;

                case FrameType.Decrement:
                    WriteYodaRg("FG_CNTRL", 5);
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
                WriteYodaRg("FG_FRM_LEN", framLength);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Length set to {framLength}", FeedBackType = FeedbackType.Info });
            }
            else
            {
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Frame Length Not set - value must be less than 65535", FeedBackType = FeedbackType.Info });
            }
        }

        private void SetMacAddresses(bool isEnable, string src, string dest)
        {
            if (isEnable)
            {
                if (src != null)
                {
                    WriteYodaRg("FgSa", Convert.ToUInt32(src, 16));
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Source MAC Address set to 0x{src}", FeedBackType = FeedbackType.Info });
                }

                if (dest != null)
                {
                    WriteYodaRg("FgDa5Emi", Convert.ToUInt32(dest, 16));
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"Destination MAC Address set to 0x{dest}", FeedBackType = FeedbackType.Info });
                }
            }
            else
            {
                WriteYodaRg("FgSa", 0xE1);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Source MAC Address set to 0xE1", FeedBackType = FeedbackType.Info });
                WriteYodaRg("FgDa5Emi", 0x01);
                OnWriteProcessCompleted(new FeedbackModel() { Message = $"Source MAC Address set to 0x01", FeedBackType = FeedbackType.Info });
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
    }
}
