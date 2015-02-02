using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using MinskTrans.DesctopClient;

namespace MinskTrans.Universal.Converters
{
	class TransportToColor:IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			Rout.TransportType type = (Rout.TransportType) value;
			if (type == Rout.TransportType.Bus)
				return Application.Current.Resources["GreenSolidColorBrush"];
			if (type == Rout.TransportType.Trol)
				return Application.Current.Resources["BlueSolidColorBrush"];
			if (type==Rout.TransportType.Tram)
				return Application.Current.Resources["RedSolidColorBrush"];
			return Application.Current.Resources["VioletSolidColorBrush"];
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
