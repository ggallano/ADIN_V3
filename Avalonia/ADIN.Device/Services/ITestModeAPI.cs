// <copyright file="ITestModeAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.Models;

namespace ADIN.Device.Services
{
    public interface ITestModeAPI
    {
        void SetTestMode(TestModeListingModel testMode, uint framelength);
    }
}
