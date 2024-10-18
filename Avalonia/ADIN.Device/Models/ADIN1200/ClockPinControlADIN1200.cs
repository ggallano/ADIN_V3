// <copyright file="FeedbackModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models.ADIN1200
{
    public class ClockPinControlADIN1200 : IClockPinControl
    {
        public ClockPinControlADIN1200()
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

            Clk25RefPinControls = new List<string>()
            {
                "None",
                "25 MHz Reference"
            };
            Clk25RefPnCtrl = Clk25RefPinControls[0];
        }

        public List<string> Clk25RefPinControls { get; set; }
        public string Clk25RefPnCtrl { get; set; }
        public string GpClkPinControl { get; set; }
        public List<string> GpClkPinControls { get; set; }
    }
}