// <copyright file="IAdvertisedSpeedAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.Generic;

namespace ADIN.Device.Services
{
    public interface IAdvertisedSpeedAPI
    {
        void AdvertisedForcedSpeed(string advFrcSpd);
        void Speed100EEEAdvertisement(bool spd100EEEAdv_st);
        void Speed100FdAdvertisement(bool spd100FdAdv_st);
        void Speed100HdAdvertisement(bool spd100HdAdv_st);
        void Speed10FdAdvertisement(bool spd10FdAdv_st);
        void Speed10HdAdvertisement(bool spd10HdAdv_st);
        void LogAdvertisedSpeed(List<string> listAdvSpd);
        void SetForcedSpeed(string advFrcSpd);
    }
}
