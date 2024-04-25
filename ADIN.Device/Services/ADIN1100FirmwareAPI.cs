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
    public class ADIN1100FirmwareAPI : IFirmwareAPI, ICableDiagnostic
    {
        private const string EXTRACTNUMBER_REGEX = @"(?<=\=)(\d+\.?\d*)";
        private AutoNegMasterSlaveAdvertisementItem _autoNegMasterSlave;
        private bool _autoNegotiationStatus;
        private BoardRevision _boardRev;
        private BoardType _boardType;
        private decimal _faultDistance;
        private IFTDIServices _ftdiService;
        private LoopBackMode _loopbackState = LoopBackMode.OFF;
        private double _nvp;
        private uint _phyAddress;
        private EthPhyState _phyState;
        private ObservableCollection<RegisterModel> _registers;
        private TestModeType _testmodeState = TestModeType.Normal;
        private PeakVoltageAdvertisementItem _txLevel;
        private uint checkedFrames = 0;
        private uint checkedFramesErrors = 0;
        private object thisLock = new object();

        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="ftdiService"></param>
        public ADIN1100FirmwareAPI(IFTDIServices ftdiService)
        {
            _ftdiService = ftdiService;
        }

        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="ftdiService"></param>
        /// <param name="phyAddress"></param>
        /// <param name="registers"></param>
        /// <param name="boardRev"></param>
        /// <param name="boardType"></param>
        public ADIN1100FirmwareAPI(IFTDIServices ftdiService, uint phyAddress, ObservableCollection<RegisterModel> registers, BoardRevision boardRev, BoardType boardType)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
            _boardRev = boardRev;
            _boardType = boardType;
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

        public bool isFrameGenCheckerOngoing { get; set; } = false;

        public double actLengthEstimation(List<double> echo, int offset, double th, int width_th_low, int width_th_high, int sR)
        {
            var nvpValue = float.Parse(GetCoeffActive(), CultureInfo.InvariantCulture);
            double c = 299792458;
            bool mov_avg = true;
            int rem_idx = 0;

            List<PeakModel> localMaxUnion = new List<PeakModel>();
            List<PeakModel> localMinUnion = new List<PeakModel>();
            LocalRangeModel shortRange = new LocalRangeModel();
            LocalRangeModel longRange = new LocalRangeModel();

            //Short range max & mins: high threshold to cover peaks close to the local PHY
            shortRange = localMax_3(echo, th = 0.005, width_th_low, width_th_high, mov_avg);
            //Long range maximums: reduced threhold limit by 10x to cover smaller peaks along the cable
            longRange = localMax_3(echo, th = 0.001, width_th_low, width_th_high, mov_avg);

            //combine the two list
            localMaxUnion = shortRange.LocalMaxRange.Cast<PeakModel>().Concat(longRange.LocalMaxRange).ToList();
            //filtered the list (removed duplicate)
            List<PeakModel> local_maxs = new List<PeakModel>();
            local_maxs = localMaxUnion.GroupBy(s => s.idx).Select(s => s.First()).ToList();

            //combine the two list
            localMinUnion = shortRange.LocalMinRange.Cast<PeakModel>().Concat(longRange.LocalMinRange).ToList();
            //filtered the list (removed duplicate)
            List<PeakModel> local_mins = new List<PeakModel>();
            local_mins = localMinUnion.GroupBy(s => s.idx).Select(s => s.First()).ToList();

            //Sort both lists in ascending order. Items are sorted by their index number
            local_maxs.Sort((a, b) => a.idx.CompareTo(b.idx));
            local_mins.Sort((a, b) => a.idx.CompareTo(b.idx));

            var num_maxs = local_maxs.Count;
            var num_mins = local_mins.Count;

            //No mins nor maxs, then no valid echo
            if ((num_maxs == 0) && (num_mins == 0))
            {
                rem_idx = -1;
                offset = -1;
            }
            //Only one min, no maxs: possible cable length beyong range: no remote reflection detected
            else if ((num_mins == 1) && (num_maxs == 0))
            {
                if (local_mins[0].idx < 7)
                {
                    rem_idx = -1;
                    offset = -1;
                }
                else
                {
                    rem_idx = local_mins[0].idx;
                    offset = 5;
                }
            }
            //Only one min, no maxs: possible cable length beyong range: no remote reflection detected
            else if ((num_mins == 0) && (num_maxs == 1))
            {
                if (local_maxs[0].idx < 7)
                {
                    rem_idx = -1;
                    offset = -1;
                }
                else
                {
                    rem_idx = local_maxs[0].idx;
                    offset = 5;
                }
            }
            else if ((num_mins == 0) || (num_maxs == 0))
            {
                rem_idx = -1;
                offset = -1;
            }
            else
            {
                //Offset Estimation
                if (sR == 15)
                {
                    if (local_mins[0].idx < local_maxs[0].idx)
                        offset = local_mins[0].idx;
                    else
                        offset = local_maxs[0].idx;

                    // if the first peak is not detected properly, use default value offset = 5
                    if (offset > 7 || offset < 4)
                        offset = 5;
                }

                //ONLY 1 min and 1 Max
                if (num_maxs == 1 && num_mins == 1)
                {
                    offset = 5;
                    if (local_mins[0].idx <= local_maxs[0].idx)
                        rem_idx = local_maxs[0].idx;
                    else
                        rem_idx = local_mins[0].idx;
                }
                // More than 1 min and 1 max
                else
                {
                    if (local_maxs[num_maxs - 1].idx > local_mins[num_mins - 1].idx)
                    {
                        if (local_maxs[num_maxs - 1].StartEdge - 1 <= local_mins[num_mins - 1].EndEdge)
                            rem_idx = local_mins[num_mins - 1].idx;
                        else
                            rem_idx = local_maxs[num_maxs - 1].idx;
                    }
                    else
                    {
                        if (local_maxs[num_maxs - 1].EndEdge >= local_mins[num_mins - 1].StartEdge - 1)
                            rem_idx = local_maxs[num_maxs - 1].idx;
                        else
                            rem_idx = local_mins[num_mins - 1].idx;
                    }
                }
            }

            double cable_length = ((rem_idx - offset) * c * nvpValue) / (sR * 2e6);

            Debug.WriteLine($"Index = {rem_idx}");
            Debug.WriteLine($"Cable Length in Active Link = {cable_length}");

            return cable_length;
        }

        public void ExecuteSript(ScriptModel script)
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
                    WriteYodaRg(uint.Parse(register.RegisterAddress), uint.Parse(register.Value));
                    OnWriteProcessCompleted(new FeedbackModel() { Message = $"[{_ftdiService.GetSerialNumber()}] [Write] Address: 0x{uint.Parse(register.RegisterAddress).ToString("X")}, Value: {uint.Parse(register.Value).ToString("X")}", FeedBackType = FeedbackType.Info });
                    continue;
                }
            }
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
            string command = string.Empty;
            string response = string.Empty;
            List<string> coeffs = new List<string>();

            command = "tdrgetcoeff\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            var res = RegexService.ExtractNumberData(response);
            if (res.Count >= 1)
            {
                coeffs.Add(res[0].ToString("f6", CultureInfo.InvariantCulture));
                coeffs.Add(res[1].ToString("f6", CultureInfo.InvariantCulture));
                coeffs.Add(res[2].ToString("f6", CultureInfo.InvariantCulture));
            }

            if (response == "" || res.Count == 0)
            {
                OnErrorOccured(new FeedbackModel() { Message = $"[tdrgetcoeff] {response}", FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrgetcoeff] {response}", FeedBackType = FeedbackType.Info });
            return coeffs;
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

        public FrameType GetFrameContentInitialization(bool eventTrigger = false)
        {
            var FG_CNTRL = ReadYodaRg("FG_CNTRL");
            FrameType result = FrameType.Random;

            switch (FG_CNTRL)
            {
                case "1":
                    result = FrameType.Random;
                    break;

                case "2":
                    result = FrameType.All0s;
                    break;

                case "3":
                    result = FrameType.All1s;
                    break;

                case "4":
                    result = FrameType.Alt10s;
                    break;

                case "5":
                    result = FrameType.Decrement;
                    break;

                default:
                    result = FrameType.Random;
                    break;
            }

            if (eventTrigger)
                OnFrameContentChanged(result);

            return result;
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

        public string GetLinkLength()
        {
            string linkLength = string.Empty;
            List<double> echos = new List<double>();

            var offset = 5;
            var th = 0.001;
            var width_th_low = 1;
            var width_th_high = 30;
            var SR = 15;

            if (GetPhyState() == EthPhyState.LinkUp)
            {
                echos = acquireCoeffSerial();
                linkLength = Math.Round(actLengthEstimation(echos, offset, th, width_th_low, width_th_high, SR), 0).ToString();
            }
            else
                linkLength = "--";

            return linkLength;
        }

        public string GetLinkStatus()
        {
            return GetPhyState().ToString();
        }

        public LoopBackMode GetLoopbackInitialization(bool eventTrigger = false)
        {
            var AN_EN = ReadYodaRg("AN_EN") == "1" ? true : false;
            var AN_FRC_MODE_EN = ReadYodaRg("AN_FRC_MODE_EN") == "1" ? true : false;
            var B10L_LB_PMA_LOC_EN = ReadYodaRg("B10L_LB_PMA_LOC_EN") == "1" ? true : false;
            var B10L_LB_PCS_EN = ReadYodaRg("B10L_LB_PCS_EN") == "1" ? true : false;
            var MAC_IF_LB_EN = ReadYodaRg("MAC_IF_LB_EN") == "1" ? true : false;
            var MAC_IF_REM_LB_EN = ReadYodaRg("MAC_IF_REM_LB_EN") == "1" ? true : false;
            var RMII_TXD_CHK_EN = ReadYodaRg("RMII_TXD_CHK_EN") == "1" ? true : false;

            LoopBackMode result = LoopBackMode.OFF;

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = LoopBackMode.Digital;

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = LoopBackMode.LineDriver;

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (RMII_TXD_CHK_EN)
                                        result = LoopBackMode.ExtCable;

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = LoopBackMode.MacRemote;

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = LoopBackMode.MAC;

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (!B10L_LB_PMA_LOC_EN)
                        if (!B10L_LB_PCS_EN)
                            if (!MAC_IF_LB_EN)
                                if (!MAC_IF_REM_LB_EN)
                                    if (!RMII_TXD_CHK_EN)
                                        result = LoopBackMode.OFF;

            if (eventTrigger)
                OnLoopbackChanged(result);

            Debug.WriteLine($"[GetLoopback] {result.ToString()}");
            return result;
        }

        public LoopBackMode GetLoopbackState()
        {
            return _loopbackState;
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

        public BoardType GetModelNum(out uint phyAddress)
        {
            phyAddress = 0;
            
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    var value = MdioReadCl45(Convert.ToUInt32(0x1E0003));
                    if (value == "ERROR")
                        continue;

                    var modelNum = (Convert.ToUInt32(value, 16) & 0x3F0) >> 4;
                    switch (modelNum)
                    {
                        case 0x2:
                        case 0x8: // ADIN1100/ADIN1101
                        case 0x9: // ADIN1110/ADIN1111
                        case 0xA: // ADIN2111
                            _boardType = BoardType.ADIN1100;
                            break;

                        default:
                            _boardType = BoardType.ADIN1100;
                            break;
                    }
                    break;
                }
                catch (ApplicationException ex)
                {
                }
                phyAddress = _phyAddress = (uint)i;
            }
            return _boardType;
        }

        public string GetMseValue()
        {
            if (_boardRev == BoardRevision.Rev0)
                return "N/A";

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

        public AutoNegMasterSlaveAdvertisementItem GetNegotiationMasterSlaveInitialization(bool eventTrigger = false)
        {
            bool AN_EN = ReadYodaRg("AN_EN") == "1" ? true : false;
            bool AN_ADV_MST = ReadYodaRg("AN_ADV_MST") == "1" ? true : false;
            bool AN_ADV_FORCE_MS = ReadYodaRg("AN_ADV_FORCE_MS") == "1" ? true : false;

            AutoNegMasterSlaveAdvertisementItem result = AutoNegMasterSlaveAdvertisementItem.Prefer_Slave;

            if (AN_EN)
            {
                if (AN_ADV_MST)
                {
                    if (AN_ADV_FORCE_MS)
                        result = AutoNegMasterSlaveAdvertisementItem.Forced_Master;
                    else
                        result = AutoNegMasterSlaveAdvertisementItem.Prefer_Master;
                }
                else
                {
                    if (AN_ADV_FORCE_MS)
                        result = AutoNegMasterSlaveAdvertisementItem.Forced_Slave;
                    else
                        result = AutoNegMasterSlaveAdvertisementItem.Prefer_Slave;
                }
            }
            else
            {
                result = AutoNegMasterSlaveAdvertisementItem.Prefer_Slave;
            }

            if (eventTrigger)
                OnNegotiationMasterSlaveChanged(result);

            Debug.WriteLine($"[GetNegotiationMasterSlaveSetting] {result.ToString()}");
            return result;
        }

        public string GetNvp()
        {
            string command = string.Empty;
            string response = string.Empty;

            command = "tdrgetnvp\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            if (response.Contains("ERROR"))
            {
                OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrgetnvp] {response}", FeedBackType = FeedbackType.Info });
            return response;
        }

        public string GetOffset()
        {
            string command = string.Empty;
            string response = string.Empty;
            command = "tdrgetoffset\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            if (response.Contains("ERROR"))
            {
                OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrgetoffset] {response}", FeedBackType = FeedbackType.Info });
            return response;
        }

        public PeakVoltageAdvertisementItem GetPeakVoltageInitialization(bool eventTrigger)
        {
            bool AN_ADV_B10L_TX_LVL_HI_ABL = ReadYodaRg("AN_ADV_B10L_TX_LVL_HI_ABL") == "1" ? true : false;
            PeakVoltageAdvertisementItem result = PeakVoltageAdvertisementItem.Capable2p4Volts_Requested2p4Volts;

            if (AN_ADV_B10L_TX_LVL_HI_ABL)
                result = PeakVoltageAdvertisementItem.Capable2p4Volts_Requested2p4Volts;
            else
                result = PeakVoltageAdvertisementItem.Capable1Volt;

            if (eventTrigger)
                OnPeakVoltageChanged(result);

            Debug.WriteLine($"[GetPeakVoltage] {result.ToString()}");
            return result;
        }

        public EthPhyState GetPhyState()
        {
            if (ReadYodaRg("CRSM_SFT_PD") == "1")
                return _phyState = EthPhyState.Powerdown;

            if (!(ReadYodaRg("AN_LINK_STATUS") == "1"))
                return _phyState = EthPhyState.LinkDown;

            return _phyState = EthPhyState.LinkUp;
        }

        public RegisterModel GetRegister(string name)
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

        public string GetRegisterJsonFile(BoardRevision revNum)
        {
            string jsonFile = string.Empty;

            switch (revNum)
            {
                case BoardRevision.Rev0:
                    jsonFile = "registers_adin1100_S1.json";
                    break;

                case BoardRevision.Rev1:
                    jsonFile = "registers_adin1100_S2.json";
                    break;

                default:
                    break;
            }

            return jsonFile;
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

        public ObservableCollection<RegisterModel> GetStatusRegisters(/*uint phyAddress*/)
        {
            foreach (var register in _registers)
            {
                register.Value = MdioReadCl45(register.Address);
            }
            return _registers;
        }

        public TestModeType GetTestModeInitialization(bool eventTrigger = false)
        {
            var B10L_TX_TEST_MODE = ReadYodaRg("B10L_TX_TEST_MODE");
            var B10L_TX_DIS_MODE_EN = ReadYodaRg("B10L_TX_DIS_MODE_EN") == "1" ? true : false;
            var AN_FRC_MODE_EN = ReadYodaRg("AN_FRC_MODE_EN") == "1" ? true : false;
            var AN_EN = ReadYodaRg("AN_EN") == "1" ? true : false;

            TestModeType result = TestModeType.Normal;

            if (AN_EN)
                if (!AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "0")
                        if (!B10L_TX_DIS_MODE_EN)
                            result = TestModeType.Normal;

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "1")
                        if (!B10L_TX_DIS_MODE_EN)
                            result = TestModeType.Test1;

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "2")
                        if (!B10L_TX_DIS_MODE_EN)
                            result = TestModeType.Test2;

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "3")
                        if (!B10L_TX_DIS_MODE_EN)
                            result = TestModeType.Test3;

            if (!AN_EN)
                if (AN_FRC_MODE_EN)
                    if (B10L_TX_TEST_MODE == "0")
                        if (!B10L_TX_DIS_MODE_EN)
                            result = TestModeType.Transmit;

            if (eventTrigger)
                OnTestModeChanged(result);

            Debug.WriteLine($"[GetTestMode] {result.ToString()}");
            return TestModeType.Normal;
        }

        public TestModeType GetTestModeState()
        {
            return _testmodeState;
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
        }

        public void HardwareReset()
        {
            try
            {
                string command = "phyreset\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                var response = _ftdiService.ReadCommandResponse();
                Trace.WriteLine($"Response:{response}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public LocalRangeModel localMax_3(List<double> echo_pre, double th, int width_th_low, int width_th_high, bool mov_avg)
        {
            LocalRangeModel localRange = new LocalRangeModel();
            List<PeakModel> localMaxList = new List<PeakModel>();
            List<PeakModel> localMinList = new List<PeakModel>();

            #region local variables

            var th_echo_low = th * 5.0; //0.01 is quite close to NOT detect the Helukabel to Belden 74040, so define it as 0.005
            var th_echo_high = th * 3.0; //define it as 0.003 for th =0.001
            var th_echo = 0.003; // keep this as 0.005 to discard noise. FOr long cables, 0.005 does not detect the normal reflection coming from Belden 74040 to 100Ohms

            int ratio_max = 4;      // Maximum ratio between average slope and slope to declare the beginning or end of a pulse

            double maxHecho = 0;
            int maxHechoIdx_local = 0;
            bool startPos = false;
            bool startNeg = false;

            var size = echo_pre.Count();
            List<double> echo = new List<double>();

            int pos_rise_edge = 0;
            int pos_fall_edge = 0;

            int width = 0;

            int pos_start_slope = 0;
            int pos_end_slope = 0;
            int pos_start = 0;
            int pos_end = 0;
            int width_final = 0;

            double slope = 0;
            double slope_prev = 0;
            double slope_next = 0;
            double slope_avg = 0;
            double slope_avg_final = 0;
            double slope_avg_th = 0.001;
            double slope_ratio_prev = 0;
            double slope_ratio_next = 0;
            double slope_th = 0.0005; // Minimum slope thrshold allowed before declaring a start or end of a pulse

            int neg_fall_edge = 0;
            double minHecho = 0;
            int minHechoIdx_local = 0;
            int neg_rise_edge = 0;
            int neg_start_slope = 0;
            int neg_end_slope = 0;
            int neg_start = 0;
            int neg_end = 0;

            #endregion local variables

            if (mov_avg)
            {
                echo = GetMovingAverage(echo_pre, 2);
            }

            if (echo[0] > th_echo)
            {
                pos_rise_edge = 0;
                startPos = true;
            }
            else if (echo[0] < -1 * th_echo)
            {
                neg_fall_edge = 0;
                startNeg = true;
            }

            List<double> ratios = new List<double>();
            List<double> ratios_idx = new List<double>();

            for (int i = 0; i < size - 1; i++)
            {
                //Variable Threshold
                if (i < 72)
                    th_echo = th_echo_low;
                else
                    th_echo = th_echo_high;

                #region Positive Peaks Search

                if ((echo[i] < th_echo)
                 && (echo[i + 1] >= th_echo))
                {
                    pos_rise_edge = i;
                    maxHecho = 0;
                    startPos = true;

                    Debug.WriteLine($"Start of peak at i={i},echo={echo[i]}");
                }

                //look for max in the positive pulse
                if ((echo[i + 1] > maxHecho) && startPos)
                {
                    maxHecho = echo[i + 1];
                    maxHechoIdx_local = i + 1;
                }

                //Positive peak falling edge #Adding the ()
                if ((echo[i] >= th_echo && echo[i + 1] < th_echo)
                 || (i + 2 == size && startPos))
                {
                    pos_fall_edge = i + 1;
                    startPos = false;
                    width = pos_fall_edge - pos_rise_edge;

                    //Pulse amplitude check
                    if (maxHecho > th_echo)
                    {
                        #region Search for begin point through slope

                        //Search for begin point through slope
                        var j = 0;
                        while (true)
                        {
                            if ((maxHechoIdx_local - j - 1) >= 0)
                            {
                                slope = echo[maxHechoIdx_local - j] - echo[maxHechoIdx_local - j - 1];

                                if (echo[maxHechoIdx_local - j - 1] <= 0)
                                {
                                    pos_start_slope = maxHechoIdx_local - j - 1;
                                    break;
                                }
                                else if (slope <= slope_th) //Added if j != 0 to avoid division by 0 in slope_avg calculation
                                {
                                    if ((maxHechoIdx_local - j - 2) >= 0) //Make sure index does not underflow below 0.
                                    {
                                        slope_prev = echo[maxHechoIdx_local - j - 1] - echo[maxHechoIdx_local - j - 2];

                                        if (slope_prev <= slope_th)
                                        {
                                            if (j == 0)
                                                pos_start_slope = maxHechoIdx_local - j - 1;
                                            else
                                                pos_start_slope = maxHechoIdx_local - j;

                                            break;
                                        }
                                        else if (j != 0)
                                        {
                                            slope_next = echo[maxHechoIdx_local - j + 1] - echo[maxHechoIdx_local - j]; //Calculates the slope following the negative slope
                                            slope_avg = (echo[maxHechoIdx_local] - echo[maxHechoIdx_local - j]) / (j); //Average slope to this point

                                            slope_ratio_prev = slope_avg / slope_prev; //Ratio between slopes. Calculated here to avoid dividing by zero when slope_prev ==0

                                            if (slope_ratio_prev >= ratio_max)
                                            {
                                                pos_start_slope = maxHechoIdx_local - j;
                                                break;
                                            }
                                            else
                                                j = j + 1; //no turning point found
                                        }
                                        else //# if j=0 (meaning that the first slope is already below threshold)
                                        {
                                            j = j + 1;
                                        }
                                    }
                                    else
                                    {
                                        pos_start_slope = maxHechoIdx_local - j;
                                        break;
                                    }
                                }
                                else
                                {
                                    j = j + 1;
                                }
                            }
                            else
                            {
                                pos_start_slope = maxHechoIdx_local - j;
                                break;
                            }
                        }

                        #endregion Search for begin point through slope

                        #region Search for end point through slope

                        //Search for begin point through slope
                        var k = 0;
                        while (true)
                        {
                            if (maxHechoIdx_local + k + 1 < size)
                            {
                                slope = echo[maxHechoIdx_local + (k + 1)] - echo[maxHechoIdx_local + k];

                                if (echo[maxHechoIdx_local + k + 1] <= 0)
                                {
                                    pos_end_slope = maxHechoIdx_local + k + 1;
                                    break;
                                }
                                else if (slope >= (-1 * slope_th)) //When slope changes from negative to positive or 0, this is the positive end
                                {
                                    if ((maxHechoIdx_local + k + 2) < size)
                                    {
                                        slope_next = echo[maxHechoIdx_local + (k + 2)] - echo[maxHechoIdx_local + (k + 1)];

                                        if (slope_next >= (-1 * slope_th)) //If the second adjacent slope is also above the Theshold, (2) consecutive slopes, then turning point.
                                        {
                                            if (k == 0)
                                                pos_end_slope = maxHechoIdx_local + k + 1; //avoids start max to be the same point
                                            else
                                                pos_end_slope = maxHechoIdx_local + k;

                                            break;
                                        }
                                        else if (k != 0)
                                        {
                                            //average slope from peak to inflection point
                                            slope_avg = echo[maxHechoIdx_local + k] - echo[maxHechoIdx_local] / k;
                                            //Ratios calculations
                                            slope_ratio_next = slope_avg / slope_next;

                                            if (slope_ratio_next >= ratio_max)
                                            {
                                                pos_end_slope = maxHechoIdx_local + k;
                                                break;
                                            }
                                            else
                                            {
                                                k = k + 1;
                                            }
                                        }
                                        else
                                        {
                                            k = k + 1;
                                        }
                                    }
                                    else
                                    {
                                        pos_end_slope = maxHechoIdx_local + k;
                                        break;
                                    }
                                }
                                else
                                {
                                    k = k + 1;
                                }
                            }
                            else
                            {
                                pos_end_slope = maxHechoIdx_local + k;
                                break;
                            }
                        }

                        pos_start = pos_start_slope;
                        pos_end = pos_end_slope;

                        width_final = pos_end - pos_start;

                        slope_avg_final = (echo[maxHechoIdx_local] - echo[pos_start]) / (maxHechoIdx_local - pos_start);

                        if (echo_pre[maxHechoIdx_local - 1] > echo_pre[maxHechoIdx_local])
                            maxHechoIdx_local = maxHechoIdx_local - 1;

                        Debug.WriteLine("PosEchoIdx: " + maxHechoIdx_local);
                        Debug.WriteLine("PosEcho: " + maxHecho);
                        Debug.WriteLine("StartEdge: " + pos_start);
                        Debug.WriteLine("EndEdge: " + pos_end);
                        Debug.WriteLine("Width: " + width_final);
                        Debug.WriteLine($"Final average start slope = {slope_avg_final}");

                        if ((width_final > width_th_low) && (width_final < width_th_high))
                        {
                            if (Math.Abs(slope_avg_final) > slope_avg_th)
                            {
                                localMaxList.Add(new PeakModel() { Type = "max", idx = maxHechoIdx_local, Amplitude = maxHecho, StartEdge = pos_start, EndEdge = pos_end });
                            }
                        }

                        #endregion Search for end point through slope
                    }
                }

                #endregion Positive Peaks Search

                #region Negative Peaks Search

                if ((echo[i] > -1 * th_echo)
                 && (echo[i + 1] <= -1 * th_echo)) //Negative peak falling edge
                {
                    neg_fall_edge = i;
                    minHecho = 0;
                    startNeg = true;
                }

                if ((echo[i + 1] < minHecho) && startNeg) //Look for max in the positive pulse
                {
                    minHecho = echo[i + 1];
                    minHechoIdx_local = i + 1;
                }

                if (((echo[i] <= -1 * th_echo)
                  && (echo[i + 1] > -1 * th_echo))
                  || ((i + 2 == size) && startNeg)) //Positive peak falling edge
                {
                    neg_rise_edge = i + 1;
                    startNeg = false;

                    if (minHecho < (-1 * th_echo))
                    {
                        #region Search for begin point through slope

                        var j = 0;
                        while (true)
                        {
                            if (minHechoIdx_local - j - 1 >= 0)
                            {
                                slope = echo[minHechoIdx_local - j] - echo[minHechoIdx_local - j - 1];

                                if (echo[minHechoIdx_local - j - 1] >= 0)
                                {
                                    neg_start_slope = minHechoIdx_local - j - 1;
                                    break;
                                }
                                else if ((slope >= (-1.0 * slope_th))) //and (j is not 0): # When the slope changes from negative to positive or zero, looking from the from the peak, this is the start of the negative peak
                                {
                                    if ((minHechoIdx_local - j - 2) >= 0)
                                    {
                                        slope_prev = echo[minHechoIdx_local - j - 1] - echo[minHechoIdx_local - j - 2];

                                        if (slope_prev >= (-1 * slope_th))
                                        {
                                            if (j == 0)
                                                neg_start_slope = minHechoIdx_local - j - 1; //Avoids the start and Peak to be the same point
                                            else
                                                neg_start_slope = minHechoIdx_local - j;

                                            break;
                                        }
                                        else if (j != 0)
                                        {
                                            slope_next = echo[minHechoIdx_local - j + 1] - echo[minHechoIdx_local - j];
                                            slope_avg = (echo[minHechoIdx_local] - echo[minHechoIdx_local - j]) / (j);
                                            slope_ratio_prev = slope_avg / slope_prev;

                                            if (slope_ratio_prev >= ratio_max)
                                            {
                                                neg_start_slope = minHechoIdx_local - j;
                                                break;
                                            }
                                            else
                                            {
                                                j = j + 1;
                                            }
                                        }
                                        else
                                        {
                                            j = j + 1;
                                        }
                                    }
                                    else
                                    {
                                        neg_start_slope = minHechoIdx_local - j;
                                        break;
                                    }
                                }
                                else
                                {
                                    j = j + 1;
                                }
                            }
                            else
                            {
                                neg_start_slope = minHechoIdx_local - j;
                                break;
                            }
                        }

                        #endregion Search for begin point through slope

                        #region Search for end point through slope

                        var k = 0;
                        while (true)
                        {
                            if ((minHechoIdx_local + k + 1) < size)
                            {
                                slope = echo[minHechoIdx_local + (k + 1)] - echo[minHechoIdx_local + k];

                                if (echo[minHechoIdx_local + k + 1] >= 0)
                                {
                                    neg_end_slope = minHechoIdx_local + k + 1;
                                    break;
                                }
                                else if (slope <= slope_th) //When the slope changes from positive to negative or 0, this is the negative pulse end.
                                {
                                    if ((minHechoIdx_local + k + 2) < size)
                                    {
                                        slope_next = echo[minHechoIdx_local + (k + 2)] - echo[minHechoIdx_local + (k + 1)];
                                        if (slope_next <= slope_th)
                                        {
                                            if (j == 0)
                                                neg_end_slope = minHechoIdx_local + k + 1;
                                            else
                                                neg_end_slope = minHechoIdx_local + k;
                                            break;
                                        }
                                        else if (k != 0)
                                        {
                                            slope_avg = echo[minHechoIdx_local + k] - echo[minHechoIdx_local] / k; //average slope from peak to inflection point
                                            slope_ratio_next = slope_avg / slope_next;
                                            if (slope_ratio_next >= ratio_max)
                                            {
                                                neg_end_slope = minHechoIdx_local + k;
                                                break;
                                            }
                                            else
                                            {
                                                k = k + 1;
                                            }
                                        }
                                        else
                                        {
                                            k = k + 1;
                                        }
                                    }
                                    else
                                    {
                                        neg_end_slope = minHechoIdx_local + k;
                                        break;
                                    }
                                }
                                else
                                {
                                    k = k + 1;
                                }
                            }
                            else
                            {
                                neg_end_slope = minHechoIdx_local + k;
                                break;
                            }
                        }

                        neg_start = neg_start_slope;
                        neg_end = neg_end_slope;
                        width_final = neg_end - neg_start;
                        slope_avg_final = (echo[minHechoIdx_local] - echo[neg_start]) / (minHechoIdx_local - neg_start);

                        if (echo_pre[minHechoIdx_local - 1] < echo_pre[minHechoIdx_local])
                            minHechoIdx_local = minHechoIdx_local - 1;

                        Debug.WriteLine("NegEchoIdx: " + minHechoIdx_local);
                        Debug.WriteLine("NegEcho: " + minHecho);
                        Debug.WriteLine("StartEdge: " + neg_start);
                        Debug.WriteLine("EndEdge: " + neg_end);
                        Debug.WriteLine("Width: " + width_final);
                        Debug.WriteLine($"Final average start slope = {slope_avg_final}");

                        if ((width_final > width_th_low)
                         && (width_final < width_th_high))
                        {
                            if (Math.Abs(slope_avg_final) > slope_avg_th)
                            {
                                localMinList.Add(new PeakModel() { Type = "min", idx = minHechoIdx_local, Amplitude = minHecho, StartEdge = neg_start, EndEdge = neg_end });
                            }
                        }

                        #endregion Search for end point through slope
                    }
                }

                #endregion Negative Peaks Search
            }

            localRange.LocalMaxRange = localMaxList;
            localRange.LocalMinRange = localMinList;

            return localRange;
        }

        public string MdioReadCl22(uint regAddress)
        {
            throw new NotImplementedException();
        }

        public string MdioReadCl45(uint regAddress)
        {
            string response = string.Empty;
            string command = string.Empty;

            command = $"mdiord_cl45 {_phyAddress},{regAddress.ToString("X")}\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();
            }

            if (response.Contains("ERROR"))
            {
                OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            Debug.WriteLine($"Command:{command.TrimEnd()}");
            Debug.WriteLine($"Response:{response}");

            return response;
        }

        public string MdioWriteCl22(uint regAddress, uint data)
        {
            throw new NotImplementedException();
        }

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            string response = string.Empty;
            string command = string.Empty;

            command = $"mdiowr_cl45 {_phyAddress},{regAddress.ToString("X")},{data.ToString("X")}\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);

                response = _ftdiService.ReadCommandResponse().Trim();
            }

            if (response.Contains("ERROR"))
            {
                OnErrorOccured(new FeedbackModel() { Message = response, FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            Debug.WriteLine($"Command:{command.TrimEnd()}");
            Debug.WriteLine($"Response:{response}");

            return response;
        }

        public string PerformCableCalibration(decimal length)
        {
            string command = string.Empty;
            string response = string.Empty;

            command = $"tdrcablecal {length}\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            var res = RegexService.ExtractNumberData(response);
            if (res.Count == 1)
                response = res[0].ToString();

            if (response == "" || res.Count == 0 || response.Contains("ERROR"))
            {
                OnErrorOccured(new FeedbackModel() { Message = $"[tdrcablecal] {response}", FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrcablecal] NVP={response}", FeedBackType = FeedbackType.Info });
            return response;
        }

        public FaultType PerformFaultDetection()
        {
            string command = string.Empty;
            string response = string.Empty;
            FaultType fault = FaultType.None;

            command = $"tdrfaultdet\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

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
            }
            else
            {
                fault = FaultType.None;
            }

            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrfaultdet] Fault = {fault.ToString()}", FeedBackType = FeedbackType.Info });
            return fault;
        }

        public string PerformOffsetCalibration()
        {
            string command = string.Empty;
            string response = string.Empty;

            command = $"tdroffsetcal\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            var res = RegexService.ExtractNumberData(response);
            if (res.Count == 1)
                response = res[0].ToString();

            if (response == "" || res.Count == 0 || response.Contains("ERROR"))
            {
                OnErrorOccured(new FeedbackModel() { Message = $"[tdroffsetcal] {response}", FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdroffsetcal] Offset={response}", FeedBackType = FeedbackType.Info });
            return response;
        }

        public void ReadRegsiters()
        {
            foreach (var register in _registers)
            {
                register.Value = ReadYodaRg(register.Address);
            }
            Debug.WriteLine("ReadRegisters Done");
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
            string command = string.Empty;
            string response = string.Empty;
            List<string> coeffs = new List<string>();

            command = $"tdrsetcoeff {nvp},{coeff0},{coeffi}\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

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

        public void SetLoopbackSetting(LoopbackListingModel loopback)
        {
            FeedbackModel feedback = new FeedbackModel();

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

                    feedback.Message = $"[{_ftdiService.GetSerialNumber()}] Loopback Mode: {loopback.Name} Loopback Mode";
                    feedback.FeedBackType = FeedbackType.Info;
                    _loopbackState = LoopBackMode.Digital;
                    OnWriteProcessCompleted(feedback);
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

                    feedback.Message = $"[{_ftdiService.GetSerialNumber()}] Loopback Mode: {loopback.Name} Loopback Mode";
                    feedback.FeedBackType = FeedbackType.Info;
                    _loopbackState = LoopBackMode.LineDriver;
                    OnWriteProcessCompleted(feedback);
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

                    feedback.Message = $"[{_ftdiService.GetSerialNumber()}] Loopback Mode: {loopback.Name} Loopback Mode";
                    feedback.FeedBackType = FeedbackType.Info;
                    _loopbackState = LoopBackMode.ExtCable;
                    OnWriteProcessCompleted(feedback);
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

                    feedback.Message = $"[{_ftdiService.GetSerialNumber()}] Loopback Mode: {loopback.Name} Loopback Mode";
                    feedback.FeedBackType = FeedbackType.Info;
                    _loopbackState = LoopBackMode.MacRemote;
                    OnWriteProcessCompleted(feedback);
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

                    feedback.Message = $"[{_ftdiService.GetSerialNumber()}] Loopback Mode: {loopback.Name} Loopback Mode";
                    feedback.FeedBackType = FeedbackType.Info;
                    _loopbackState = LoopBackMode.MAC;
                    OnWriteProcessCompleted(feedback);
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

                    feedback.Message = $"[{_ftdiService.GetSerialNumber()}] Loopback Mode: {loopback.Name} Loopback Mode";
                    feedback.FeedBackType = FeedbackType.Info;
                    _loopbackState = LoopBackMode.OFF;
                    OnWriteProcessCompleted(feedback);
                    break;

                default:
                    //this.Info("    SPE PHY Loopback NOT configured - use one of PMA / PCS / MAC Interface / MAC Interface Remote / External MII/RMII");
                    break;
            }
        }

        public void SetMode(CalibrationMode mode)
        {
            string command = string.Empty;
            string response = string.Empty;

            command = $"tdrsetmode {(int)mode}\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            var res = RegexService.ExtractNumberData(response);
            if (res.Count == 1)
                response = res[0].ToString();

            if (response == "" || res.Count == 0)
            {
                OnErrorOccured(new FeedbackModel() { Message = $"[tdrsetmode] {response}", FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            //OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrsetmode] {response}", FeedBackType = FeedbackType.Info });
        }

        public void SetNegotiateMasterSlaveSetting(AutoNegMasterSlaveAdvertisementItem negotiateMasterSlave)
        {
            Debug.WriteLine("SetNegotiateMasterSlaveSetting Start");
            Debug.WriteLine($"SetNegotiateMasterSlaveSetting is {negotiateMasterSlave}");

            switch (negotiateMasterSlave)
            {
                case AutoNegMasterSlaveAdvertisementItem.Prefer_Slave:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_ADV_MST", 0);
                    WriteYodaRg("AN_ADV_FORCE_MS", 0);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _autoNegMasterSlave = AutoNegMasterSlaveAdvertisementItem.Prefer_Slave;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Master or Slave Preference: Prefer_Slave", FeedBackType = FeedbackType.Info });
                    break;

                case AutoNegMasterSlaveAdvertisementItem.Prefer_Master:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_ADV_MST", 1);
                    WriteYodaRg("AN_ADV_FORCE_MS", 0);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _autoNegMasterSlave = AutoNegMasterSlaveAdvertisementItem.Prefer_Master;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Master or Slave Preference: Prefer_Master", FeedBackType = FeedbackType.Info });
                    break;

                case AutoNegMasterSlaveAdvertisementItem.Forced_Slave:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_ADV_MST", 0);
                    WriteYodaRg("AN_ADV_FORCE_MS", 0);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _autoNegMasterSlave = AutoNegMasterSlaveAdvertisementItem.Forced_Slave;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Master or Slave Preference: Forced_Slave", FeedBackType = FeedbackType.Info });
                    break;

                case AutoNegMasterSlaveAdvertisementItem.Forced_Master:
                    WriteYodaRg("CRSM_SFT_PD", 1);
                    WriteYodaRg("AN_ADV_MST", 1);
                    WriteYodaRg("AN_ADV_FORCE_MS", 1);
                    WriteYodaRg("AN_EN", 1);
                    WriteYodaRg("CRSM_SFT_PD", 0);
                    _autoNegMasterSlave = AutoNegMasterSlaveAdvertisementItem.Forced_Master;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Master or Slave Preference: Forced_Master", FeedBackType = FeedbackType.Info });
                    break;

                default:
                    break;
            }
            Debug.WriteLine("SetNegotiateMasterSlaveSetting End");
        }

        public List<string> SetNvp(decimal nvpValue)
        {
            string command = string.Empty;
            string response = string.Empty;
            List<string> resList = new List<string>();
            command = $"tdrsetnvp {nvpValue}\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            var res = RegexService.ExtractNumberData(response);
            if (res.Count >= 1)
            {
                resList.Add(res[0].ToString());
                resList.Add(res[1].ToString());
            }

            if (response == "" || res.Count == 0 || response.Contains("ERROR"))
            {
                //OnErrorOccured(new FeedbackModel() { Message = $"[tdrsetnvp] {response}", FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrsetnvp] {response}", FeedBackType = FeedbackType.Info });
            return resList;
        }

        public string SetOffset(decimal offset)
        {
            string command = string.Empty;
            string response = string.Empty;

            command = $"tdrsetoffset {offset}\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            var res = RegexService.ExtractNumberData(response);
            if (res.Count == 1)
                response = res[0].ToString();

            if (response == "" || res.Count == 0 || response.Contains("ERROR"))
            {
                //OnErrorOccured(new FeedbackModel() { Message = $"[tdrsetoffset] {response}", FeedBackType = FeedbackType.Error });
                throw new ApplicationException("[Offset Calibration]" + response);
            }

            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[Offset Calibration] Offset={response}", FeedBackType = FeedbackType.Info });
            return response;
        }

        public void SetPeakToPeakVoltageSetting(PeakVoltageAdvertisementItem pkpkVoltage)
        {
            Debug.WriteLine("SetPeakToPeakVoltageSetting Start");
            Debug.WriteLine($"SetPeakToPeakVoltageSetting is {pkpkVoltage}");

            switch (pkpkVoltage)
            {
                case PeakVoltageAdvertisementItem.Capable2p4Volts_Requested2p4Volts:
                    // Configuring for high voltage transmit levels 2.4VPkpk
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_ABL", 1);
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_REQ", 1);
                    _txLevel = PeakVoltageAdvertisementItem.Capable2p4Volts_Requested2p4Volts;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Tx Level: Capable2p4Volts_Requested2p4Volts", FeedBackType = FeedbackType.Info });
                    break;

                case PeakVoltageAdvertisementItem.Capable2p4Volts_Requested1Volt:
                    // Configuring for high voltage transmit levels 2.4VPkpk
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_ABL", 1);
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_REQ", 0);
                    _txLevel = PeakVoltageAdvertisementItem.Capable2p4Volts_Requested1Volt;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Tx Level: Capable2p4Volts_Requested1Volt", FeedBackType = FeedbackType.Info });
                    break;

                case PeakVoltageAdvertisementItem.Capable1Volt:
                    // Configuring for low voltage transmit levels 1.0VPk-pk AnAdvB10lTxLvlHiAbl
                    WriteYodaRg("AN_ADV_B10L_TX_LVL_HI_ABL", 0);
                    _txLevel = PeakVoltageAdvertisementItem.Capable1Volt;
                    OnWriteProcessCompleted(new FeedbackModel() { Message = "Tx Level: Capable1Volt", FeedBackType = FeedbackType.Info });
                    break;

                default:
                    break;
            }

            // Renegotiate immediately
            RestartAutoNegotiation();
            Debug.WriteLine("SetPeakToPeakVoltageSetting End");
        }

        public void SetRxSuppressionSetting(bool isRxSuppression)
        {
            WriteYodaRg("MAC_IF_REM_LB_RX_SUP_EN", isRxSuppression ? (uint)1 : (uint)0);
        }

        public void SetTestModeSetting(TestModeListingModel testModeModel)
        {
            switch (testModeModel.Name1)
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
                    Debug.WriteLine($"Test Mode Selected, {testModeModel.Name1}:{testModeModel.Name2}");
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
                    Debug.WriteLine($"Test Mode Selected, {testModeModel.Name1}:{testModeModel.Name2}");
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
                    Debug.WriteLine($"Test Mode Selected, {testModeModel.Name1}:{testModeModel.Name2}");
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
                    Debug.WriteLine($"Test Mode Selected, {testModeModel.Name1}:{testModeModel.Name2}");
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
                    Debug.WriteLine($"Test Mode Selected, {testModeModel.Name1}:{testModeModel.Name2}");
                    break;

                default:
                    break;
            }
        }

        public void SetTxSuppressionSetting(bool isTxSuppression)
        {
            WriteYodaRg("MAC_IF_LB_TX_SUP_EN", isTxSuppression ? (uint)1 : (uint)0);
        }

        public void SoftwarePowerdown(bool isSoftwarePowerdown)
        {
            if (isSoftwarePowerdown)
                WriteYodaRg("CRSM_SFT_PD", 1);
            else
                WriteYodaRg("CRSM_SFT_PD", 0);
        }

        public void SoftwareReset()
        {
            try
            {
                string command = "reset\n";

                _ftdiService.Purge();
                _ftdiService.SendData(command);
                var response = _ftdiService.ReadCommandResponse();
                Trace.WriteLine($"Response:{response}");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public void TDRInit()
        {
            string command = string.Empty;
            string response = string.Empty;
            try
            {
                command = $"tdrinit\n";
                lock (thisLock)
                {
                    _ftdiService.Purge();
                    _ftdiService.SendData(command);
                    response = _ftdiService.ReadCommandResponse().Trim();
                }

                if (response.Contains("ERROR"))
                    throw new ApplicationException(response);

                OnWriteProcessCompleted(new FeedbackModel() { Message = $"[tdrinit] TDR Initialized", FeedBackType = FeedbackType.Info });
            }
            catch (ApplicationException ex)
            {
                OnErrorOccured(new FeedbackModel() { Message = ex.Message, FeedBackType = FeedbackType.Error });
            }
        }

        protected virtual void OnErrorOccured(FeedbackModel feedback)
        {
            ErrorOccured?.Invoke(this, feedback);
        }

        protected virtual void OnFrameContentChanged(FrameType value)
        {
            FrameContentChanged?.Invoke(this, value);
        }

        protected virtual void OnFrameGenCheckerStatusChanged(string status)
        {
            FrameGenCheckerTextStatusChanged?.Invoke(this, status);
        }

        protected virtual void OnLinklengthChanged(string linkLengthValue)
        {
            LinkLengthChanged?.Invoke(this, linkLengthValue);
        }

        protected virtual void OnLoopbackChanged(LoopBackMode value)
        {
            LoopbackChanged?.Invoke(this, value);
        }

        protected virtual void OnMseValueChanged(string mseValue)
        {
            MseValueChanged?.Invoke(this, mseValue);
        }

        protected virtual void OnNegotiationMasterSlaveChanged(AutoNegMasterSlaveAdvertisementItem value)
        {
            NegotiationMasterSlaveChanged?.Invoke(this, value);
        }

        protected virtual void OnPeakVoltageChanged(PeakVoltageAdvertisementItem value)
        {
            PeakVoltageChanged?.Invoke(this, value);
        }

        protected virtual void OnReadProcessCompleted(FeedbackModel feedback)
        {
            ReadProcessCompleted?.Invoke(this, feedback);
        }

        protected virtual void OnResetFrameGenCheckerStatisticsChanged(string status)
        {
            ResetFrameGenCheckerStatisticsChanged?.Invoke(this, status);
        }

        protected virtual void OnTestModeChanged(TestModeType value)
        {
            TestModeChanged?.Invoke(this, value);
        }

        protected virtual void OnWriteProcessCompleted(FeedbackModel feedback)
        {
            WriteProcessCompleted?.Invoke(this, feedback);
        }

        private List<double> acquireCoeffSerial()
        {
            List<double> echo = new List<double>();
            //List<double> echoReadValues = new List<double>();

            // Registers needed to read the active link TDR
            uint SigAnlEn = 0x1f9400;
            uint SigAnlSel1 = 0x1f9403;
            uint SigAnlCfgRst = 0x1f942d;
            uint AdptFltrCoefRdReq = 0x0182b1;
            uint AdptFltrFrzAct = 0x0182b2;
            uint AdptFltrCoef = 0x0182b3;

            //Device configuration for TDR coefficient read
            WriteYodaRg(SigAnlCfgRst, 0x1);
            WriteYodaRg(SigAnlSel1, 0x1);
            WriteYodaRg(SigAnlEn, 0x1);

            //Request TDR coefficient
            WriteYodaRg(AdptFltrCoefRdReq, 0x3);

            //Poll AdptFltrFrzAct until ==1, put a timeout equivalent to 10ms
            pollRegister(AdptFltrFrzAct, 0x1);

            //Read AdptFltrCoef 144 times and store in previously initialized array
            for (int i = 0; i < 144; i++)
            {
                //Read AdptFltrCoef
                var echoReadValue = ReadYodaRg(AdptFltrCoef);

                //Convert to meaningful information
                echo.Add(conv_2c(Convert.ToUInt32(echoReadValue, 16), 16) / (1 << 14));
            }

            //return to previous configuration
            WriteYodaRg(AdptFltrCoefRdReq, 0x00);

            return echo;
        }

        private double conv_2c(uint echo, int nbits)
        {
            return echo - ((echo * 2) & (1 << nbits));
        }

        private string GetCoeffActive()
        {
            string command = string.Empty;
            string response = string.Empty;
            string nvp = string.Empty;

            command = "tdrgetcoeff\n";
            lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
            }

            var res = RegexService.ExtractNumberData(response);
            if (res.Count >= 1)
            {
                nvp = res[0].ToString("f6", CultureInfo.InvariantCulture);
            }

            if (response == "" || res.Count == 0)
            {
                OnErrorOccured(new FeedbackModel() { Message = $"[tdrgetnvp] {response}", FeedBackType = FeedbackType.Error });
                throw new ApplicationException(response);
            }

            return nvp;
        }

        private List<double> GetMovingAverage(List<double> echo, uint avg = 2)
        {
            List<double> outputValue = new List<double>();
            outputValue.Add(0);
            for (int i = 0; i < echo.Count; i++)
            {
                if (i != 0)
                {
                    outputValue.Add((echo[i] + echo[i - 1]) / 2.0);
                }
            }

            return outputValue;
        }

        private bool pollRegister(uint regAddr, uint value, uint count = 100)
        {
            uint cnt = 0;
            while (Convert.ToUInt32(ReadYodaRg(regAddr)) != value && cnt < count)
            {
                cnt += 1;
            }

            if (cnt == count - 1)
                return false;
            else
                return true;
        }

        private string ReadYodaRg(string name)
        {
            RegisterModel register = null;
            string value = string.Empty;

            register = GetRegister(name);
            if (register == null)
                throw new ApplicationException("Invalid Register");

            lock (thisLock)
                register.Value = MdioReadCl45(register.Address);

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

            value = MdioReadCl45(registerAddress);

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

        private void WriteYodaRg(string name, uint value)
        {
            RegisterModel register = null;
            var result = _registers.Where(x => x.Name == name).ToList();
            if (result.Count == 0)
            {
                result = _registers.Where(r => r.BitFields.Any(b => b.Name == name)).ToList();
                if (result.Count == 1)
                {
                    register = result[0];
                    foreach (var bitField in result[0].BitFields)
                    {
                        if (bitField.Name == name)
                            bitField.Value = value;
                    }
                }
                else
                    throw new NullReferenceException($"{name} is not in the register/bitfield list.");
            }
            else
            {
                register = result[0];
                register.Value = value.ToString("X");
            }

            Debug.WriteLine($"Read Register Value:{MdioReadCl45(register.Address)}");
            MdioWriteCl45(register.Address, UInt32.Parse(register.Value, NumberStyles.HexNumber));
            Debug.WriteLine($"Read RegisterValue:{MdioReadCl45(register.Address)}");
            OnWriteProcessCompleted(new FeedbackModel() { Message = $"[{_ftdiService.GetSerialNumber()}] [Write] Name: {name}, Value: {value.ToString("X")}", FeedBackType = FeedbackType.Info });
        }

        private void WriteYodaRg(uint registerAddress, uint value)
        {
            MdioWriteCl45(registerAddress, value);
            //OnWriteProcessCompleted(new FeedbackModel() { Message = $"[{_ftdiService.GetSerialNumber()}] [Write] Address: 0x{registerAddress.ToString("X")}, Value: {value.ToString("X")}", FeedBackType = FeedbackType.Info });
        }

        public void Speed1000FdAdvertisement(bool spd1000FdAdv_st)
        {
            throw new NotImplementedException();
        }

        public void Speed1000HdAdvertisement(bool spd1000HdAdv_st)
        {
            throw new NotImplementedException();
        }

        public void Speed100FdAdvertisement(bool spd100FdAdv_st)
        {
            throw new NotImplementedException();
        }
        public void Speed100HdAdvertisement(bool spd100HdAdv_st)
        {
            throw new NotImplementedException();
        }
        public void Speed10FdAdvertisement(bool spd10FdAdv_st)
        {
            throw new NotImplementedException();
        }
        public void Speed10HdAdvertisement(bool spd10HdAdv_st)
        {
            throw new NotImplementedException();
        }
        public void Speed1000EEEAdvertisement(bool spd1000EEEAdv_st)
        {
            throw new NotImplementedException();
        }
        public void Speed100EEEAdvertisement(bool spd100EEEAdv_st)
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
    }
}