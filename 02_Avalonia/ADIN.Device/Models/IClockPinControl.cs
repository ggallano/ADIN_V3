// <copyright file="IClockPinControl.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models
{
    public interface IClockPinControl
    {
        List<string> GpClkPinControls { get; set; }
        List<string> Clk25RefPinControls { get; set; }
        string GpClkPinControl { get; set; }
        string Clk25RefPnCtrl { get; set; }
    }
}
