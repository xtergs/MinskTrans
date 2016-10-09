using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using MapControl;

namespace MyLibrary
{
	public class PushPinBuilder
	{
		public Style Style { get; set; }
        public Style IStyle { get; set; }
        public Style StartPositon { get; set; }
        public Style EndPosition { get; set; }

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

	    public Pushpin CreateIPushPin(Location location)
	    {
            var tempPush = new Pushpin()
            {
                Style = IStyle,
            };
            MapPanel.SetLocation(tempPush, location);
            return tempPush;
        }
	}
}
