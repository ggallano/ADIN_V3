// <copyright file="TDRModel.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

namespace ADIN.Device.Models
{
    public class TDRModel
    {
        public decimal NVP { get; set; } = 0.67M;
        public decimal CableOffset { get; set; } = 80.00M;
        public decimal Coeff0 { get; set; } = 0.754M;
        public decimal Coeff1 { get; set; } = 1.003M;
        public FaultType Fault { get; set; } = FaultType.None;
        public CalibrationMode Mode { get; set; } = CalibrationMode.AutoRange;
    }
}
