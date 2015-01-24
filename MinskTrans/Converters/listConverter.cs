using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace MinskTrans.DesctopClient
{
	public class ListIntToStrConverter : IValueConverter
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
			string str = "";
			var list = (List<int>) value;
			foreach (int time in list)
			{
				str += time.ToString("00") + ", ";
			}
			return str;
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
			return new List<int>();
		}

		#endregion
	}
}