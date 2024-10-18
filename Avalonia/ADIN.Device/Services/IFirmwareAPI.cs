// <copyright file="IFirmwareAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Register.Models;
using Helper.Feedback;
using System.Collections.ObjectModel;

namespace ADIN.Device.Services
{
    public interface IFirmwareAPI
    {
        event EventHandler<FrameType> FrameContentChanged;

        event EventHandler<string> FrameGenCheckerTextStatusChanged;

        event EventHandler<string> ResetFrameGenCheckerStatisticsChanged;

        event EventHandler<string> ResetFrameGenCheckerErrorStatisticsChanged;

        event EventHandler<FeedbackModel> WriteProcessCompleted;

        event EventHandler<List<string>> GigabitCableDiagCompleted;

        string AdvertisedSpeed();

        void DisableLinking(bool isDisabledLinking);

        string GetCableLength();

        void GetFrameCheckerStatus();

        string GetFrameGeneratorStatus();

        string GetLinkStatus();

        MseModel GetMseValue();

        MseModel GetMseValue(BoardRevision boardRev);

        EthPhyState GetPhyState();

        string GetSpeedMode();

        List<string> LocalAdvertisedSpeedList();

        ObservableCollection<RegisterModel> ReadRegsiters();
        List<string> RemoteAdvertisedSpeedList();
        void ResetPhy(ResetType reset);

        void RestartAutoNegotiation();

        void SoftwarePowerdown(bool isSoftwarePowerdown);

        void ExecuteSript(ScriptModel script);
    }
}
