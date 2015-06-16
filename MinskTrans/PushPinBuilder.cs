using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_PHONE_APP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#else
using Style = System.Windows.Style;
#endif
using MapControl;

namespace MyLibrary
{
	public class PushPinBuilder
	{
		public Style Style { get; set; }
#if WINDOWS_PHONE_APP

		public TappedEventHandler Tapped { get; set; }
#endif
		public Pushpin CreatePushPin(Location location)
		{
			var tempPush =  new Pushpin()
			{
				Style = Style,
			};
			MapPanel.SetLocation(tempPush, location);
#if WINDOWS_PHONE_APP
			tempPush.Tapped += Tapped;
#endif
			return tempPush;
		}
	}
}
