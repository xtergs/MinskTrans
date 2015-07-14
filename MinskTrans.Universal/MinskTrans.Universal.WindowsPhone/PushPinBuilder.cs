using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using MapControl;

namespace MyLibrary
{
	public class PushPinBuilder
	{
		public Style Style { get; set; }

		public TappedEventHandler Tapped { get; set; }

		public Pushpin CreatePushPin(Location location)
		{
			var tempPush =  new Pushpin()
			{
				Style = Style,
			};
			MapPanel.SetLocation(tempPush, location);
			tempPush.Tapped += Tapped;
			return tempPush;
		}
	}
}
