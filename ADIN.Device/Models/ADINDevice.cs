// <copyright file="ADINDevice.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADI.Register.Models;
using ADIN.Device.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ADIN.Device.Models
{
    public class ADINDevice
    {
        public ADINDevice(AbstractADINFactory device, bool isMultichipBoard = false)
        {
            Device = device;
            IsMultichipBoard = isMultichipBoard;
        }

        public List<string> AdvertisedSpeeds => Device.AdvertisedSpeeds;
        public string BoardName => Device.BoardName;
        public BoardRevision BoardRev => Device.BoardRev;
        public bool CableDiagOneTimePopUp { get; set; } = false;
        public string Checker { get; set; }
        public IClockPinControl ClockPinControl => Device.ClockPinControl;
        public AbstractADINFactory Device { get; set; }
        public IDeviceStatus DeviceStatus => Device.DeviceStatus;
        public BoardType DeviceType => Device.DeviceType;
        public IFrameGenChecker FrameGenChecker => Device.FrameGenChecker;
        public IFirmwareAPI FwAPI => Device.FirmwareAPI;
        public bool IsADIN1100CableDiagAvailable => TimeDomainReflectometryPort1 == null ? false : true;
        public bool IsADIN1300CableDiagAvailable => true;
        public bool IsMultichipBoard { get; set; }
        public ILinkProperties LinkProperties => Device.LinkProperties;
        public ILoopback Loopback => Device.Loopback;
        public uint PhyAddress => Device.PhyAddress;
        public uint PortNumber => Device.PortNumber;
        public ObservableCollection<RegisterModel> Registers => Device.Registers;
        public string SerialNumber => Device.SerialNumber;
        public ITestMode TestMode => Device.TestMode;
        public ITimeDomainReflectometry TimeDomainReflectometryPort1 => Device.TimeDomainReflectometryPort1;
        public ITimeDomainReflectometry TimeDomainReflectometryPort2 => Device.TimeDomainReflectometryPort2;
        public double MaxSlicerErrorPort1 { get; set; }
        public double SpikeCountPortPort1 { get; set; }
        public double MaxSlicerErrorPort2 { get; set; }
        public double SpikeCountPortPort2 { get; set; }

        /// <summary>
        /// Gets or sets ADIN1300/12000 Cable Diag.
        /// </summary>
        public List<string> CableDiagStatus { get; set; }

        /// <summary>
        /// Gets or sets ADIN1300/12000 Cable Diag.
        /// </summary>
        public bool IsCrossPair { get; set; } = true;
    }
}