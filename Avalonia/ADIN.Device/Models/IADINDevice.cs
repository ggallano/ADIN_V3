// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Services;

namespace ADIN.Device.Models
{
    public interface IADINDevice
    {
        string BoardName { get; set; }
        string SerialNumber { get; set; }
        IFirmwareAPI FirmwareAPI { get; set; }
        public string ActivePhyMode { get; set; }
        public string MacInterface { get; set; }
    }
}
