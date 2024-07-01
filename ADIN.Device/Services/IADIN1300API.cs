// <copyright file="IADIN1300API.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Services
{
    public interface IADIN1300API : IFirmwareAPI, IMDIOAPI, IAdvertisedSpeedAPI, IClockPinControlAPI, IDownSpeedAPI, IAutoMDIXAPI, IEnergyDetectAPI, ITestModeAPI, ILoopbackAPI, IFrameGenCheckerAPI
    {
        void DownSpeed100Hd(bool dwnSpd100Hd);
        void Speed1000EEEAdvertisement(bool spd1000EEEAdv_st);
        void Speed1000FdAdvertisement(bool spd1000FdAdv_st);
        void Speed1000HdAdvertisement(bool spd1000HdAdv_st);
    }
}
