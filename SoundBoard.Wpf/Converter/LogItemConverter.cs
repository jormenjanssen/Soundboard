using System;
using System.Globalization;
using System.Windows.Data;
using SoundBoard.Data;

namespace SoundBoard.Wpf.Converter
{
    public class LogItemConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (QueueLogInfo) value;

            if (item == null) return "";
            return $"{item.QueueTimestamp.ToString("hh:mm:ss")} : '{item.QueuedBy}' played '{item.SampleName.Replace(".mp3", "")}'";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}