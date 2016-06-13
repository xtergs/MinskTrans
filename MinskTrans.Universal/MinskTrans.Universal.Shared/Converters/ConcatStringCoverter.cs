using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    public class ConcatStringCoverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{(string) parameter} {(string) value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
