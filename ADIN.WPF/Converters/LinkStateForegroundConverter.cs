using ADIN.Device.Models;
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
                    return new SolidColorBrush(Colors.Black);

                case EthPhyState.Standby:
                    return new SolidColorBrush(Colors.OrangeRed);

                case EthPhyState.LinkDown:
                    return new SolidColorBrush(Colors.Red);

                case EthPhyState.LinkUp:
                    return new SolidColorBrush(Colors.Green);

                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}