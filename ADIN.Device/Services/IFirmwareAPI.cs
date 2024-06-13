using ADI.Register.Models;
using ADIN.Device.Models;
using ADIN.WPF.Models;
using Helper.Feedback;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADIN.Device.Services
{
    public interface IFirmwareAPI
    {
        event EventHandler<FrameType> FrameContentChanged;

        event EventHandler<string> FrameGenCheckerTextStatusChanged;

        event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        event EventHandler<FeedbackModel> WriteProcessCompleted;

        void DisableLinking(bool isDisabledLinking);

        void GetFrameCheckerStatus();

        string GetFrameGeneratorStatus();

        string GetLinkStatus();

        string GetMseValue();

        string GetMseValue(BoardRevision boardRev);

        EthPhyState GetPhyState();

        string GetSpeedMode();

        List<string> LocalAdvertisedSpeedList();

        ObservableCollection<RegisterModel> ReadRegsiters();
        List<string> RemoteAdvertisedSpeedList();
        void ResetPhy(ResetType reset);

        void RestartAutoNegotiation();

        void SoftwarePowerdown(bool isSoftwarePowerdown);
    }
}