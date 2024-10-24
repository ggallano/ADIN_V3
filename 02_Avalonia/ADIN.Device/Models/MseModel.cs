// <copyright file="MseModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models
{
    public class MseModel
    {
        public MseModel(string displayString)
        {
            MseA_Raw = displayString;
            MseB_Raw = displayString;
            MseC_Raw = displayString;
            MseD_Raw = displayString;
            MseA_Max = displayString;
            MseB_Max = displayString;
            MseC_Max = displayString;
            MseD_Max = displayString;
            MseA_Combined = displayString;
            MseB_Combined = displayString;
            MseC_Combined = displayString;
            MseD_Combined = displayString;
            MseA_dB = displayString;
            MseB_dB = displayString;
            MseC_dB = displayString;
            MseD_dB = displayString;
        }
        public string MseA_Raw { get; set; }
        public string MseB_Raw { get; set; }
        public string MseC_Raw { get; set; }
        public string MseD_Raw { get; set; }
        public string MseA_Max { get; set; }
        public string MseB_Max { get; set; }
        public string MseC_Max { get; set; }
        public string MseD_Max { get; set; }
        public string MseA_Combined { get; set; }
        public string MseB_Combined { get; set; }
        public string MseC_Combined { get; set; }
        public string MseD_Combined { get; set; }
        public string MseA_dB { get; set; }
        public string MseB_dB { get; set; }
        public string MseC_dB { get; set; }
        public string MseD_dB { get; set; }
    }
}
