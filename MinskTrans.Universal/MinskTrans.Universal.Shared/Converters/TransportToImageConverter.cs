using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using MinskTrans.DesctopClient;

namespace MinskTrans.Universal.Converters
{
	class TransportToImageConverter: IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			MinskTrans.DesctopClient.Rout.TransportType strValue = (MinskTrans.DesctopClient.Rout.TransportType)value;
			switch (strValue)
			{
				case Rout.TransportType.Trol:
					return "Image/trol.jpg";
				case Rout.TransportType.Bus:
					return "Image/bus.jpg";
				case Rout.TransportType.Tram:
					return "Image/tram.jpg";
				case Rout.TransportType.Metro:
					return "Image/subway.jpg";
			}
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
