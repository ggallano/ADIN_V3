using ADI.Register.Models;
using ADI.Register.Services;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using FTDIChip.Driver.Services;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ADIN1300FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers)
        {
            _ftdiService = ftdiService;
            _registers = registers;
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

        public bool isFrameGenCheckerOngoing { get; set; }
        public void ExecuteSript(ScriptModel script)
        {
            throw new NotImplementedException();
        }

        public string GetAnStatus()
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

        public AutoNegMasterSlaveAdvertisementItem GetNegotiationMasterSlaveInitialization(bool eventTrigger = false)
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
            throw new NotImplementedException();
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

        public void MdioReadCl22(uint phyAddress, uint regAddress)
        {
            throw new NotImplementedException();
        }

        public string MdioReadCl45(uint regAddress)
        {
            throw new NotImplementedException();
        }

        public void MdioWriteCl22(uint phyAddress, uint regAddress, uint data)
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
    }
}
