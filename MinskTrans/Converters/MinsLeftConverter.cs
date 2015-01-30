using System;
using System.Globalization;
using System.Windows.Data;

namespace MinskTrans.DesctopClient.Converters
{
	internal class MinsLeftConverter : IValueConverter
	{
		#region Implementation of IValueConverter

		/// <summary>
		///     Converts a value.
		/// </summary>
		/// <returns>
		///     A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var mins = (int)value;
			mins -= DateTime.Now.Hour * 60 + DateTime.Now.Minute;
			if (mins == 0)
				return "прибывает";
			if (mins >= 60)
				return (mins / 60) + " " + Tools.HourToStr(mins / 60) + " " + (mins - (mins / 60) * 60) + " " + Tools.MinsToStr(mins - (mins / 60) * 60);
			return mins + " " + Tools.MinsToStr(mins);
		}

		/// <summary>
		///     Converts a value.
		/// </summary>
		/// <returns>
		///     A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}