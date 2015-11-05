using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Utilites.FuzzySearch;
using System;
using MinskTrans.AutoRouting.AutoRouting;

namespace MinskTrans.Context
{
	public class Location
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}

	public class ErrorMessage
	{
		public DateTime TimeEvent { get; set; }
		public string Message { get; set; }
	}

	public class BussnessLogic : IBussnessLogic
	{
		private IContext context;

		public IContext Context { get { return context; } }
		
		Queue<ErrorMessage> MessageToShow { get; set; }


		public IEnumerable<Stop> FilteredStops(string StopNameFilter, TransportType selectedTransport = TransportType.All, Location location = null,bool FuzzySearch = false)
		{
			
				IEnumerable<Stop> returnList = Context.ActualStops;
				if (returnList != null)
					returnList = returnList.Where(x => x.Routs.Any(d => selectedTransport.HasFlag(d.Transport))).ToList();
				if (!string.IsNullOrWhiteSpace(StopNameFilter) && returnList != null)
				{
					var tempSt = StopNameFilter.ToLower();
					IEnumerable<Stop> temp;
					if (FuzzySearch)
						temp = Levenshtein.Search(tempSt, returnList.ToList(), 0.4);
					else
						temp = returnList.Where(
							x => x.SearchName.Contains(tempSt)).OrderBy(x => x.SearchName.StartsWith(tempSt));
					if (location != null)
						return
							SmartSort(temp, location);
					//Enumerable.OrderBy<Stop, double>(temp, (Func<Stop, double>) this.Distance)
					//	.ThenByDescending((Func<Stop, uint>) Context.GetCounter);
					else
						return temp.OrderByDescending(Context.GetCounter).ThenByDescending(x => x.SearchName.StartsWith(tempSt));

				}
				if (returnList != null)
					return location == null
						? returnList.OrderByDescending(Context.GetCounter).ThenBy(x => x.SearchName)
						//: Context.ActualStops.OrderBy(Distance).ThenByDescending(Context.GetCounter);
						: SmartSort(returnList, location);
				return null;
			
		}



		private IEnumerable<Stop> SmartSort(IEnumerable<Stop> stops, Location location)
		{

			var byDeistance = stops.OrderBy(x=> Distance(x, location)).ToList();
			var result = stops.OrderByDescending(x => Context.GetCounter(x))
				.Select((x, i) => new { x, byCounter = i, byDistance = byDeistance.IndexOf(x) })
				.OrderBy(x => x.byCounter + x.byDistance)
				.Select(x => x.x)
				.ToList();
			return result;
		}

		EquirectangularDistance distance = new EquirectangularDistance();
		private double Distance(Stop x, Location location)
		{
			return distance.CalculateDistance(location.Latitude, location.Longitude, x.Lat, x.Lng);
			//return Math.Abs(Math.Sqrt(Math.Pow( - x.Lng, 2) + Math.Pow(LocationXX.Get().Latitude - x.Lat, 2)));
		}

		public IEnumerable<TimeLineModel> GetStopTimeLine(Stop stp, int day, int startingTime, TransportType selectedTransportType = TransportType.All,
			int endTime = int.MaxValue)
		{
			IEnumerable<TimeLineModel> stopTimeLine = new List<TimeLineModel>();


			foreach (Rout rout in stp.Routs.Where(x => selectedTransportType.HasFlag(x.Transport)))
			{
				Schedule sched = rout.Time;
				IEnumerable<TimeLineModel> temp =
					sched.GetListTimes(rout.Stops.IndexOf(stp), day, startingTime, endTime)
						.Select(x => new TimeLineModel(x.Key, new TimeSpan(0, 0, x.Value, 0, 0)));

				stopTimeLine = stopTimeLine.Concat(temp);


			}
			stopTimeLine = stopTimeLine.OrderBy(x => x.Time);
			return stopTimeLine;


		}


		public IQueryable<Stop> GetDirection(int stopID)
		{
			return context.Stops.Select(x => new { x.ID, x.Routs }).First(s => s.ID == stopID).Routs.Select(
				r =>
					r.Stops.Last()).AsQueryable();
		}

		public void AddRemoveFavouriteStop(Stop stop)
		{
			if (context.IsFavouriteStop(stop))
				context.RemoveFavouriteStop(stop);
			else
				context.AddFavouriteStop(stop);
		}

		public void AddRemoveFavouriteRoute(Rout route)
		{
			if (context.IsFavouriteRout(route))
			{
				context.RemoveFavouriteRout(route);
			}
			else
				context.AddFavouriteRout(route);
		}
    }
}
