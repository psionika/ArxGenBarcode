using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ArxGenBarcode.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null && value is Bitmap)
            {
                var bitmap = (Bitmap)value;
                if(bitmap.Size.Height > 0 && bitmap.Size.Width > 0)
                {
                    BitmapImage answer = bitmap.ToWpfImage();

                    return answer;
                }
                    
            }            

            return new BitmapImage();
        }

        public object ConvertBack(
            object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
