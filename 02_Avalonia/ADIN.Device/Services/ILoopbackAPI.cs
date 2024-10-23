// <copyright file="ILoopbackAPI.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;

namespace ADIN.Device.Services
{
    public interface ILoopbackAPI
    {
        void SetLoopbackSetting(LoopbackModel loopback);
        void SetTxSuppression(bool isTxSuppressed);
        void SetRxSuppression(bool isRxSuppressed);
    }
}
