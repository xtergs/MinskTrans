using System.Windows;
using MapControl;
using MinskTrans.Context;
using Location = MapControl.Location;

#if WINDOWS_PHONE_APP
using Windows.UI.Xaml;
#else

#endif

namespace MinskTrans.DesctopClient.Model
{
	public class PushpinLocation : PushpinLocationBase
	{
		public PushpinLocation():base() { }

		public PushpinLocation(Pushpin pin, Location loc)
			:base(pin,loc)
		{
		
		}

		
	}

	
}
