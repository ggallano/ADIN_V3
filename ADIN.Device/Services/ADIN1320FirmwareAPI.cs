using ADI.Register.Models;
using ADI.Register.Services;
using ADIN.Device.Models;
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
    public class ADIN1320FirmwareAPI : IADIN1320API
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

        public ADIN1320FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, uint phyAddress, object mainLock)
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
        public event EventHandler<List<string>> GigabitCableDiagCompleted;

        public string AdvertisedSpeed()
        {
            throw new NotImplementedException();
        }

        public void DisableLinking(bool isDisabledLinking)
        {
            throw new NotImplementedException();
        }

        public void ExecuteSript(ScriptModel script)
        {
            throw new NotImplementedException();
        }

        public string GetCableLength()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public MseModel GetMseValue()
        {
            throw new NotImplementedException();
        }

        public MseModel GetMseValue(BoardRevision boardRev)
        {
            throw new NotImplementedException();
        }

        public EthPhyState GetPhyState()
        {
            throw new NotImplementedException();
        }

        public string GetSpeedMode()
        {
            throw new NotImplementedException();
        }

        public List<string> LocalAdvertisedSpeedList()
        {
            throw new NotImplementedException();
        }

        protected virtual void OnWriteProcessCompleted(FeedbackModel feedback)
        {
            WriteProcessCompleted?.Invoke(this, feedback);
        }

        public ObservableCollection<RegisterModel> ReadRegsiters()
        {
            throw new NotImplementedException();
        }

        public List<string> RemoteAdvertisedSpeedList()
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

        public void SoftwarePowerdown(bool isSoftwarePowerdown)
        {
            throw new NotImplementedException();
        }
    }
}
