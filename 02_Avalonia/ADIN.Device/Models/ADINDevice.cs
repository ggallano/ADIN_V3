// <copyright file="ADINDevice.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Services;
using ADIN.Register.Models;
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
        public string CableLength { get; set; } = "-";
        public string Checker { get; set; }
        public string CheckerError { get; set; }
        public IClockPinControl ClockPinControl => Device.ClockPinControl;
        public AbstractADINFactory Device { get; set; }
        public IDeviceStatus DeviceStatus => Device.DeviceStatus;
        public BoardType DeviceType => Device.DeviceType;
        public IFrameGenChecker FrameGenChecker => Device.FrameGenChecker;
        public IFirmwareAPI FwAPI => Device.FirmwareAPI;
        public bool IsADIN1100CableDiagAvailable => TimeDomainReflectometry == null ? false : true;
        public bool IsADIN1300CableDiagAvailable => true;
        public bool IsMultichipBoard { get; set; }
        public ILinkProperties LinkProperties => Device.LinkProperties;
        public ILoopback Loopback => Device.Loopback;
        public int PhyAddress => Device.PhyAddress;
        public IPhyMode PhyMode => Device.PhyMode;
        public int PortNumber => Device.PortNum;
        public ObservableCollection<RegisterModel> Registers => Device.Registers;
        public string SerialNumber => Device.SerialNumber;
        public ITestMode TestMode => Device.TestMode;
        public ITimeDomainReflectometry TimeDomainReflectometry => Device.TimeDomainReflectometry;
        public double MaxSlicerError { get; set; }
        public double SpikeCountPort { get; set; }

        /// <summary>
        /// Gets or sets ADIN1300/12000 Cable Diag.
        /// </summary>
        public List<string> CableDiagStatus { get; set; }

        /// <summary>
        /// Gets or sets ADIN1300/12000 Cable Diag.
        /// </summary>
        public bool IsCrossPair { get; set; } = true;


        public bool AllowInput { get; set; } = false;
    }
}
