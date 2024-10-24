// <copyright file="PhyModeADIN1320.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System.Collections.ObjectModel;

namespace ADIN.Device.Models.ADIN1320
{
    public class PhyModeADIN1320 : IPhyMode
    {
        public PhyModeADIN1320()
        {
            //ActivePhyMode = "Copper Media Only";
            //ActivePhyMode = "Fiber Media Only";
            //ActivePhyMode = "Media Converter";

            PhyModes = new ObservableCollection<string>()
            {
                "Copper Media Only",
                "Fiber Media Only",
                "Backplane",
                "Auto Media Detect_Cu",
                "Auto Media Detect_Fi",
                "Media Converter"
            };
            ActivePhyMode = PhyModes[0];

            MacInterface = "RMII";
        }
        public string ActivePhyMode { get; set; }
        public string MacInterface { get; set; }

        public ObservableCollection<string> PhyModes { get; set; }
    }
}
