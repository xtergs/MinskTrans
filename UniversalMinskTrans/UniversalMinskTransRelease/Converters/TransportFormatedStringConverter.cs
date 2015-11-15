using System;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    public class TransportFormatedStringConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string str = (string) value;
            if (str.Length == 1)
                str = str.PadLeft(5);
            if (str.Length == 2)
                str = str.PadLeft(4);
            if (str.Length == 3)
                str = str.PadLeft(3);
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
