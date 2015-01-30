using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal
{
	class MinsToTimeConverter:IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var mins = (int)value;
			int hour = mins / 60;
			if (hour >= 24)
				hour -= 24;
			return hour.ToString("00") + ":" + (mins - (mins / 60) * 60).ToString("00");
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
