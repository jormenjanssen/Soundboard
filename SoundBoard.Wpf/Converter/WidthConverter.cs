using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SoundBoard.Wpf.Converter
{
    public class WidthConverter : IValueConverter
    {
        private const int _width = 150;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var widthCount = ((value.ToString().Length/50) + 1);
            return (widthCount * _width) + (widthCount > 1 ? (9 * widthCount) : 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
