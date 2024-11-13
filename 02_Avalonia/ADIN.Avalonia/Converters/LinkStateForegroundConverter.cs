// <copyright file="LinkStateForegroundConverter.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using ADIN.Helper.ForegroundColor;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;
using System;
using Avalonia.Styling;
using Tmds.DBus.Protocol;
using Avalonia;

namespace ADIN.Avalonia.Converters
{
    public class LinkStateForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
            {
                if ((value == null) || (value.ToString() == "") || (value.ToString() == "-"))
                    return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_info);

                switch ((EthPhyState)Enum.Parse(typeof(EthPhyState), value.ToString()))
                {
                    case EthPhyState.Powerdown:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_info);
                    case EthPhyState.Standby:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_warning);
                    case EthPhyState.LinkDown:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_danger);
                    case EthPhyState.LinkUp:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_success);
                    default:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.dark_system_info);
                }
            }
            else
            {
                if ((value == null) || (value.ToString() == "") || (value.ToString() == "-"))
                    return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_info);

                switch ((EthPhyState)Enum.Parse(typeof(EthPhyState), value.ToString()))
                {
                    case EthPhyState.Powerdown:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_info);
                    case EthPhyState.Standby:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_warning);
                    case EthPhyState.LinkDown:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_danger);
                    case EthPhyState.LinkUp:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_success);
                    default:
                        return new SolidColorBrush(AnalogDevices.Desktop.Harmonic.Resources.Colors.light_system_info);
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}