// <copyright file="LinkStateForegroundConverter.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using ADIN.Device.Models;
using Helper.ForegroundColor;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ADIN.WPF.Converters
{
    public class LinkStateForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value.ToString() == "") || (value.ToString() == "-"))
                return new SolidColorBrush(Colors.Black);

            switch ((EthPhyState)Enum.Parse(typeof(EthPhyState), value.ToString()))
            {
                case EthPhyState.Powerdown:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom(ForegroundColorStyle.Black);
                case EthPhyState.Standby:
                    return (SolidColorBrush)new BrushConverter().ConvertFrom(ForegroundColorStyle.Orange);
                case EthPhyState.LinkDown:
                    return (SolidColorBrush) new BrushConverter().ConvertFrom(ForegroundColorStyle.Red);
                case EthPhyState.LinkUp:
                    return (SolidColorBrush) new BrushConverter().ConvertFrom(ForegroundColorStyle.Green);
                default:
                    return (SolidColorBrush) new BrushConverter().ConvertFrom(ForegroundColorStyle.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}