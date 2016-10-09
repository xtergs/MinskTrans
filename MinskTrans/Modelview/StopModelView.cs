using MyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.Geopositioning;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Utilites.FuzzySearch;
using Location = MinskTrans.Context.Location;
using PositionChangedEventArgs = MinskTrans.Context.Base.PositionChangedEventArgs;
#if !(WINDOWS_PHONE_APP || WINDOWS_AP || WINDOWS_UWP)

using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient.Properties;
#else
using Windows.Foundation.Metadata;
using GalaSoft.MvvmLight.Command;

#endif

namespace MinskTrans.DesctopClient.Modelview
{
    public class ListStopTimePair
    {
        public ListStopTimePair(IList<StopTimePair> stopTimePairs, Rout rout, TimeSpan SelectedIndex)
        {
            if (stopTimePairs == null)
                throw new ArgumentNullException(nameof(stopTimePairs));
            if (rout == null)
                throw new ArgumentNullException(nameof(rout));
            StopTimePairs = stopTimePairs;
            Rout = rout;
	        this.SelectedIndex = StopTimePairs.IndexOf(StopTimePairs.First(x => x.Time == SelectedIndex));
        }

        public Rout Rout { get; }
		public int SelectedIndex { get; set; }
        public IList<StopTimePair> StopTimePairs { get; }
    }

    public class StopModelView : StopModelViewBase
    {
        private ISettingsModelView settingsModelView;
        private IExternalCommands commands;
        protected WebSeacher webSeacher;
        private bool autoDay;
        private bool autoNowTime;
/*
		private bool bus;
*/
        private int curDay;
        private int nowTimeHour;
        private int nowTimeMin;
/*
		private bool tram;
*/
/*
		private bool trol;
*/

        private string destinationStop;
        //private LocationXX lastLocation;

/*
		private IGeolocation geolocator;
*/


        //public StopModelView()
        //	: this(null)
        //{
        //}

        protected TransportType selectedTransport = TransportType.Bus | TransportType.Metro | TransportType.Tram |
                                                    TransportType.Trol;

        public StopModelView(IBussnessLogics newContext, ISettingsModelView settings, IExternalCommands commands,
            WebSeacher webSeacher,
            bool UseGPS = false)
            : base(newContext, settings)
        {
            Bus = Trol = Tram = Metro = AutoDay = AutoNowTime = true;
            if (webSeacher == null)
                throw new ArgumentNullException(nameof(webSeacher));
            if (commands == null)
                throw new ArgumentNullException("commands");
            settingsModelView = settings;
            this.webSeacher = webSeacher;
            this.commands = commands;
            IsShowFavouriteStops = settings.IsShowFavouriteStops;
            settingsModelView.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(ISettingsModelView.UseGPS):
                        SetGPS();
                        //OnStatusGPSChanged();
                        break;
                    case nameof(ISettingsModelView.TimeInPast):
                    case nameof(ISettingsModelView.ConsiderFrequencySortStops):
					case nameof(ISettingsModelView.ConsiderDistanceSortStops):
						this.Refresh();
		                break;
                    default:
                        break;
                }
            };
            if (UseGPS)
                SetGPS();

            //IsShowFavouriteStops = false;
        }

        public void SetGPS()
        {
			//return null;

			//	var geolocationStatus = await Geolocator.RequestAccessAsync();
			//	if (geolocationStatus == GeolocationAccessStatus.Denied)
			//		return;
			try
			{
				if (Context.Geolocation == null)
				{
					//geolocator = new Geolocator();
					//geolocator.StatusChanged += GeolocatorOnStatusChanged;
					return;
				}
			}
			catch (Exception ex)
			{
				if (unchecked((uint)ex.HResult == 0x80004004))
				{
					// the application does not have the right capability or the location master switch is off
					//MessageDialog box = new MessageDialog("location  is disabled in phone settings");
					//box.ShowAsync();
				}
				//else
				{
					// something else happened acquring the location
				}
			}
			if (Settings.UseGPS)
			{
				
				Context.Geolocation.MovementThreshold = Settings.GPSThreshholdMeters;

				Context.Geolocation.ReportInterval = Settings.GPSInterval;
				Context.Geolocation.PositionChanged += OnGeolocatorOnPositionChanged;

			}
			else
			{
				if (Context.Geolocation == null)
					return;
				Context.Geolocation.PositionChanged -= OnGeolocatorOnPositionChanged;
				//geolocator.StatusChanged -= GeolocatorOnStatusChanged;

				//geolocator = null;

				//Context.Geolocation.Get().Latitude = defaultLocation.Latitude;
				//LocationXX.Get().Longitude = defaultLocation.Longitude;
			}
		}

		private void OnGeolocatorOnPositionChanged(object o, PositionChangedEventArgsArgs positionChangedEventArgsArgs)
		{
			this.Refresh();
			OnStatusGPSChanged();
		}

		//private void GeolocatorOnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
		//{
		//	if (args.Status == PositionStatus.Ready)
		//	{
		//		OnStatusGPSChanged();
		//	}
		//	else if (args.Status == PositionStatus.Disabled ||
		//			 args.Status == PositionStatus.NotAvailable)
		//	{
		//		LocationXX.Get().Latitude = defaultLocation.Latitude;
		//		LocationXX.Get().Longitude = defaultLocation.Longitude;

		//		OnStatusGPSChanged();
		//	}
		//}

		public event EventHandler<EventArgs> StatusGPSChanged;

		public void OnStatusGPSChanged()
		{
			var handler = StatusGPSChanged;
			if (handler != null)
				handler(this, new EventArgs());
		}


		public bool fuzzySearch;
        private bool isShowFavouriteStops;
        private TimeLineModel _selectedTimeLineModel;
        private bool _isWorking = false;


        //[Deprecated("FilteredStops is depricated, use FilteredStopsStore", DeprecationType.Deprecate, 0)]
        //Deprecated
        public override IEnumerable<Stop> FilteredStops => FilterStops();


        public override IEnumerable<Stop> FilterStops()
        {
            if (IsShowFavouriteStops)
                return Context.Context.FavouriteStops;
            var stops = Context.FilteredStops(StopNameFilter, selectedTransport, Context.Geolocation.CurLocation,
                FuzzySearch);
            return stops;
        }


        public bool IsWorking
        {
            get { return _isWorking; }
            set
            {
                _isWorking = value;
                OnPropertyChanged();
            }
        }

        public override async Task<IEnumerable<Stop>> FilterStopsAsync()
        {
            if (IsShowFavouriteStops)
            {
                FilteredStopsStore = Context.Context.FavouriteStops;
                return FilteredStopsStore;
            }
            if (sourceToken != null)
                sourceToken.Cancel();
            IsWorking = true;
            using (sourceToken = new CancellationTokenSource())
            {
                var token = sourceToken.Token;

                var res = await Context.FilteredStopsAsync(StopNameFilter, token, selectedTransport,
                            Context.Geolocation.CurLocation, FuzzySearch);
                if (token.IsCancellationRequested)
                    return new Stop[0];
                FilteredStopsStore = res;
                sourceToken = null;
                IsWorking = false;
                return res;
            }
        }

        public ISettingsModelView SettingsModelView => settingsModelView;

        public bool IsShowFavouriteStops
        {
            get { return isShowFavouriteStops; }
            set
            {
                if (isShowFavouriteStops == value)
                    return;
                isShowFavouriteStops = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredStops));
                FilterStopsAsync()?.ConfigureAwait(false);
                FavouriteStopsCount = 1;
                Settings.IsShowFavouriteStops = value;
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            OnPropertyChanged(nameof(FavouriteStopsCount));
        }

        public virtual void NotifyTimeScheduleChanged()
        {
            OnPropertyChanged(nameof(TimeSchedule));
        }

        public string DestinationStop
        {
            get { return destinationStop ?? (destinationStop = ""); }
            set
            {
                destinationStop = value;
                OnPropertyChanged();
                NotifyTimeScheduleChanged();
            }
        }


        public bool AutoDay
        {
            get { return autoDay; }
            set
            {
                //if (value.Equals(autoDay)) return;
                autoDay = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurDay));
                NotifyTimeScheduleChanged();
            }
        }

        public int CurDay
        {
            get
            {
                if (AutoDay)
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                        return 7;
                    else
                        return (int) DateTime.Now.DayOfWeek;
                if (curDay <= 0)
                    CurDay = 1;
                return curDay;
            }
            set
            {
                if (value == curDay) return;
                if (value <= 0 || value > 7)
                    return;
                curDay = value;
                AutoDay = false;
                OnPropertyChanged();
                NotifyTimeScheduleChanged();
            }
        }

        public int NowTimeHour
        {
            get { return nowTimeHour; }
            set
            {
                if (value == nowTimeHour) return;
                if (value >= 24 || value < 0)
                    value = 0;
                nowTimeHour = value;
                OnPropertyChanged();
                NotifyTimeScheduleChanged();
            }
        }

        public int NowTimeMin
        {
            get { return nowTimeMin; }
            set
            {
                if (value == nowTimeMin) return;

                while (value >= 60)
                {
                    NowTimeHour++;
                    value -= 60;
                }

                nowTimeMin = value;
                OnPropertyChanged();
                NotifyTimeScheduleChanged();
            }
        }

        public bool AutoNowTime
        {
            get { return autoNowTime; }
            set
            {
                if (value.Equals(autoNowTime)) return;
                autoNowTime = value;
                OnPropertyChanged();
                NotifyTimeScheduleChanged();
            }
        }

        public int CurTime
        {
            get
            {
                if (AutoNowTime)
                    return DateTime.Now.Hour*60 + DateTime.Now.Minute;
                return NowTimeHour*60 + NowTimeMin;
            }
        }

        #region Transport flags

        public bool Trol
        {
            get { return selectedTransport.HasFlag(TransportType.Trol); }
            set
            {
                SetTransportFlag(value, TransportType.Trol);
                OnPropertyChanged();
            }
        }

        public bool Bus
        {
            get { return selectedTransport.HasFlag(TransportType.Bus); }
            set
            {
                SetTransportFlag(value, TransportType.Bus);
                OnPropertyChanged();
            }
        }

        //public RelayCommand<Stop> ShowStopMap
        //{
        //	get { return new RelayCommand<Stop>((x) => OnShowStop(new ShowArgs() { SelectedStop = x }), (x) => x != null); }
        //}

        public bool Tram
        {
            get { return selectedTransport.HasFlag(TransportType.Tram); }
            set
            {
                SetTransportFlag(value, TransportType.Tram);
                OnPropertyChanged();
            }
        }


/*
		private bool metro;
*/
/*
		private Visibility showDetailViewStop;
*/

        public bool Metro
        {
            get { return selectedTransport.HasFlag(TransportType.Metro); }
            set
            {
                SetTransportFlag(value, TransportType.Metro);
                OnPropertyChanged();
            }
        }

        private void SetTransportFlag(bool value, TransportType flag)
        {
            var oldVal = selectedTransport.HasFlag(flag);
            if (value == oldVal)
                return;
            if (value)
                selectedTransport |= flag;
            else
                selectedTransport ^= flag;
            OnPropertyChanged(nameof(FilteredStops));
            OnPropertyChanged(nameof(TimeSchedule));
            FilterStopsAsync();
        }

        #endregion

        public int FavouriteStopsCount
        {
            get { return IsShowFavouriteStops ? 1 : Context.Context.FavouriteStops.Count; }
            set { OnPropertyChanged(); }
        }

        public TimeLineModel SelectedTimeLineModel
        {
            get { return _selectedTimeLineModel; }
            set
            {
                var oldVal = _selectedTimeLineModel;
                _selectedTimeLineModel = value;
                if (oldVal == value || value == null)
                    return;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StopsTimesForRout));
                //var xxx = StopsTimesForRout;
            }
        }

	    public int ShortTimeInPast = 10;

        public virtual IEnumerable<TimeLineModel> TimeSchedule
        {
            get
            {
                if (FilteredSelectedStop == null)
                    return null;
                return Context.GetStopTimeLine(FilteredSelectedStop, CurDay, CurTime - SettingsModelView.TimeInPast,
                    selectedTransport);
            }
        }

        public ListStopTimePair StopsTimesForRout
        {
            get
            {
                if (SelectedTimeLineModel == null)
                    return null;
                return new ListStopTimePair(
                    Context.GetStopsTimesParis(SelectedTimeLineModel.Rout, FilteredSelectedStop,
                        (int) SelectedTimeLineModel.Time.TotalMinutes,
                        CurDay), SelectedTimeLineModel.Rout, SelectedTimeLineModel.Time);
            }
        }

        public RelayCommand<Stop> AddRemoveFavouriteStop
        {
            get
            {
                return new RelayCommand<Stop>((x) =>
                {
                    Context.AddRemoveFavouriteStop(x);
                    FavouriteStopsCount = 1;
                });
            }
        }

        public virtual RelayCommand RefreshTimeSchedule => new RelayCommand(NotifyTimeScheduleChanged);

        public RelayCommand<int> SetTimeInPast
        {
            get { return new RelayCommand<int>(x => settingsModelView.TimeInPast = x); }
        }

        public RelayCommand<Stop> ViewStop
        {
            get
            {
                return new RelayCommand<Stop>(stop =>
                {
                    OnViewStopOn();
                    FilteredSelectedStop = stop;
                    RefreshTimeSchedule.Execute(null);
                });
            }
        }

        public RelayCommand<Stop> ShowStopMap
        {
            get { return commands.ShowStopMap; }
        }

        public RelayCommand BackCommand
        {
            get { return commands.BackPressedCommand; }
        }

        public bool FuzzySearch
        {
            get { return fuzzySearch; }
            set
            {
                fuzzySearch = value;
                OnPropertyChanged(nameof(FilteredStops));
            }
        }

        public event EventHandler ViewStopOn;


        //public ActionCommand ShowRouteMap
        //{
        //	get { return new ActionCommand(x => OnShowRoute(new ShowArgs()), p => RouteSelectedValue != null); }
        //}


        protected virtual void OnViewStopOn()
        {
            var handler = ViewStopOn;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}