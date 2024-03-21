using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ADIN.WPF.Converters
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
            var message = value.ToString();
            if (message.Contains("[Error]"))
            {
                return new SolidColorBrush(Colors.Red);
            }

            if (message.Contains("[Warning]"))
            {
                return new SolidColorBrush(Colors.Blue);
            }

            if (message.Contains("[VerboseInfo]"))
            {
                return new SolidColorBrush(Colors.Green);
            }

            return new SolidColorBrush(Colors.Black);
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