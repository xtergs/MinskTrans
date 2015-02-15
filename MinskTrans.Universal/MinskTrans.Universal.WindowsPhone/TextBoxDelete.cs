using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace MinskTrans.Universal
{
	public sealed class TextBoxDelete : TextBox
	{
		public TextBoxDelete()
		{
			this.DefaultStyleKey = typeof(TextBoxDelete);
			
		}

		private void ClearButton(object sender, RoutedEventArgs e)
		{
			Text = String.Empty;
		}
	}
}
