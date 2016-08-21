﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ExpanderControl
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (bool)value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return (Visibility) value == Visibility.Visible;
		}
	}

	public class BooleanToNotVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (bool)value ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return (Visibility)value != Visibility.Visible;
		}
	}
}