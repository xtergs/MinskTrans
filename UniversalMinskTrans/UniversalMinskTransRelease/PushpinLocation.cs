using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using MapControl;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.Universal
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
