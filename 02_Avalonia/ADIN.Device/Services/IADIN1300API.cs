// <copyright file="IADIN1300API.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Services
{
    public interface IADIN1300API : IFirmwareAPI, IMDIOAPI, IAdvertisedSpeedAPI, IAdvertisedGbSpeedAPI, IClockPinControlAPI, IDownSpeedAPI, IAutoMDIXAPI, IEnergyDetectAPI, ITestModeAPI, ILoopbackAPI, IMasterSlaveSettingsAPI, IFrameGenCheckerAPI, ICableDiagAPI, IValueUpdate
    {
    }
}
