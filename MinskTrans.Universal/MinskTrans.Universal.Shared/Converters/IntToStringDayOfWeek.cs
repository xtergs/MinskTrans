using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    public class IntToStringDayOfWeek : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int val = (int) value;
            if (val == 7)
                val = 0;       
           return CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName((DayOfWeek)val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
