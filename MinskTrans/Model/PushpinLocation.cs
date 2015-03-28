using System.Windows;
using MapControl;
#if WINDOWS_PHONE_APP
using Windows.UI.Xaml;
#else
#endif

namespace MinskTrans.DesctopClient.Model
{
	public class PushpinLocation
	{
		public PushpinLocation() { }

		public PushpinLocation(Pushpin pin, Location loc)
		{
			Pushpin = pin;
			Location = loc;
		}

		public Stop Stop;
		public Style Style;
		private Pushpin pushpin;

		public PushpinLocation(Pushpin pin, bool getLocation = true)
		{
			Pushpin = pin;
			Location = MapPanel.GetLocation(pin);
		}

		public Pushpin Pushpin
		{
			get
			{
				if (pushpin == null)
				{
					pushpin = new Pushpin(){Tag = Stop, Content = Stop.Name};
					pushpin.Style = Style;
					MapPanel.SetLocation(pushpin, Location);
				}
				return pushpin;
			}
			set { pushpin = value; }
		}

		public Location Location { get; set; }
	}
}
