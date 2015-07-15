﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Converters
{
    class IntToVisibleConverter:IValueConverter
    {
	    #region Implementation of IValueConverter

	    public object Convert(object value, Type targetType, object parameter, string language)
	    {
		    int val = (int) value;
		    if (val == 0)
			    return Visibility.Collapsed;
		    return Visibility.Visible;
	    }

	    public object ConvertBack(object value, Type targetType, object parameter, string language)
	    {
		    throw new NotImplementedException();
	    }

	    #endregion
    }
}
