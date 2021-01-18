//-----------------------------------------------------------------------
// <copyright file="RegisterValueConverter.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace ADIN1300_Eval.Themes.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Convert a register vaue to a string
    /// </summary>
    public class RegisterValueConverter : IValueConverter
    {
        /// <summary>
        /// Convert to a hex string
        /// </summary>
        /// <param name="value">The source string value</param>
        /// <param name="targetType">The type of the target value</param>
        /// <param name="parameter">The additional parameter to calculate the target value</param>
        /// <param name="culture">The culture of the caller element</param>
        /// <returns>Returns the Visible,if value has string "Deleted", else Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((parameter as string) == "16")
            {
                return string.Format("0x{0:X4}", value);
            }
            else
            {
                return string.Format("0x{0:X}", value);
            }
        }

        /// <summary>
        /// Convert from a hex string
        /// </summary>
        /// <param name="value">The source width value</param>
        /// <param name="targetType">The type of the target value</param>
        /// <param name="parameter">The additional parameter to calculate the target value</param>
        /// <param name="culture">The culture of the caller element</param>
        /// <returns>Returns error if called,but will not be called</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hexstring = (string)value;

            if (hexstring.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                hexstring = hexstring.Substring(2);
            }

            return uint.Parse(hexstring, NumberStyles.HexNumber, CultureInfo.CurrentCulture);
        }
    }
}
