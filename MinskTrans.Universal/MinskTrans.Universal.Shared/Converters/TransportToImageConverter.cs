using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    class TransportToImageConverter: IValueConverter
    {
	    #region Implementation of IValueConverter

	    public object Convert(object value, Type targetType, object parameter, string language)
	    {
		    string strValue = (string) value;
			switch (strValue)
			{
				case "trol":
					return "Image/trol.jpg";
				case "bus":
					return "Image/bus.jpg";
				case "tram":
					return "Image/tram.jpg";
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
