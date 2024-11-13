// <copyright file="ColorToBrushConverter.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Helper.ForegroundColor;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;
using System;
using Avalonia;
using Avalonia.Styling;

namespace ADIN.Avalonia.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        /// <summary>
        /// This method calculates color based on string message passed
        /// </summary>
        /// <param name="value">The source tab control</param>
        /// <param name="targetType">The type of the target value</param>
        /// <param name="parameter">The additional parameter, is not used</param>
        /// <param name="culture">The culture of the caller element</param>
        /// <returns>Returns the division of the width for each tab item</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_info); ;

            var message = value.ToString();

            if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
            {
                if (message.Contains("[Error]") || message.Contains("Failed"))
                    return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_danger);

                if (message.Contains("[Warning]"))
                    return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_warning);

                if (message.Contains("[VerboseInfo]") || message.Contains("Success"))
                    return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_success);

                return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_info);
            }
            else
            {
                if (message.Contains("[Error]") || message.Contains("Failed"))
                    return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_danger);

                if (message.Contains("[Warning]"))
                    return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_warning);

                if (message.Contains("[VerboseInfo]") || message.Contains("Success"))
                    return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_success);

                return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_info);
            }
        }

        /// <summary>
        /// This method is not implemented, because it will not be used
        /// </summary>
        /// <param name="value">The source width value</param>
        /// <param name="targetType">The types of the target value</param>
        /// <param name="parameter">The additional parameter to calculate the target value</param>
        /// <param name="culture">The culture of the caller element</param>
        /// <returns>Returns error if called,but will not be called</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}