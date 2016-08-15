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
    }
    public interface IBussnessLogics : INotifyPropertyChanged
    {
        IContext Context { get; }
        ISettingsModelView Settings { get; }
        IGeolocation Geolocation { get; }
        IEnumerable<Rout> Routs { get; }
        Task LoadDataBase(LoadType loadType = LoadType.LoadAll);
        Task Save(bool saveAllDB = true);
        IEnumerable<Stop> FilteredStops(string StopNameFilter, TransportType selectedTransport = TransportType.All, Location location = null,bool FuzzySearch = false);
        Task<IEnumerable<Stop>> FilteredStopsAsync(string StopNameFilter, CancellationToken token, TransportType selectedTransport = TransportType.All, Location location = null, bool FuzzySearch = false);
        void SetGPS(bool v, object useGPS);

        TimeLineModel[] GetStopTimeLine(Stop stp, int day, int startingTime, TransportType selectedTransportType = TransportType.All,
            int endTime = int.MaxValue);
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
    }
}