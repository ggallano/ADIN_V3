// <copyright file="RegisterParametersConverter.cs" company="Analog Devices, Inc.">
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
    public class RegisterParametersConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convert the value
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            RegisterParameters parameters = new RegisterParameters();

            if (values.Length > 0)
            {
                if (values[0] is string)
                {
                    string text = (string)values[0];

                    try
                    {
                        parameters.RegisterAddress = uint.Parse(text, System.Globalization.NumberStyles.HexNumber);
                        //if (parameters.RegisterAddress >= 32)
                        //{
                        //    parameters.RegisterAddress = (30 << 16) | parameters.RegisterAddress;
                        //}
                    }
                    catch (FormatException)
                    {
                    }
                }
            }

            if (values.Length > 1)
            {
                if (values[1] is string)
                {
                    string text = (string)values[1];
                    try
                    {
                        parameters.RegisterValue = uint.Parse(text, System.Globalization.NumberStyles.HexNumber);
                    }
                    catch (FormatException)
                    {
                    }
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
