using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    class BoolToOpacityConverter : IValueConverter
	{
		public double Opacity { get; set; }
		public double NormalOpacity { get; set; }

	    public object Convert(object value, Type targetType, object parameter, string language)
	    {
		    if (!(value is bool))
			    return value;
			var val = (bool)value;
		    if (val)
			    return Opacity;
		    return NormalOpacity;
	    }

	    public object ConvertBack(object value, Type targetType, object parameter, string language)
	    {
		    throw new NotImplementedException();
	    }
	}
}
