namespace SoundBoard.AutoUpdate.Converters
{
    #region Namespaces
    using System.Windows.Data;
    using System;
    using System.Globalization;
    using System.Windows;
    #endregion

    public class BytesToHumanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert the value to long value
            if (!(value is long))
                return DependencyProperty.UnsetValue;

            // Determine the length
            var length = (long)value;
            if (length < 1024L * 1024)
                return string.Format("{0:N0} bytes", length);
            if (length < 1024L * 1024 * 1024)
                return string.Format("{0:N0} KB", length / 1024);
            return string.Format("{0:N0} MB", length / (1024 * 1024));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not supported
            throw new NotSupportedException();
        }

        #endregion
    }
}