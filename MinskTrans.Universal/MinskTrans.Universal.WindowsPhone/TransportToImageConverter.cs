﻿using System;
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
			
			TransportType strValue = (MinskTrans.DesctopClient.TransportType)value;
			switch (strValue)
			{
				case TransportType.Trol:
					return "Image/trol.jpg";
				case TransportType.Bus:
					return "Image/bus.jpg";
				case TransportType.Tram:
					return "Image/tram.jpg";
				case TransportType.Metro:
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