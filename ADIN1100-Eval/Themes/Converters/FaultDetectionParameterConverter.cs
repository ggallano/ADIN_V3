// <copyright file="FaultDetectionParameterConverter.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2021 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval.Themes.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using TargetInterface.Parameters;
    using static TargetInterface.FirmwareAPI;

    public class FaultDetectionParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            FaultDetectionParameters parameters = new FaultDetectionParameters();

            parameters.CableType = values[0].ToString();
            parameters.CableLength = float.Parse(values[1].ToString());
            parameters.CalibrateType = (Calibrate)Enum.Parse(typeof(Calibrate), values[2].ToString());
            parameters.CalculatedNVP = float.Parse(values[3].ToString());
            return parameters;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
