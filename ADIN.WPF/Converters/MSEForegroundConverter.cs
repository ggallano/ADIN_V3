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

            if (mse.Contains("N/A") || (mse.Contains("-") && !mse.Contains("dB")) || mse.Contains("∞") /*|| mse.Contains("")*/)
                return new SolidColorBrush(Colors.Red);

            if (mse.Contains("dB"))
            {
                mse = mse.Replace("dB", "").Trim();
                try
                {
                    var val = float.Parse(mse);

                    if (val < -21)
                        return new SolidColorBrush(Colors.Green);
                    else if (val < -19 && val >= -21)
                        return new SolidColorBrush(Colors.Yellow);
                    else /*if (val >= -19)*/
                        return new SolidColorBrush(Colors.Red);
                }
                catch (Exception ex)
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }

            return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}