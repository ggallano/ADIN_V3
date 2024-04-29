using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADI.Register.Models;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using FTDIChip.Driver.Services;

namespace ADIN.Device.Services
{
    public class ADIN2111FirmwareAPI : IFirmwareAPI, ICableDiagnostic
    {
        private BoardRevision _boardRev;
        private BoardType _boardType;
        private IFTDIServices _ftdiService;
        private uint _phyAddress;
        private ObservableCollection<RegisterModel> _registers;

        public ADIN2111FirmwareAPI(IFTDIServices ftdiService)
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
        public ADIN2111FirmwareAPI(IFTDIServices ftdiService, uint phyAddress, ObservableCollection<RegisterModel> registers, BoardRevision boardRev, BoardType boardType)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
            _boardRev = boardRev;
            _boardType = boardType;
        }

        public bool isFrameGenCheckerOngoing
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
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

        public void ExecuteSript(ScriptModel script)
        {
            throw new NotImplementedException();
        }

        public string GetAnStatus()
        {
            throw new NotImplementedException();
        }

        public List<string> GetCoeff()
        {
            throw new NotImplementedException();
        }

        public decimal GetFaultDistance()
        {
            throw new NotImplementedException();
        }

        public void GetFrameCheckerStatus()
        {
            throw new NotImplementedException();
        }

        public FrameType GetFrameContentInitialization(bool eventTrigger = false)
        {
            throw new NotImplementedException();
        }

        public string GetFrameGeneratorStatus()
        {
            throw new NotImplementedException();
        }

        public string GetLinkLength()
        {
            throw new NotImplementedException();
        }

        public string GetLinkStatus()
        {
            throw new NotImplementedException();
        }

        public LoopBackMode GetLoopbackInitialization(bool eventTrigger = false)
        {
            throw new NotImplementedException();
        }

        public LoopBackMode GetLoopbackState()
        {
            throw new NotImplementedException();
        }

        public string GetMasterSlaveStatus()
        {
            throw new NotImplementedException();
        }

        public BoardType GetModelNum(out uint phyAddress)
        {
            throw new NotImplementedException();
        }

        public string GetMseValue()
        {
            throw new NotImplementedException();
        }

        public AutoNegMasterSlaveAdvertisementItem GetNegotiationMasterSlaveInitialization(bool eventTrigger = false)
        {
            throw new NotImplementedException();
        }

        public string GetNvp()
        {
            throw new NotImplementedException();
        }

        public string GetOffset()
        {
            throw new NotImplementedException();
        }

        public PeakVoltageAdvertisementItem GetPeakVoltageInitialization(bool eventTrigger = false)
        {
            throw new NotImplementedException();
        }

        public EthPhyState GetPhyState()
        {
            throw new NotImplementedException();
        }

        public string GetRegisterJsonFile(BoardRevision revNum)
        {
            throw new NotImplementedException();
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

        public ObservableCollection<RegisterModel> GetStatusRegisters()
        {
            throw new NotImplementedException();
        }

        public TestModeType GetTestModeInitialization(bool eventTrigger = false)
        {
            throw new NotImplementedException();
        }

        public TestModeType GetTestModeState()
        {
            throw new NotImplementedException();
        }

        public string GetTxLevelStatus()
        {
            throw new NotImplementedException();
        }

        public void HardwareReset()
        {
            throw new NotImplementedException();
        }

        public string MdioReadCl22(uint regAddress)
        {
            throw new NotImplementedException();
        }

        public string MdioReadCl45(uint regAddress)
        {
            throw new NotImplementedException();
        }

        public string MdioWriteCl22(uint regAddress, uint data)
        {
            throw new NotImplementedException();
        }

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            throw new NotImplementedException();
        }

        public string PerformCableCalibration(decimal length)
        {
            throw new NotImplementedException();
        }

        public FaultType PerformFaultDetection()
        {
            throw new NotImplementedException();
        }

        public string PerformOffsetCalibration()
        {
            throw new NotImplementedException();
        }

        public void ReadRegsiters()
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

        public List<string> SetCoeff(decimal nvp, decimal coeff0, decimal coeffi)
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

        public void SetMode(CalibrationMode mode)
        {
            throw new NotImplementedException();
        }

        public void SetNegotiateMasterSlaveSetting(AutoNegMasterSlaveAdvertisementItem negotiateMasterSlave)
        {
            throw new NotImplementedException();
        }

        public List<string> SetNvp(decimal nvpValue)
        {
            throw new NotImplementedException();
        }

        public string SetOffset(decimal offset)
        {
            throw new NotImplementedException();
        }

        public void SetPeakToPeakVoltageSetting(PeakVoltageAdvertisementItem pkpkVoltage)
        {
            throw new NotImplementedException();
        }

        public void SetRxSuppressionSetting(bool isRxSuppression)
        {
            throw new NotImplementedException();
        }

        public void SetTestModeSetting(TestModeListingModel testModeModel)
        {
            throw new NotImplementedException();
        }

        public void SetTxSuppressionSetting(bool isTxSuppression)
        {
            throw new NotImplementedException();
        }

        public void SoftwarePowerdown(bool isSoftwarePowerdown)
        {
            throw new NotImplementedException();
        }

        public void SoftwareReset()
        {
            throw new NotImplementedException();
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
        public void SetGpClkPinControl(string gpClkPinCtrl)
        {
            throw new NotImplementedException();
        }

        public void TDRInit()
        {
            throw new NotImplementedException();
        }
    }
}
