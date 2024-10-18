// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Services
{
    public interface IADIN1200API : IFirmwareAPI, IMDIOAPI, IAdvertisedSpeedAPI, IClockPinControlAPI, IDownSpeedAPI, IAutoMDIXAPI, IEnergyDetectAPI, ITestModeAPI, ILoopbackAPI, IFrameGenCheckerAPI, ICableDiagAPI
    {
    }
}
