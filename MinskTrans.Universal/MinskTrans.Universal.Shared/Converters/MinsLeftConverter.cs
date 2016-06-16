using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using MinskTrans.DesctopClient;
using MyLibrary;

namespace MinskTrans.Universal
{
	class MinsLeftConverter:IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, string language)
		{
		    int mins = 0;
            if (value is int)
			    mins = (int)value;
		    if (value is TimeSpan)
		        mins = (int)((TimeSpan) value).TotalMinutes;
			mins -= DateTime.Now.Hour * 60 + DateTime.Now.Minute;
			if (mins == 0)
				return "прибывает";
			if (mins >= 60)
				return(mins / 60) + " " + Tools.HourToStr(mins / 60) + " " + (mins - (mins / 60) * 60) + " " + Tools.MinsToStr(mins - (mins / 60) * 60);
			return mins  + " " +  Tools.MinsToStr(mins);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

    public class MinsLeftFromTimeSpanConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var mins = (int)((TimeSpan)value).TotalMinutes;
            mins -= DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (mins == 0)
                return "прибывает";
            if (mins >= 60)
                return (mins / 60) + " " + Tools.HourToStr(mins / 60) + " " + (mins - (mins / 60) * 60) + " " + Tools.MinsToStr(mins - (mins / 60) * 60);
            return mins + " " + Tools.MinsToStr(mins);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
