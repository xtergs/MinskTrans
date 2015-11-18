using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MyLibrary;

namespace MinskTrans.Context
{
    public interface IBussnessLogics : INotifyPropertyChanged
    {
        IContext Context { get; }
        ISettingsModelView Settings { get; }
        IGeolocation Geolocation { get; }
        IEnumerable<Rout> Routs { get; }
        Task LoadDataBase(LoadType loadType = LoadType.LoadAll);
        Task Save(bool saveAllDB = true);
        IEnumerable<Stop> FilteredStops(string StopNameFilter, TransportType selectedTransport = TransportType.All, Location location = null,bool FuzzySearch = false);
        void SetGPS(bool v, object useGPS);

        IEnumerable<TimeLineModel> GetStopTimeLine(Stop stp, int day, int startingTime, TransportType selectedTransportType = TransportType.All,
            int endTime = int.MaxValue);
        Stop GetStop(int stopId);
        IEnumerable<Stop> GetDirection(int stopID);
        void AddRemoveFavouriteStop(Stop stop);
        void AddRemoveFavouriteRoute(Rout route);
        Task<bool> UpdateNewsTableAsync();
        Task<bool> UpdateTimeTableAsync(bool withLightCheck = false);
        IEnumerable<Rout> GetDirectionsStop(Stop FilteredSelectedStop);
        void SetGPS(bool useGPS);
        event EventHandler<EventArgs> LoadEnded;
        event EventHandler<EventArgs> LoadStarted;
        event EventHandler<EventArgs> UpdateDBStarted;
        event EventHandler<EventArgs> UpdateDBEnded;
        IEnumerable<Rout> GetRouteNums(TransportType typeTransport, string routNum);
         event EventHandler<EventArgs> NeedUpdadteDB;

        DateTime LastUpdateDbDateTimeUtc { get; }

    }
}