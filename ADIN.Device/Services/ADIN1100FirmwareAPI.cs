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
    public class ADIN1100FirmwareAPI : IFirmwareAPI
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

        public event EventHandler<FrameType> FrameContentChanged;

        public event EventHandler<string> FrameGenCheckerTextStatusChanged;

        public event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        public event EventHandler<FeedbackModel> WriteProcessCompleted;

        public bool isFrameGenCheckerOngoing { get; set; } = false;

        public void AdvertisedForcedSpeed(string advFrcSpd)
        {
            throw new NotImplementedException();
        }

        public void AutoMDIXMode(string autoMDIXmod)
        {
            throw new NotImplementedException();
        }

        public void DisableLinking(bool isDisabledLinking)
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

        public void EnableEnergyDetectPowerDown(string enEnergyDetect)
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

        public void LogAdvertisedSpeed(List<string> listAdvSpd)
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

        public void SetForcedSpeed(string advFrcSpd)
        {
            throw new NotImplementedException();
        }

        public void SetFrameCheckerSetting(FrameGenCheckerModel frameContent)
        {
            throw new NotImplementedException();
        }

        public void SetGpClkPinControl(string gpClkPinCtrl)
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

        public void Speed1000EEEAdvertisement(bool spd1000EEEAdv_st)
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

        public void Speed100EEEAdvertisement(bool spd100EEEAdv_st)
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
    }
}