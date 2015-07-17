using MapControl;
using MinskTrans.Context.Base.BaseModel;
#if WINDOWS_PHONE_APP || WINDOWS_UAP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MinskTrans.DesctopClient.Model
{
	public abstract class PushpinLocationBase
	{
		public PushpinLocationBase() { }

		public PushpinLocationBase(Pushpin pin, Location loc)
		{
			Pushpin = pin;
			Location = loc;
		}

		public Stop Stop;
		public Style Style;
		private Pushpin pushpin;

		public PushpinLocationBase(Pushpin pin, bool getLocation = true)
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
					//pushpin.Style = Style;
					MapPanel.SetLocation(pushpin, Location);
				}
				return pushpin;
			}
			set { pushpin = value; }
		}

		public Location Location { get; set; }
	}
}