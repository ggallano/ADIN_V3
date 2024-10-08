// <copyright file="TestModeModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.WPF.Models;
using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public class TestModeModel
    {
        public List<TestModeListingModel> TestModes { get; set; }
        public TestModeListingModel TestMode { get; set; }

        public TestModeModel()
        {
            TestModes = new List<TestModeListingModel>();
        }
    }
}
