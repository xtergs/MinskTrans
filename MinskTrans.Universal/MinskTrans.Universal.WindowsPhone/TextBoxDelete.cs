using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
