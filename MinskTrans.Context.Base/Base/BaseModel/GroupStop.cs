using System.Collections.Generic;
using System.Linq;

namespace MinskTrans.Context.Base.BaseModel
{

	public class GroupStop :BaseModel
	{
		private string name;
		private IList<Stop> stops;

		public GroupStop()
		{
			Stops = new List<Stop>();
		}

		public GroupStop(GroupStop group)
		{
			Name = group.Name;
			Stops = new List<Stop>();
			foreach (var stop in group.Stops)
			{
				Stops.Add(stop);
			}
		}

		public IList<Stop> Stops
		{
			get
			{
				if (stops == null)
					stops = new List<Stop>();
				return stops;
			}
			set
			{
				stops = value;
				//OnPropertyChanged();
			}
		}

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				//OnPropertyChanged();
			}
		}
	}
}