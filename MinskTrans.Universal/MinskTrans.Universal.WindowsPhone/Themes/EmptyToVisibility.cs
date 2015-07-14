using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MinskTrans.Universal.Themes
{
	class EmptyToVisibility:IValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (String.IsNullOrEmpty((string) value))
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
