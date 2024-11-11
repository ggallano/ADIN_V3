// <copyright file="IValueUpdate.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADIN.Device.Services
{
    public interface IValueUpdate
    {
        uint GetLinkProp_DownspeedRetries();
        bool GetLinkProp_IsAdv1000BaseTFd();
        bool GetLinkProp_IsAdv1000BaseTHd();
        bool GetLinkProp_IsAdv100BaseTxFd();
        bool GetLinkProp_IsAdv100BaseTxHd();
        bool GetLinkProp_IsAdv10BaseTFd();
        bool GetLinkProp_IsAdv10BaseTHd();
        bool GetLinkProp_IsAdvEee1000();
        bool GetLinkProp_IsAdvEee100();
        bool GetLinkProp_IsDwnspd100TxHd();
        bool GetLinkProp_IsDwnspd10THd();
        string GetLinkProp_SpeedMode();
        string GetLinkProp_EDPD();
        string GetLinkProp_ForcedSpeed();
        string GetLinkProp_MDIX();
        string GetLinkProp_LeadFollow();

        LoopBackMode GetLoopback_Loopback();
        bool GetLoopback_TxSupp();
        bool GetLoopback_RxSupp();

        bool GetFrameGen_EnContMode();
        uint GetFrameGen_FrameBurst();
        uint GetFrameGen_FrameLength();
        FrameType GetFrameGen_FrameContent();
    }
}
