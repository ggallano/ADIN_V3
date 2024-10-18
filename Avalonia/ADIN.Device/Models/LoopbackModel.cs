// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models
{
    public class LoopbackModel
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public LoopBackMode EnumLoopbackType { get; set; }
        public List<string> DisabledModes { get; set; }
    }
}
