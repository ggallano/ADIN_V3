// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;

namespace ADIN.Device.Services
{
    public interface IFrameGenCheckerAPI
    {
        bool isFrameGenCheckerOngoing { get; set; }
        void SetFrameCheckerSetting(FrameGenCheckerModel frameContent);
        void ResetFrameGenCheckerStatistics();
    }
}
