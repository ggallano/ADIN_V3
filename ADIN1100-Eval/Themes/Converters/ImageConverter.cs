//-----------------------------------------------------------------------
// <copyright file="ImageConverter.cs" company="Analog Devices, Inc.">
//     Copyright (c) 2021 Analog Devices, Inc. All Rights Reserved.
//     This software is proprietary and confidential to Analog Devices, Inc. and its licensors.
// </copyright>
//-----------------------------------------------------------------------

namespace ADIN1100_Eval.Themes.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;
    using Telerik.Windows.Controls;

    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fileName = string.Empty;
            RadComboBoxItem selectedLoopback = value as RadComboBoxItem;
            try
            {
                switch ((string)selectedLoopback.Content)
                {
                    case "None":
                        fileName = "NoLoopback";
                        break;
                    case "Digital":
                        fileName = "DigitalLoopback";
                        break;
                    case "Line Driver":
                        fileName = "DriverLoopback";
                        break;
                    case "External Cable":
                        fileName = "ExternalLoopback";
                        break;
                    case "Remote":
                        fileName = "RemoteLoopback";
                        break;
                    default:
                        fileName = "NoLoopback";
                        break;
                }

                Uri imguri = new Uri(string.Format(@"../Images/loopback/{0}.png", fileName), UriKind.Relative);
                return new BitmapImage(imguri);
            }
            catch (Exception ex)
            {
                return null; // or some default image
            }
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
