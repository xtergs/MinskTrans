using Windows.Devices.Geolocation;
using Windows.Foundation;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using PositionChangedEventArgs = MinskTrans.Context.Base.PositionChangedEventArgs;
using PositionStatus = MinskTrans.Context.Base.PositionStatus;
using StatusChangedEventArgs = MinskTrans.Context.Base.StatusChangedEventArgs;

namespace CommonLibrary
{
    static class PositionHeler
    {
        public static PositionStatus ToPositionStatus(this Windows.Devices.Geolocation.PositionStatus pos)
        {
            switch (pos)
            {
                    case Windows.Devices.Geolocation.PositionStatus.Disabled: return PositionStatus.Disabled;
                    case Windows.Devices.Geolocation.PositionStatus.Ready: return PositionStatus.Ready;
                    default: return PositionStatus.NotAvailable;
            }
        }
    }
    public class UniversalGeolocator:IGeolocation
    {
        readonly Geolocator geolocator = new Geolocator();

        public UniversalGeolocator()
        {
            
            geolocator.PositionChanged += (sender, args) =>
            {
                Location newLoc = new Location( args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);
                OnPositionChanged(new PositionChangedEventArgsArgs()
                {
                    NewLocation = newLoc,
                    PrevLocation = CurLocation
                });
                CurLocation = newLoc;
            };

            geolocator.StatusChanged += (sender, args) =>
            {
                OnStatusChanged(new StatusChangedEventArgsArgs() {Status = args.Status.ToPositionStatus()});
            };
        }

        #region Implementation of IGeolocation

        public int MovementThreshold { get { return (int)geolocator.MovementThreshold; } set
        {
            geolocator.MovementThreshold = value;
        } }
        public uint ReportInterval { get { return geolocator.ReportInterval; } set
        {
            geolocator.ReportInterval = value;
        } }
        public Location CurLocation { get; protected set; }
        public event PositionChangedEventArgs PositionChanged;
        public event StatusChangedEventArgs StatusChanged;

        #endregion

        protected virtual void OnPositionChanged(PositionChangedEventArgsArgs args)
        {
            PositionChanged?.Invoke(this, args);
        }

        protected virtual void OnStatusChanged(StatusChangedEventArgsArgs args)
        {
            StatusChanged?.Invoke(this, args);
        }
    }
}
