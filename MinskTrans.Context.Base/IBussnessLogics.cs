using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MyLibrary;

namespace MinskTrans.Context
{
    public class StopTimePair
    {
        public Stop Stop { get; set; }

        public TimeSpan Time { get; set; }

	    public bool InPast => Time < DateTime.Now.TimeOfDay;
    }

    public class StopSearchResult : Stop
    {
        private Stop _stop = null;

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

        public double? Distance { get; set; } = null;
        public double? Frequency { get; set; } = null;
        public int StartMatch { get; set; } = -1;
        public int MatchLength { get; set; } = -1;
        public bool FoundInRout { get; set; } = false;


        public string FirstPart
        {
            get
            {
                if (StartMatch > 0)
                    return Stop.Name.Substring(0, StartMatch);
                else if (StartMatch == 0)
                    return "";
                return Stop.Name;
            }
        }

        public string Hightlight
        {
            get
            {
                if (MatchLength > 0)
                    return Stop.Name.Substring(StartMatch, MatchLength);
                return "";
            }
        }

        public string SecondPart
        {
            get
            {
                if (StartMatch >= 0 && StartMatch + MatchLength < Stop.Name.Length)
                    return Stop.Name.Substring(StartMatch + MatchLength);
                return "";
            }
        }
    }
    public interface IBussnessLogics : INotifyPropertyChanged
    {
        IContext Context { get; }
        ISettingsModelView Settings { get; }
        IGeolocation Geolocation { get; }
        IEnumerable<Rout> Routs { get; }
        Task LoadDataBase(LoadType loadType = LoadType.LoadAll);
        Task Save(bool saveAllDB = true);
        IEnumerable<Stop> FilteredStops(string StopNameFilter, TransportType selectedTransport = TransportType.All, Location location = null,bool FuzzySearch = false, bool considerFrequency = true);
        Task<IEnumerable<Stop>> FilteredStopsAsync(string StopNameFilter, CancellationToken token, TransportType selectedTransport = TransportType.All, Location location = null, bool FuzzySearch = false);

        IEnumerable<StopSearchResult> FilteredStopsEx(string StopNameFilter,
            TransportType selectedTransport = TransportType.All, Location location = null, bool FuzzySearch = false,
            bool considerFrequency = true);
        void SetGPS(bool v, object useGPS);

        TimeLineModel[] GetStopTimeLine(Stop stp, int day, int startingTime, TransportType selectedTransportType = TransportType.All,
            int endTime = int.MaxValue);

	    TimeLineModel[] GetStopTimeLine(int StopId, int day, int currentTimeMin, List<Rout> routs = null,
		    int prevCount = 1, int nexCount = 3);

		Stop GetStop(int stopId);
        IEnumerable<Stop> GetDirection(int stopID, int count);
        void AddRemoveFavouriteStop(Stop stop);
        void AddRemoveFavouriteRoute(Rout route);
        Task<bool> UpdateNewsTableAsync(CancellationToken token);
        Task<bool> UpdateTimeTableAsync(CancellationToken token, bool withLightCheck = false, bool tryOnlyOriginalLink = false);
        IEnumerable<Rout> GetDirectionsStop(Stop FilteredSelectedStop);
	    IEnumerable<Rout> GetIntersectionRoutsByStops(Stop stop1, Stop stop2); 
        void SetGPS(bool useGPS);
        event EventHandler<EventArgs> LoadEnded;
        event EventHandler<EventArgs> LoadStarted;
        event EventHandler<EventArgs> UpdateDBStarted;
        event EventHandler<EventArgs> UpdateDBEnded;
        IEnumerable<Rout> GetRouteNums(TransportType typeTransport, string routNum);
         event EventHandler<EventArgs> NeedUpdadteDB;

        DateTime LastUpdateDbDateTimeUtc { get; }
        List<StopTimePair> GetStopsTimesParis(Rout rout, Stop stop, int mins, int day);

        void ForsStopActivity();
        Schedule GetRouteSchedule(int routId);

        int GetTimeIndex(Rout rout, Stop stop, int mins, int day);
        void ResetState();
    }
}