// <copyright file="MSEForegroundConverter.cs" company="Analog Devices Inc.">
//     Copyright (c) 2024 Analog Devices Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices Inc. and its licensors.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ADIN.WPF.Converters
{
    public class MSEForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string mse = ((string)value).Trim();

            if (mse.Contains("N/A") /*|| mse.Contains("-")*/ || mse.Contains("∞") /*|| mse.Contains("")*/)
                return (SolidColorBrush)(new BrushConverter().ConvertFrom("#C81A28"));

            mse = mse.Replace("dB", "").Trim();
            try
            {
                var val = float.Parse(mse);

                if (val < -21)
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#2E9E6F"));
                else if (val < -19 && val >= -21)
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#E76423"));
                else /*if (val >= -19)*/
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom("#C81A28"));
            }
            catch (Exception ex)
            {
                return (SolidColorBrush)(new BrushConverter().ConvertFrom("#C81A28"));
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}