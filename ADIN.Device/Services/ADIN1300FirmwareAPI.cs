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
        private IFTDIServices _ftdiService;
        private IRegisterService _registerService;
        private ObservableCollection<RegisterModel> _registers;
        private uint _phyAddress;

        public ADIN1300FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, uint phyAddress)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
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

        public string MdioReadCl22(uint regAddress)
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
                throw new ApplicationException(response);
            }

            Debug.WriteLine($"Command:{command.TrimEnd()}");
            Debug.WriteLine($"Response:{response}");

            return response;
        }

        public string MdioReadCl45(uint regAddress)
        {
            string response = string.Empty;
            string command = string.Empty;

            command = $"mdiord_cl45 {_phyAddress},{regAddress.ToString("X")}\n";
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

        public string MdioWriteCl22(uint regAddress, uint data)
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

        public string MdioWriteCl45(uint regAddress, uint data)
        {
            string response = string.Empty;
            string command = string.Empty;
            string command2 = string.Empty;

            command = $"mdiowrite {_phyAddress},10,{regAddress.ToString("X")}\n";
            command2 = $"mdiowrite {_phyAddress},11,{data.ToString("X")}\n";
            //lock (thisLock)
            {
                _ftdiService.Purge();
                _ftdiService.SendData(command);
                response = _ftdiService.ReadCommandResponse().Trim();
                _ftdiService.SendData(command2);
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

        private string ReadYogaRg(string name)
        {
            RegisterModel register = null;
            string value = string.Empty;

            register = GetRegister(name);
            if (register == null)
                throw new ApplicationException("Invalid Register");

            //lock (thisLock)
                register.Value = MdioReadCl45(register.Address);

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
            string value = string.Empty;

            value = MdioReadCl45(registerAddress);

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
    }
}
