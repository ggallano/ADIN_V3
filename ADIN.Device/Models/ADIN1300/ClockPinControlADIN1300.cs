// <copyright file="ClockPinControlADIN1300.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.Generic;

namespace ADIN.Device.Models.ADIN1300
{
    public class ClockPinControlADIN1300 : IClockPinControl
    {
        public ClockPinControlADIN1300()
        {
            GpClkPinControls = new List<string>()
            {
                "None",
                "125 MHz PHY Recovered",
                "125 MHz PHY Free Running",
                "Recovered HeartBeat",
                "Free Running HeartBeat",
                "25 MHz Reference"
            };
            GpClkPinControl = GpClkPinControls[0];
        }

        public List<string> GpClkPinControls { get; set; }
        public string GpClkPinControl { get; set; }
    }
}
