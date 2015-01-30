using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    class StopsToStr:IValueConverter
    {
	    #region Implementation of IValueConverter

	    public object Convert(object value, Type targetType, object parameter, string language)
	    {
			var tempList = (IEnumerable<string>)value;
			StringBuilder returnStr = new StringBuilder();
			foreach (var o in tempList)
			{
				returnStr.Append(o);
				returnStr.Append(", ");
			}
			//	returnStr.Remove(returnStr.Length - 2, 2);
			return returnStr.ToString();
	    }

	    public object ConvertBack(object value, Type targetType, object parameter, string language)
	    {
		    throw new NotImplementedException();
	    }

	    #endregion
    }
}
