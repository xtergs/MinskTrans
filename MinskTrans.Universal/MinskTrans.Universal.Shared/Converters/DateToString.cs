using System;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    public class DateToString : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is DateTime))
                throw new ArgumentException("Type is not complied");
            DateTime val = (DateTime) value;
            if (val == default(DateTime))
                return "За все время"; 
            if (val.Date == DateTime.Now.Date)
                return "Сегодня";
            if (val.Date == DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).Date)
                return "Вчера";
            var data =  val.Date.ToString("M");
            return data;
        } 

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
