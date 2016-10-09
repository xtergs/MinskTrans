using System;
using System.Windows.Input;
using MapControl;
#if WINDOWS_PHONE_APP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#else
using Style = System.Windows.Style;
#endif

namespace MinskTrans.DesctopClient
{
	public class PushPinBuilder
	{
		public Style Style { get; set; }
#if WINDOWS_PHONE_APP

		public TappedEventHandler Tapped { get; set; }
#else
		public MouseButtonEventHandler Tapped { get; set; }
	    public Style StartPositon { get; set; }
	    public Style EndPosition { get; set; }
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
#else
			tempPush.MouseRightButtonUp += Tapped;
#endif
			return tempPush;
		}

	    public Pushpin CreateIPushPin(Location location)
	    {
	        throw new NotImplementedException();
	    }
	}
}
