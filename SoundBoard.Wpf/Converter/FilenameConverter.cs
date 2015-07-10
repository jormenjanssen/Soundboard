namespace SoundBoard.Wpf.Converter
{
    #region Namespaces

    using System;
    using System.Linq;
    using System.Windows.Data;

    #endregion

    public class FilenameConverter : IValueConverter
    {
        #region Private fields

        private readonly string[] _extentions = {".mp3", ".wav"};

        #endregion

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return _extentions.Aggregate(value.ToString(), (current, extention) => current.Replace(extention, ""));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}