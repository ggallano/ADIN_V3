// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models
{
    public class RegisterAccessModel
    {
        public string MemoryMap { get; set; }
        public string RegisterName { get; set; }
        public string RegisterAddress { get; set; }
        public string Value { get; set; }
    }
}
