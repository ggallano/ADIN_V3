// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace FTDIChip.Driver.Models
{
    public class FTDIDevice
    {
        public uint ID { get; set; }

        public string SerialNumber { get; set; }

        public string Description { get; set; }
    }
}
