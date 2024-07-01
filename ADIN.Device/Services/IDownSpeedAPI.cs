// <copyright file="IDownSpeedAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Services
{
    public interface IDownSpeedAPI
    {
        void DownSpeed10Hd(bool dwnSpd10Hd);
        void DownSpeedRetriesSetVal(uint dwnSpdRtryVal);
    }
}
