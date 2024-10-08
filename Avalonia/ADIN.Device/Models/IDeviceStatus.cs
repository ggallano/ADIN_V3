// <copyright file="IDeviceStatus.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public interface IDeviceStatus
    {
        bool IsSoftwarePowerDown { get; set; }
        List<string> AdvertisedSpeeds { get; set; }
    }
}
