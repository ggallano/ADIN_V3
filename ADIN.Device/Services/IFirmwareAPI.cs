using ADI.Register.Models;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADIN.Device.Services
{
    public interface IFirmwareAPI : IADIN1300API, IADIN1200API
    {
        event EventHandler<FeedbackModel> WriteProcessCompleted;

        event EventHandler<string> FrameGenCheckerTextStatusChanged;

        event EventHandler<FrameType> FrameContentChanged;

        event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;
        ObservableCollection<RegisterModel> ReadRegsiters();
        EthPhyState GetPhyState();
        string GetLinkStatus();
        string GetMseValue();
        void GetFrameCheckerStatus();
        string GetFrameGeneratorStatus();
        string GetSpeedMode();
        List<string> LocalAdvertisedSpeedList();
        List<string> RemoteAdvertisedSpeedList();
        void SoftwarePowerdown(bool isSoftwarePowerdown);
        void DisableLinking(bool isDisabledLinking);
        void RestartAutoNegotiation();
        void ResetPhy(ResetType reset);
    }
}