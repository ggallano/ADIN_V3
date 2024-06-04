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
        private IFTDIServices _ftdiService;
        private ObservableCollection<RegisterModel> _registers;
        private uint _phyAddress;
        private object _mainLock;

        public ADIN1100FirmwareAPI(IFTDIServices ftdiService, ObservableCollection<RegisterModel> registers, uint phyAddress, object mainLock)
        {
            _ftdiService = ftdiService;
            _registers = registers;
            _phyAddress = phyAddress;
            _mainLock = mainLock;
        }

        public bool isFrameGenCheckerOngoing { get; set; }

        public event EventHandler<FeedbackModel> WriteProcessCompleted;
        public event EventHandler<string> FrameGenCheckerTextStatusChanged;
        public event EventHandler<FrameType> FrameContentChanged;
        public event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        public void DisableLinking(bool isDisabledLinking)
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

        public string GetMseValue()
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
            throw new NotImplementedException();
        }

        public string RegisterRead(uint regAddress)
        {
            throw new NotImplementedException();
        }

        public string RegisterWrite(uint regAddress, uint data)
        {
            throw new NotImplementedException();
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

        public void SetTestMode(TestModeListingModel testMode, uint framelength)
        {
            throw new NotImplementedException();
        }

        public void SoftwarePowerdown(bool isSoftwarePowerdown)
        {
            throw new NotImplementedException();
        }
    }
}