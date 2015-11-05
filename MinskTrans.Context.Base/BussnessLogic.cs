using System.Collections.Generic;
using System.Linq;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Utilites.FuzzySearch;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.AutoRouting.AutoRouting;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;

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
        private string urlUpdateDates = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111529&authkey=%21ADs9KNHO9TDPE3Q&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D4";
        private string urlUpdateNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111532&authkey=%21AAQED1sY1RWFib8&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D8";
        private string urlUpdateHotNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111531&authkey=%21AIJo-8Q4661GpiI&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D2";
      

        private IContext context;
	    private UpdateManagerBase updateManager;
	    private InternetHelperBase internetHelper;
	    private FileHelperBase fileHelper;
	    private NewsManagerBase newManager;
	    private ISettingsModelView settings;

        public IContext Context { get { return context; } }
		
		Queue<ErrorMessage> MessageToShow { get; set; }

        public DateTime LastUpdateDbDateTimeUtc { get; set; }

	    public async Task LoadDataBase(LoadType loadType = LoadType.LoadAll)
	    {
	        await Context.Load(loadType);
	    }


		public IEnumerable<Stop> FilteredStops(string StopNameFilter, TransportType selectedTransport = TransportType.All, Location location = null,bool FuzzySearch = false)
		{
			
				IEnumerable<Stop> returnList = Context.ActualStops;
				if (returnList != null)
					returnList = returnList.Where(x => x.Routs.Any(d => selectedTransport.HasFlag(d.Transport))).ToList();
				if (!string.IsNullOrWhiteSpace(StopNameFilter) && returnList != null)
				{
					var tempSt = StopNameFilter.ToLower();
				    IEnumerable<Stop> temp = new List<Stop>();
				    if (FuzzySearch)
				        //TODO
				        //temp = Levenshtein.Search(tempSt, (List<Stop>) returnList.ToList(), 0.4);
				        ;
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
		    IEnumerable<Stop> enumerable = stops as IList<Stop> ?? stops.ToList();
		    var byDeistance = enumerable.OrderBy(x=> Distance(x, location)).ToList();
			var result = enumerable.OrderByDescending(x => Context.GetCounter(x))
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



	    private bool updatingNewsTable = false;

	    public async Task<bool> UpdateNewsTableAsync()
	    {
	        if (updatingNewsTable)
	            return false;
	        DateTime utcNow = DateTime.UtcNow;
	        string fileNews = "datesNews_002.dat";
	        try
	        {
	            updatingNewsTable = true;
	            try
	            {
	                await internetHelper.Download(urlUpdateDates, fileNews, TypeFolder.Temp);
	            }
	            catch (Exception e)
	            {

	                return false;
	            }
	            string resultStr = await fileHelper.ReadAllTextAsync(TypeFolder.Temp, fileNews);

	            var timeShtaps = resultStr.Split('\n');
                utcNow = DateTime.Parse(timeShtaps[0]);
	            //NewsManager manager = new NewsManager();
	            await newManager.Load();
	            DateTime oldMonthTime = newManager.LastUpdateMainNewsDateTimeUtc;
	            DateTime oldDaylyTime = newManager.LastUpdateHotNewsDateTimeUtc;

	            if (utcNow > newManager.LastUpdateMainNewsDateTimeUtc)
	            {
	                try
	                {
	                    await internetHelper.Download(urlUpdateNews, newManager.FileNameMonths, TypeFolder.Local);
	                    newManager.LastUpdateMainNewsDateTimeUtc = utcNow;
	                    //TODO settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
	                }
	                catch (Exception e)
	                {
	                    string message =
	                        new StringBuilder("News manager download months exception").AppendLine(e.ToString())
	                            .AppendLine(e.Message)
	                            .AppendLine(e.StackTrace)
	                            .ToString();
	                    Debug.WriteLine(message);
	                }
	            }


                utcNow = DateTime.Parse(timeShtaps[1]);
	            if (utcNow > newManager.LastUpdateHotNewsDateTimeUtc)
	            {
	                try
	                {
	                    await internetHelper.Download(urlUpdateHotNews, newManager.FileNameDays, TypeFolder.Local);
                        newManager.LastUpdateHotNewsDateTimeUtc = utcNow;
	                   //TODO settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
	                }
	                catch (Exception e)
	                {
	                    string message =
	                        new StringBuilder("News manager download days exception").AppendLine(e.ToString())
	                            .AppendLine(e.Message)
	                            .AppendLine(e.StackTrace)
	                            .ToString();
	                    Debug.WriteLine(message);
	                }
	            }

	            DateTime nowTimeUtc = DateTime.UtcNow;
                //TODO
	            //var listOfDaylyNews = newManager.NewNews.Where(
	            //    key => key.PostedUtc > oldMonthTime && ((nowTimeUtc - key.PostedUtc).TotalDays < MaxDaysAgo));

                //TODO
	            //var listOfMonthNews = newManager.AllHotNews.Where(key =>
	            //{
	            //    if (key.RepairedLineUtc != default(DateTime))
	            //    {
	            //        double totalminutes = (nowTimeUtc.ToLocalTime() - key.RepairedLineLocal).TotalMinutes;
	            //        if (totalminutes <= MaxMinsAgo)
	            //            return true;
	            //        return false;
	            //    }
	            //    return (key.CollectedUtc > oldDaylyTime) &&
	            //           ((nowTimeUtc - key.CollectedUtc).TotalDays < 1);
	            //});
	            return true;
	        }
	        finally
	        {
	            updatingNewsTable = false;
	        }
	    }


	    private bool updatingTimeTable = false;

	    public async Task<bool> UpdateTimeTableAsync(bool withLightCheck = false)
	    {
	        if (updatingTimeTable)
	            return false;
	        DateTime utcNow = DateTime.UtcNow;
            string fileNews = "datesNews_001.dat";
            try
	        {
	            updatingTimeTable = true;
	            if (withLightCheck)
	            {
	                try
	                {
	                    await internetHelper.Download(urlUpdateDates, fileNews, TypeFolder.Temp);
	                }
	                catch (Exception e)
	                {

	                    return false;
	                }
	                string resultStr = await fileHelper.ReadAllTextAsync(TypeFolder.Temp, fileNews);

	                var timeShtaps = resultStr.Split('\n');
	                
	                if (timeShtaps.Length > 2)
	                    utcNow = DateTime.Parse(timeShtaps[2]);
	                if (utcNow <= LastUpdateDbDateTimeUtc)
	                {
	                    return false;
	                }
	            }
	            if (!await updateManager.DownloadUpdate())
	                return false;
	            var timeTable = await updateManager.GetTimeTable();
	            if (await Context.HaveUpdate(timeTable.Routs as IList<Rout>, timeTable.Stops as IList<Stop>, timeTable.Time as IList<Schedule>))
	            {
	                await Context.ApplyUpdate(timeTable.Routs as IEnumerable<Rout>, timeTable.Stops as IList<Stop>, timeTable.Time as IList<Schedule>);
	                Context.AllPropertiesChanged();
	                await Context.Save(true);
	            }
	            LastUpdateDbDateTimeUtc = utcNow;
	            return true;
	        }
	        finally
	        {
	            updatingTimeTable = false;
	        }
	    }
	}
}
