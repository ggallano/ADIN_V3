// <copyright file="IFirmwareAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADI.Register.Models;
using ADIN.Device.Models;
using Helper.Feedback;
using Helper.MSE;
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

        event EventHandler<List<string>> GigabitCableDiagCompleted;

        void DisableLinking(bool isDisabledLinking);

        void GetFrameCheckerStatus();

        string GetFrameGeneratorStatus();

        string GetLinkStatus();

        string GetMseValue();

        MseSnr GetMseSnrValue();

        string GetMseValue(BoardRevision boardRev);

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