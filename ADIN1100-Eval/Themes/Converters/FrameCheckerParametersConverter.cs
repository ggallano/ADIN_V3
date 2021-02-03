// <copyright file="FrameCheckerParametersConverter.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>

namespace ADIN1100_Eval.Themes.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using static TargetInterface.FirmwareAPI;

    /// <summary>
    /// Convertor for paramters for GePhyLoopbackConfig
    /// </summary>
    public class FrameCheckerParametersConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FrameCheckerParameters parameters = new FrameCheckerParameters();

            if (values.Length == 5)
            {
                if (values[0] is double)
                {
                    parameters.FrameNumber = (uint)(double)values[0];
                }

                if (values[1] is double)
                {
                    parameters.FrameLength = (uint)(double)values[1];
                }

                if (values[2] is bool)
                {
                    parameters.EnableChecker = (bool)values[2];
                }

                if (values[3] is string)
                {
                    FrameType frameContent;
                    string text = (string)values[3];

                    text = text.Replace(" ", string.Empty);
                    if (Enum.TryParse(text, out frameContent))
                    {
                        parameters.FrameContent = frameContent;
                    }
                }

                if (values[4] is bool)
                {
                    parameters.EnableContinuous = (bool)values[4];
                }
            }

            return parameters;
        }

        /// <summary>
        /// Convert back
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
