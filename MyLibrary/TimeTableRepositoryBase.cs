using MinskTrans.DesctopClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient
{
	public abstract class TimeTableRepositoryBase
	{
		readonly Context context;
		public TimeTableRepositoryBase(Context context)
		{
			this.context = context;
		}

		public Context Context { get { return context; } }

		public abstract string TransportToString(Stop stop, TransportType type);

		public IList<RoutWithDestinations> FavouriteRouts
		{
			get
			{
				return Context.FavouriteRouts;
			}
		}

		public IList<Stop> FavouriteStops
		{
			get
			{
				return Context.FavouriteStops;
			}
		}
		public IList<GroupStop> Groups
		{
			get
			{
				return Context.Groups;
			}
		}

		public IList<Schedule> Times
		{
			get { return Context.Times; }
		}

		public IList<Stop> Stops
		{
			get { return Context.ActualStops; }
		}
		

		public IList<Rout> Routs
		{
			get { return Context.Routs; }
		}

		public abstract bool  RoutsHaveStopId(int stopId);
	}
}
