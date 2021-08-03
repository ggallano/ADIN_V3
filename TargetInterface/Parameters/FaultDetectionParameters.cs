// <copyright file="FaultDetectionParameters.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2021 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace TargetInterface.Parameters
{
    using static FirmwareAPI;

    public class FaultDetectionParameters
    {
        /// <summary>
        /// gets or sets the cable type
        /// </summary>
        public string CableType { get; set; }

        /// <summary>
        /// gets or sets the cable length
        /// </summary>
        public float CableLength { get; set; }

        /// <summary>
        /// gets or sets type of calibration
        /// </summary>
        public Calibrate CalibrateType { get; set; }

        /// <summary>
        /// gets or sets the calculated NVP
        /// </summary>
        public float CalculatedNVP { get; set; }
    }
}
