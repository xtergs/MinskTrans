using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
using System.Windows.Controls;
#else
using Windows.UI.Xaml;
#endif


namespace MinskTrans.DesctopClient
{
	public static class FrameworkElementExt
	{
		public static void BringToFront(this FrameworkElement element)
		{
			if (element == null) return;

#if !WINDOWS_PHONE_APP && !WINDOWS_AP
			Panel parent = element.Parent as Panel;
			if (parent == null) return;

			var maxZ = parent.Children.OfType<UIElement>()
			  .Where(x => x != element)
			  .Select(x => Panel.GetZIndex(x))
			  .Max();
			Panel.SetZIndex(element, maxZ + 1);
#endif
		}
	}
}
