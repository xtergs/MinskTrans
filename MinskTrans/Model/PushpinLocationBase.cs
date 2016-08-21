using System.Collections.Generic;
using System.Windows.Controls;
using MapControl;
using MinskTrans.Context.Base.BaseModel;
#if WINDOWS_PHONE_APP || WINDOWS_UAP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace MinskTrans.DesctopClient.Model
{

	public class MapPolylineEx : MapPolyline
	{
		public Stop StopStart { get; set; }
		public Stop StopEnd { get; set; }
		public List<Rout> Routs { get; set; } 
	}
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

		public void Init(Stop newStop)
		{
			this.Stop = newStop;
			Location.Latitude = newStop.Lat;
			Location.Longitude = newStop.Lng;
		}

		public void ResetLocation()
		{
			MapPanel.SetLocation(Pushpin, Location);
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