using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinskTrans.AutoRouting.AutoRouting;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.Comparer;
using MyLibrary;


namespace MinskTrans.Context
{
    public abstract class GenericBussnessLogic: IBussnessLogics
    {
        private Dictionary<CancellationToken, bool> tokens = new Dictionary<CancellationToken, bool>(); 
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

        Queue<ErrorMessage> MessageToShow { get; set; }

        public IGeolocation Geolocation { get; set; }
        public IEnumerable<Rout> Routs { get { return Context.Routs; } }

        private int countUpdateFail = 0;
        private int maxCountUpdateFail = 10;

        public async Task LoadDataBase(LoadType loadType = LoadType.LoadAll)
        {
            OnLoadStarted();
            try
            {
                await Context.Load(loadType);
            }
            finally
            {
                OnLoadEnded();
            }
        }

        public async Task Save(bool saveAllDB = true)
        {
            await Context.Save(saveAllDB);
        }


        public IEnumerable<Stop> FilteredStops(string StopNameFilter, TransportType selectedTransport = TransportType.All, Location location = null, bool FuzzySearch = false)
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
                        x => x.SearchName.Contains(tempSt) || x.Routs.Any(r=> r.RouteNum.Contains(tempSt))).OrderBy(x => x.SearchName.StartsWith(tempSt));
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

        public async Task<IEnumerable<Stop>> FilteredStopsAsync(string StopNameFilter, CancellationToken token,
            TransportType selectedTransport = TransportType.All, Location location = null, bool FuzzySearch = false)
        {
            if (token.IsCancellationRequested)
                return null;
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
                        if (FuzzySearch)
                            //TODO
                            //temp = Levenshtein.Search(tempSt, (List<Stop>) returnList.ToList(), 0.4);
                            ;
                        else
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
                    return null;
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
            var byDeistance = enumerable.OrderBy(x => Distance(x, location)).ToList();
            var result = enumerable.OrderByDescending(x => Context.GetCounter(x))
                .Select((x, i) => new { x, byCounter = i, byDistance = byDeistance.IndexOf(x) })
                .OrderBy(x => x.byCounter + x.byDistance)
                .Select(x => x.x)
                .ToList();
            return result;
        }

        public void SetGPS(bool v, object useGPS)
        {
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
            if (stp == null)
                return null;
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

        Dictionary<int, IEnumerable<Stop>> GetDirectionCache = new Dictionary<int, IEnumerable<Stop>>();
        public IEnumerable<Stop> GetDirection(int stopID, int count)
        {
            if (GetDirectionCache.Keys.Contains(stopID))
                return GetDirectionCache[stopID];
            IList<Stop> tempList;
            if (Context.GetStopDirectionDelegate != null)
                tempList =  Context.GetStopDirectionDelegate.Invoke(stopID, count).ToList();
            else
            tempList =  Context.Routs.AsParallel().Where(x => x.Stops.AsParallel().Contains(Context.Stops.First(s => s.ID == stopID))).Select(
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

        public abstract Task<bool> UpdateTimeTableAsync(CancellationToken token, bool withLightCheck = false, bool tryOnlyOriginalLink = false);

        public IEnumerable<Rout> GetDirectionsStop(Stop FilteredSelectedStop)
        {
            if (FilteredSelectedStop == null)
                return null;
            return Context.Routs.Where(x => x.Stops.Contains(FilteredSelectedStop)).Distinct();

        }

        public void SetGPS(bool useGPS)
        {
            if (useGPS)
            { StartGPS(); }
            else { StopGPS(); }
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
                if (!string.IsNullOrWhiteSpace(RoutNum) )
                    temp = temp.Where(x => x.RouteNum.Contains(RoutNum));
                //if (temp.Any())
                //    RouteNumSelectedValue = temp.First();
                return temp;
            }
            return new List<Rout>();
            //return routeNums;
        }

        

        public List<StopTimePair> GetStopsTimesParis(Rout rout, int mins, int day)
        {
            int index = 0;
            for (int i = 0; i < rout.Stops.Count; i++)
            {
               index = rout.Time.TimesDictionary[i].Where(x=> x.Days.Contains(day.ToString())).First().Times.IndexOf(mins);
                if (index >= 0)
                {
                    break;
                }
            }
            if (index >= 0)
            {
                List<StopTimePair> pair = new List<StopTimePair>();
                for (int i = 0; i < rout.Stops.Count; i++)
                {
                    StopTimePair p = new StopTimePair();
                    p.Time = new TimeSpan(0,0,rout.Time.TimesDictionary[i][day-1].Times[index] - mins, 0,0);
                    p.Stop = rout.Stops[i];
                    pair.Add(p);
                }
                return pair;
            }
            return null;
        } 

        public event EventHandler<EventArgs> NeedUpdadteDB;

        public DateTime LastUpdateDbDateTimeUtc
        {
            get { return Settings.LastUpdateDbDateTimeUtc; }
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
