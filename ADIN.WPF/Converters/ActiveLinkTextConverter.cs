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