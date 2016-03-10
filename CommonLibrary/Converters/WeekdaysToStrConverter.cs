using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    class WeekdaysToStrConverter:IValueConverter
    {

		public Dictionary<int, string> daysToString = new Dictionary<int, string>()
		{
			{1, "Пн"},
			{2, "Вт"},
			{3, "Ср"},
			{4, "Чт"},
			{5, "Пт"},
			{6, "Сб"},
			{7, "Вс"}
		};

		
	    #region Implementation of IValueConverter

	    public object Convert(object value, Type targetType, object parameter, string language)
	    {
		    if ((string) value == "12345")
			    return "Будние дни";
		    if ((string) value == "67")
			    return "Выходные дни";
		    if ((string) value == "1234567")
			    return "Все дни";
			var strBuilder = new StringBuilder();
			foreach (var day in (string)value)
			{
				strBuilder.Append(daysToString[int.Parse(day.ToString())] + " ");
			}
			return strBuilder.ToString();
	    }

	    public object ConvertBack(object value, Type targetType, object parameter, string language)
	    {
		    throw new NotImplementedException();
	    }

	    #endregion
    }
}
