using System.Collections.Generic;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using PropertyChanged;

namespace UniversalMinskTransRelease.ViewModel
{
	[ImplementPropertyChanged]
	public class FavouriteStop : Stop
	{
		private Stop _stop;

		public Stop Stop
		{
			get { return _stop; }
			set
			{
				_stop = value;
				ID = value.ID;
				Lat = value.Lat;
				Lng = value.Lng;
				Name = value.Name;
			}
		}

		public TimeLineModel[] CurrentRouts { get; set; } 
	}
}
