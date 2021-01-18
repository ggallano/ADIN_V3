//-----------------------------------------------------------------------
// <copyright file="BooleanToVisibilityConverter.cs" company="Analog Devices, Inc.">
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
    /// This converts boolean value to visibility object
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether conversion logic is reversed
        /// </summary>
        public bool IsInversed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide or collapse (default)
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// This method returns visibility based on the value passed and the IsInversed boolean
        /// </summary>
        /// <param name="value">The source string value</param>
        /// <param name="targetType">The type of the target value</param>
        /// <param name="parameter">The additional parameter to calculate the target value</param>
        /// <param name="culture">The culture of the caller element</param>
        /// <returns>Returns the Visible,if value is True and IsInversed is False or vice versa, else Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value);
            if (this.IsInversed)
            {
                val = !val;
            }

            if (val)
            {
                return Visibility.Visible;
            }

            if (this.IsHidden)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// This method is not implemented, because it will not be used
        /// </summary>
        /// <param name="value">The source visibility value</param>
        /// <param name="targetType">The type of the target value</param>
        /// <param name="parameter">The additional parameter to calculate the target value</param>
        /// <param name="culture">The culture of the caller element</param>
        /// <returns>Returns error if called,but will not be called</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
