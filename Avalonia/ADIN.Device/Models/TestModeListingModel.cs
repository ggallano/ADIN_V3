// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models
{
    public class TestModeListingModel
    {
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Description { get; set; }
        public bool IsRequiringFrameLength { get; set; }
        public TestModeType TestmodeType { get; set; }
    }
}
