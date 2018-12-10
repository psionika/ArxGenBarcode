using System;
using System.Globalization;
using System.Windows.Data;

namespace ArxGenBarcode.Converters
{
    public class ItemsCountToIsEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value as int?;
            if (boolValue != null)
            {
                return boolValue > 0 ;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int?)) return true;

            if ((int?)value > 0)
            {
               return true;
            }
            return false;
        }
    }
}
