// <copyright file="DeviceListingItemViewModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;

namespace ADIN.WPF.ViewModel
{
    public class DeviceListingItemViewModel : ViewModelBase
    {
        public DeviceListingItemViewModel(ADINDevice device)
        {
            Device = device;
            ImagePath = @"..\Images\icons\Applications-Industrial-Automation-Ethernet-Icon.png";
        }

        public ADINDevice Device { get; }

        public string ImagePath { get; }
        public string Name => Device.Device.BoardName;
        public string SerialNumber => Device.Device.SerialNumber;
        public bool IsMultichipBoard => Device.IsMultichipBoard;
        public BoardType BoardType => Device.DeviceType;

        public string DeviceHeader
        {
            get
            {
                if (IsMultichipBoard)
                    return SerialNumber + " - " + BoardType.ToString();
                else
                    return SerialNumber;
            }
        }
    }
}