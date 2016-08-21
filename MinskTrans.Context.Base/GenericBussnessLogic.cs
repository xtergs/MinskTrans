﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MinskTrans.AutoRouting.AutoRouting;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.Comparer;
using MyLibrary;


namespace MinskTrans.Context
{
	public abstract class GenericBussnessLogic : IBussnessLogics
	{
		protected Dictionary<CancellationToken, bool> tokens = new Dictionary<CancellationToken, bool>();
		private bool isWorking = false;
		private bool isLoaded = false;
		private bool isLocked = false;

		//private readonly UpdateManagerBase updateManager;
		//private readonly InternetHelperBase internetHelper;
		// private readonly FileHelperBase fileHelper;
		// private readonly NewsManagerBase newManager;
		//private readonly ISettingsModelView settings;

		public GenericBussnessLogic(IContext cont)
		{
			if (cont == null)
				throw new ArgumentNullException(nameof(cont));
			Context = cont;
			Context.ErrorLoading += (sender, args) =>
			{
				if (args.Error == ErrorLoadingDelegateArgs.Errors.NoSourceFiles)
					OnNeedUpdadteDB();
			};
			// this.updateManager = updateManager;
			// this.settings = settings;
		}


		public IContext Context { get; }
		public abstract ISettingsModelView Settings { get; }

		private Queue<ErrorMessage> MessageToShow { get; set; }

		public IGeolocation Geolocation { get; set; }

		public IEnumerable<Rout> Routs
		{
			get { return Context.Routs; }
		}

		private int countUpdateFail = 0;
		private int maxCountUpdateFail = 10;
		private CancellationTokenSource loadDataSource = null;

		public async Task LoadDataBase(LoadType loadType = LoadType.LoadAll)
		{
			if (loadDataSource != null)
				return;
			using (loadDataSource = new CancellationTokenSource())
			{
				var token = loadDataSource.Token;
				await LoadDataBase(token);
			}
			loadDataSource = null;
			OnPropertyChanged(nameof(Context.Stops));
			OnPropertyChanged(nameof(Context.Routs));
		}

		public async Task LoadDataBase(CancellationToken token, LoadType loadType = LoadType.LoadAll)
		{
			OnLoadStarted();
			try
			{
				tokens.Add(token, true);
				await Context.Load(loadType);
				if (Context.UpdateDateTimeUtc > DateTime.MinValue)
					LastUpdateDbDateTimeUtc = Context.UpdateDateTimeUtc;
			}
			finally
			{
				tokens.Remove(token);
				OnLoadEnded();
			}
		}

		public async Task Save(bool saveAllDB = true)
		{
			Context.UpdateDateTimeUtc = LastUpdateDbDateTimeUtc;
			await Context.Save(saveAllDB);
		}


		public IEnumerable<Stop> FilteredStops(string StopNameFilter,
			TransportType selectedTransport = TransportType.All, Location location = null, bool FuzzySearch = false, bool considerFrequency = true)
		{
			IEnumerable<Stop> returnList = Context.ActualStops;
			if (returnList != null)
				returnList = returnList.Where(x => x.Routs.Any(d => selectedTransport.HasFlag(d.Transport))).ToList();
			if (!string.IsNullOrWhiteSpace(StopNameFilter) && returnList != null)
			{
				var tempSt = StopNameFilter.ToLower();
				IEnumerable<Stop> temp = new List<Stop>();
				//if (FuzzySearch)
				//    //TODO
				//    //temp = Levenshtein.Search(tempSt, (List<Stop>) returnList.ToList(), 0.4);
				//    ;
				//else
				temp = returnList.Where(
					x => x.SearchName.Contains(tempSt) || x.Routs.Any(r => r.RouteNum.Contains(tempSt)))
					.OrderBy(x => x.SearchName.StartsWith(tempSt));
				if (location != null)
					return
						SmartSort(temp, location);
				//Enumerable.OrderBy<Stop, double>(temp, (Func<Stop, double>) this.Distance)
				//	.ThenByDescending((Func<Stop, uint>) Context.GetCounter);
				else if (considerFrequency)
					return
						temp.OrderByDescending(Context.GetCounter)
							.ThenByDescending(x => x.SearchName.StartsWith(tempSt));
				return temp.OrderByDescending(x => x.SearchName.StartsWith(tempSt));
			}
			if (returnList != null)
				if (location == null)
					if (considerFrequency)
						return returnList.OrderByDescending(Context.GetCounter).ThenBy(x => x.SearchName);
					else
						return returnList.OrderBy(x => x.SearchName);
				else
					return SmartSort(returnList, location);
			return null;
		}

		public async Task<IEnumerable<Stop>> FilteredStopsAsync(string StopNameFilter, CancellationToken token,
			TransportType selectedTransport = TransportType.All, Location location = null, bool FuzzySearch = false)
		{
			if (token.IsCancellationRequested)
				return new Stop[0];
			tokens.Add(token, true);
			try
			{
				return await Task.Run(() =>
				{
					IEnumerable<Stop> returnList = Context.ActualStops;
					returnList =
						returnList?.Where(x => x.Routs.Any(d => selectedTransport.HasFlag(d.Transport))).ToList();
					if (!string.IsNullOrWhiteSpace(StopNameFilter) && returnList != null)
					{
						var tempSt = StopNameFilter.ToLower();
						IEnumerable<Stop> temp = new List<Stop>();
						//if (FuzzySearch)
						//    //TODO
						//    //temp = Levenshtein.Search(tempSt, (List<Stop>) returnList.ToList(), 0.4);
						//    ;
						//else
						temp = returnList.Where(
							x => x.SearchName.Contains(tempSt) || x.Routs.Any(r => r.RouteNum.Contains(tempSt)))
							.OrderBy(x => x.SearchName.StartsWith(tempSt));
						if (location != null)
							return
								SmartSort(temp, location);
						//Enumerable.OrderBy<Stop, double>(temp, (Func<Stop, double>) this.Distance)
						//	.ThenByDescending((Func<Stop, uint>) Context.GetCounter);
						else
							return
								temp.OrderByDescending(Context.GetCounter)
									.ThenByDescending(x => x.SearchName.StartsWith(tempSt));
					}
					if (returnList != null)
						return location == null
							? returnList.OrderByDescending(Context.GetCounter).ThenBy(x => x.SearchName)
							//: Context.ActualStops.OrderBy(Distance).ThenByDescending(Context.GetCounter);
							: SmartSort(returnList, location);
					return new Stop[0];
				}, token);
			}
			finally
			{
				tokens.Remove(token);
			}
		}


		private IEnumerable<Stop> SmartSort(IEnumerable<Stop> stops, Location location)
		{
			IEnumerable<Stop> enumerable = stops as IList<Stop> ?? stops.ToList();
			var byDeistance = enumerable.Select(x => new {Stop = x, Distance = Distance(x, location)})
				.OrderBy(x => x.Distance)
				.ToDictionary(x => x.Stop, x => x.Distance);
			var result = enumerable.OrderByDescending(x => Context.GetCounter(x))
				.Select((x, i) => new {x, byCounter = i, byDistance = byDeistance[x]})
				.OrderBy(x => x.byCounter + x.byDistance)
				.Select(x => x.x)
				.ToList();
			return result;
		}

		public void SetGPS(bool v, object useGPS)
		{
		}

		private EquirectangularDistance distance = new EquirectangularDistance();

		private double Distance(Stop x, Location location)
		{
			return distance.CalculateDistance(location.Latitude, location.Longitude, x.Lat, x.Lng);
			//return Math.Abs(Math.Sqrt(Math.Pow( - x.Lng, 2) + Math.Pow(LocationXX.Get().Latitude - x.Lat, 2)));
		}

		public TimeLineModel[] GetStopTimeLine(int StopId, int day, int currentTimeMin, List<Rout> routs = null,
			int prevCount = 1, int nexCount = 3)
		{
			var stp = GetStop(StopId);
			if (stp == null)
				return null;
			var stopTimeLine = new List2<Rout, int>();


			foreach (Rout rout in stp.Routs.Where(x => routs?.Any(r => r.RoutId == x.RoutId) ?? true).ToArray())
			{
				Schedule sched = GetRouteSchedule(rout.RoutId);
				if (sched == null)
					return null;
				sched.Rout = rout;
				//    Schedule sched = rout.Time;
				var temp =
					sched.GetListTimes(rout.Stops.IndexOf(stp), day, currentTimeMin - 60, currentTimeMin + 60);
				stopTimeLine.AddRange(temp);
			}
			var orderedCollection = stopTimeLine.OrderBy(s => s.Value).ToList();
			stopTimeLine = null;
			if (orderedCollection == null)
				return null;
			var dd =
				orderedCollection.Select(p => new {P = p, Diff = p.Value - currentTimeMin})
					.Where(p => p.Diff >= 0)
					.OrderBy(d => d.Diff)
					.FirstOrDefault()?
					.P;
			if (!dd.HasValue)
				return null;
			var index = orderedCollection.IndexOf(dd.Value);
			index = Math.Max(0, index - prevCount);
			var subset =
				orderedCollection.Skip(index)
					.Take(prevCount + nexCount)
					.Select(x => new TimeLineModel(x.Key, new TimeSpan(0, 0, x.Value, 0, 0)));

			subset = subset.OrderBy(x => x.Time);
			return subset.ToArray();
		}

		public TimeLineModel[] GetStopTimeLine(Stop s, int day, int startingTime,
			TransportType selectedTransportType = TransportType.All,
			int endTime = int.MaxValue)
		{
			if (s == null)
				return null;
			var stp = GetStop(s.ID);
			if (stp == null)
				return null;
			IEnumerable<TimeLineModel> stopTimeLine = new List<TimeLineModel>();


			foreach (Rout rout in stp.Routs.Where(x => selectedTransportType.HasFlag(x.Transport)))
			{
				Schedule sched = GetRouteSchedule(rout.RoutId);
				if (sched == null)
					return null;
				sched.Rout = rout;
				//    Schedule sched = rout.Time;
				IEnumerable<TimeLineModel> temp =
					sched.GetListTimes(rout.Stops.IndexOf(stp), day, startingTime, endTime)
						.Select(x => new TimeLineModel(x.Key, new TimeSpan(0, 0, x.Value, 0, 0)));

				stopTimeLine = stopTimeLine.Concat(temp);
			}
			stopTimeLine = stopTimeLine.OrderBy(x => x.Time);
			return stopTimeLine.ToArray();
		}

		private Dictionary<int, IEnumerable<Stop>> GetDirectionCache = new Dictionary<int, IEnumerable<Stop>>();

		public IEnumerable<Stop> GetDirection(int stopID, int count)
		{
			if (GetDirectionCache.Keys.Contains(stopID))
				return GetDirectionCache[stopID];
			IList<Stop> tempList;
			if (Context.GetStopDirectionDelegate != null)
				tempList = Context.GetStopDirectionDelegate.Invoke(stopID, count).ToList();
			else
				tempList =
					Context.Routs.AsParallel()
						.Where(x => x.Stops.AsParallel().Contains(Context.Stops.First(s => s.ID == stopID)))
						.Select(
							r =>
								r.Stops.Last()).Take(count).ToList();
			GetDirectionCache.Add(stopID, tempList);
			return tempList;
		}

		public void AddRemoveFavouriteStop(Stop stop)
		{
			if (Context.IsFavouriteStop(stop))
				Context.RemoveFavouriteStop(stop);
			else
				Context.AddFavouriteStop(stop);
		}

		public void AddRemoveFavouriteRoute(Rout route)
		{
			if (Context.IsFavouriteRout(route))
			{
				Context.RemoveFavouriteRout(route);
			}
			else
				Context.AddFavouriteRout(route);
		}

		public abstract Task<bool> UpdateNewsTableAsync(CancellationToken token);

		public abstract Task<bool> UpdateTimeTableAsync(CancellationToken token, bool withLightCheck = false,
			bool tryOnlyOriginalLink = false);

		public IEnumerable<Rout> GetDirectionsStop(Stop FilteredSelectedStop)
		{
			if (FilteredSelectedStop == null)
				return null;
			return Context.Routs.Where(x => x.Stops.Any(y=> y.ID ==  FilteredSelectedStop.ID)).Distinct();
		}

		public IEnumerable<Rout> GetIntersectionRoutsByStops(Stop stop1, Stop stop2)
		{
			return stop1.Routs.Intersect(stop2.Routs, new RoutComparer()).ToList();
		}

		public void SetGPS(bool useGPS)
		{
			if (useGPS)
			{
				StartGPS();
			}
			else
			{
				StopGPS();
			}
		}

		public event EventHandler<EventArgs> LoadEnded;
		public event EventHandler<EventArgs> LoadStarted;
		public event EventHandler<EventArgs> UpdateDBStarted;
		public event EventHandler<EventArgs> UpdateDBEnded;

		private void StopGPS()
		{
		}

		private void StartGPS()
		{
		}

		public IEnumerable<Rout> GetRouteNums(TransportType TypeTransport, string RoutNum)
		{
			if (Context.Routs != null)
			{
				//if (string.IsNullOrWhiteSpace(RoutNum) || TypeTransport == TransportType.None)
				//    return Context.Routs.Distinct(new RoutComparer());
				IEnumerable<Rout> temp =
					Context.Routs.Where(rout => rout.Transport == TypeTransport).Distinct(new RoutComparer());
				if (!string.IsNullOrWhiteSpace(RoutNum))
					temp = temp.Where(x => x.RouteNum.Contains(RoutNum));
				//if (temp.Any())
				//    RouteNumSelectedValue = temp.First();
				return temp;
			}
			return new List<Rout>();
			//return routeNums;
		}

		protected int GetTimeIndex(Schedule timee, Rout rout, Stop stop, int mins, int day)
		{
			int indexStop = rout.Stops.IndexOf(stop);
			var time = timee.GetScheduleForStop(stopIndex: indexStop, day: day).Times;
			int timeIndex = Array.IndexOf(time, mins);
			return timeIndex;
		}

		public int GetTimeIndex(Rout rout, Stop stop, int mins, int day)
		{
			Schedule timee = GetRouteSchedule(rout.RoutId);
			int timeIndex = GetTimeIndex(timee, rout, stop, mins, day);
			return timeIndex;
		}

		public List<StopTimePair> GetStopsTimesParis(Rout rout, Stop stop, int mins, int day)
		{
			Schedule timee = GetRouteSchedule(rout.RoutId);
			var st = GetStop(stop.ID);
			int timeIndex = GetTimeIndex(timee, rout, st, mins, day);
			//for (int i = 0; i < rout.Stops.Count; i++)
			//{
			//   index = timee.Where(.First(x => x.Days.Contains(day.ToString())).Times.ToList().IndexOf(mins);
			//    if (index >= 0)
			//    {
			//        break;
			//    }
			//}
			if (timeIndex >= 0)
			{
				return rout.Stops.Select((t, i) => new StopTimePair
				{
					Time = new TimeSpan(0, 0, timee.GetScheduleForStop(stopIndex: i, day: day)
						.Times[timeIndex], 0, 0),
					Stop = t
				}).ToList();
			}
			return null;
		}

		public void ForsStopActivity()
		{
			foreach (var token in tokens)
			{
			}
		}

		public Schedule GetRouteSchedule(int routId)
		{
			var time = Context.Times.FirstOrDefault(t => t.RoutId == routId);
			return time;
		}

		public event EventHandler<EventArgs> NeedUpdadteDB;

		public DateTime LastUpdateDbDateTimeUtc
		{
			get { return Settings.LastUpdateDbDateTimeUtc; }
			set { Settings.LastUpdateDbDateTimeUtc = value; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnLoadEnded()
		{
			LoadEnded?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnLoadStarted()
		{
			LoadStarted?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string str = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(str));
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}


		protected virtual void OnNeedUpdadteDB()
		{
			NeedUpdadteDB?.Invoke(this, new EventArgs());
		}

		public Stop GetStop(int stopId)
		{
			if (Context.GetStopDelegate != null)
				return Context.GetStopDelegate.Invoke(stopId);
			return Context.Stops.FirstOrDefault(x => x.ID == stopId);
		}

		protected virtual void OnUpdateDbStarted()
		{
			UpdateDBStarted?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnUpdateDbEnded()
		{
			UpdateDBEnded?.Invoke(this, EventArgs.Empty);
		}
	}
}