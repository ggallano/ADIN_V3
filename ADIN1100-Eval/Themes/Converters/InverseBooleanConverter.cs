//-----------------------------------------------------------------------
// <copyright file="InverseBooleanConverter.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2018 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace ADIN1100_Eval.Themes.Converters
{
    using System;
    using System.Windows.Data;

    /// <summary>
    /// Inverts a boolean
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter,    System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return !(bool)value;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
