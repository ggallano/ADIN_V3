using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ADIN.WPF.Converters
{
    public class BooleanVisibilityConverter : IValueConverter
    {
        public bool IsInversed { get; set; }
        public bool IsHidden { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var val = System.Convert.ToBoolean(value);
                if (IsInversed)
                {
                    val = !val;
                }

                if (val)
                {
                    return Visibility.Visible;
                }

                if (IsHidden)
                    return Visibility.Hidden;

                return Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (((Visibility)value).Equals(Visibility.Collapsed)) return false;
                else return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
