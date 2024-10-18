// <copyright file="TestModeADIN1100.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models.ADIN1100
{
    public class TestModeADIN1100 : ITestMode
    {
        public TestModeADIN1100()
        {
            TestModes = new List<TestModeListingModel>()
            {
                new TestModeListingModel()
                {
                    Name1 = "10BASE-T1L Normal Mode",
                    Name2 = "",
                    Description = "PHY is in normal mode",
                    IsRequiringFrameLength = false
                },
                new TestModeListingModel()
                {
                    Name1 = "10BASE-T1L Test Mode 1:",
                    Name2 = "Tx output voltage, Tx clock frequency and jitter.",
                    Description = "PHY repeatedly transmit the data symbol sequence (+1,-1)",
                    IsRequiringFrameLength = false
                },
                new TestModeListingModel()
                {
                    Name1 = "10BASE-T1L Test Mode 2:",
                    Name2 = "Tx output droop",
                    Description = "PHY Transmit ten '+1' symbols followed by ten '-1' symbols",
                    IsRequiringFrameLength = false
                },
                new TestModeListingModel()
                {
                    Name1 = "10BASE-T1L Test Mode 3:",
                    Name2 = "Power Spectral Density (PSD) and power level",
                    Description = "PHY transmit as in non-test operation and in the MASTER data mode with data set to normal Inter-Frame idle signals",
                    IsRequiringFrameLength = false
                },
                new TestModeListingModel()
                {
                    Name1 = "10BASE-T1L Transmit Disable:",
                    Name2 = "MDI Return Loss",
                    Description = "PHY's receive and transmit paths as in notmal operation but PHY transmits 0 symbols continuously",
                    IsRequiringFrameLength = false
                },
            };

            TestMode = TestModes[0];
        }
        public List<TestModeListingModel> TestModes { get; set; }
        public TestModeListingModel TestMode { get; set; }
        public uint TestModeFrameLength { get; set; }
    }
}
