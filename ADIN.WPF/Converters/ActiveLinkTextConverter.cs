// <copyright file="ActiveLinkTextConverter.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;

namespace ADIN.WPF.Converters
{
    public class ActiveLinkTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEnable = (bool)value;

            if (isEnable)
            {
                return "Disable";
            }
            else
            {
                return "Enable";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}