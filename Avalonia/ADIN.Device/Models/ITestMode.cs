// <copyright file="ITestMode.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.Models;
using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public interface ITestMode
    {
        List<TestModeListingModel> TestModes { get; set; }
        TestModeListingModel TestMode { get; set; }
        uint TestModeFrameLength { get; set; }
    }
}