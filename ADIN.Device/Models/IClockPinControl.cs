// <copyright file="IClockPinControl.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.Generic;

namespace ADIN.Device.Models
{
    public interface IClockPinControl
    {
        List<string> GpClkPinControls { get; set; }
        string GpClkPinControl { get; set; }
    }
}