// <copyright file="DeviceStatusADIN1300.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models.ADIN1300
{
    public class DeviceStatusADIN1300 : IDeviceStatus
    {
        public bool IsSoftwarePowerDown { get; set; } = true;
        public List<string> AdvertisedSpeeds { get; set; }
    }
}
