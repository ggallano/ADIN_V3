// <copyright file="AbstractADINFactory.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADI.Register.Models;
using ADIN.Device.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADIN.Device.Models
{
    public abstract class AbstractADINFactory
    {
        public string BoardName { get; set; }
        public string SerialNumber { get; set; }
        public uint PhyAddress { get; set; }
        public List<string> AdvertisedSpeeds { get; set; }
        public BoardType DeviceType { get; set; }
        public BoardRevision BoardRev { get; set; }
        public abstract IFirmwareAPI FirmwareAPI { get; set; }
        public ILinkProperties LinkProperties { get; set; }
        public ILoopback Loopback { get; set; }
        public IFrameGenChecker FrameGenChecker { get; set; }
        public IClockPinControl ClockPinControl { get; set; }
        public ITestMode TestMode { get; set; }
        public ObservableCollection<RegisterModel> Registers { get; set; }
        public IDeviceStatus DeviceStatus { get; set; }
        public uint PortNum { get; set; }

        /// <summary>
        /// Gets or sets ADIN1100 cable diag status results port 1.
        /// </summary>
        public ITimeDomainReflectometry TimeDomainReflectometry { get; set; }
    }
}
