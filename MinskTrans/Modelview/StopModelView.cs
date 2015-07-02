﻿using MinskTrans.DesctopClient.Model;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

using MapControl;
#if !WINDOWS_PHONE_APP && !WINDOWS_AP && !WINDOWS_UAP

using GalaSoft.MvvmLight.CommandWpf;
using MinskTrans.DesctopClient.Properties;

#else
using GalaSoft.MvvmLight.Command;
using Windows.Devices.Geolocation;
using Windows.System.Threading;
using Windows.Storage;
using Windows.UI.Xaml;
using MinskTrans.Universal;
using GalaSoft.MvvmLight.Command;
using MinskTrans.DesctopClient.Model;
#endif

namespace MinskTrans.DesctopClient.Modelview
{
	public class StopModelView : StopModelViewBase
	{
		struct LocationXX
		{
			private static Location loc;
			static readonly object o = new object();
			public static Location Get()
			{
				lock (o)
				{
					if (loc == null)
						loc = new Location(0, 0);			
				}
				return loc;
			}
		}
		private ISettingsModelView settingsModelView;
		private bool autoDay;
		private bool autoNowTime;
		private bool bus;
		private int curDay;
		private int nowTimeHour;
		private int nowTimeMin;
		private bool tram;
		private bool trol;
		private string destinationStop;
		//private LocationXX lastLocation;
#if WINDOWS_PHONE_APP || WINDOWS_UAP
		private Geolocator geolocator;
#endif

		//public StopModelView()
		//	: this(null)
		//{
		//}

		public StopModelView(Context newContext, SettingsModelView settings, bool UseGPS = false)
			: base(newContext, settings)
		{
			Bus = Trol = Tram = AutoDay = AutoNowTime = true;
			settingsModelView = settings;
			
			settingsModelView.PropertyChanged += (sender, args) =>
			{
				switch (args.PropertyName)
				{
					case "TimeInPast":Refresh();break;
					case "UseGPS":SetGPS(); OnStatusGPSChanged(); break;
					default: break;
				}
			};
			if (UseGPS)
				SetGPS();
		}

		async void SetGPS()
		{
#if WINDOWS_PHONE_APP && WINDOWS_AP || WINDOWS_UAP
				var geolocationStatus = await Geolocator.RequestAccessAsync();
				if (geolocationStatus == GeolocationAccessStatus.Denied)
					return;
			try
			{
				if (geolocator == null)
				{
					geolocator = new Geolocator();
					geolocator.StatusChanged += GeolocatorOnStatusChanged;
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
					geolocator.MovementThreshold = Settings.GPSThreshholdMeters;

					geolocator.ReportInterval = Settings.GPSInterval;
					geolocator.PositionChanged += OnGeolocatorOnPositionChanged;
				
			}
			else
			{
				if (geolocator == null)
					return;
				geolocator.PositionChanged -= OnGeolocatorOnPositionChanged;
				//geolocator.StatusChanged -= GeolocatorOnStatusChanged;

				geolocator = null;
			}
#endif
		}

#if WINDOWS_PHONE_APP || WINDOWS_UAP
		private void OnGeolocatorOnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
		{
			LocationXX.Get().Latitude = args.Position.Coordinate.Latitude;
			LocationXX.Get().Longitude = args.Position.Coordinate.Longitude;

			OnStatusGPSChanged();
		}

		private void GeolocatorOnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
		{
			if (args.Status == PositionStatus.Ready)
			{
				OnStatusGPSChanged();
			}
			else if (args.Status == PositionStatus.Disabled ||
					 args.Status == PositionStatus.NotAvailable)
			{
				LocationXX.Get().Latitude = defaultLocation.Latitude;
				LocationXX.Get().Longitude = defaultLocation.Longitude;

				OnStatusGPSChanged();
			}
		}
#endif

		public event EventHandler<EventArgs> StatusGPSChanged;

		public void OnStatusGPSChanged()
		{
			var handler = StatusGPSChanged;
			if (handler != null)
				handler(this, new EventArgs());
		}

		static readonly Location defaultLocation = new Location(0,0);

		public override IEnumerable<Stop> FilteredStops
		{
			get
			{
				if (StopNameFilter != null && Context.ActualStops != null)
				{
					var tempSt = StopNameFilter.ToLower();
					var temp = Context.ActualStops.AsParallel().Where(
							x => x.SearchName.Contains(tempSt));
					if (!Equals(LocationXX.Get(), defaultLocation))
						return temp.OrderBy(Distance).ThenByDescending(Context.GetCounter);
					else
						return temp.OrderByDescending(Context.GetCounter).ThenBy(x => x.SearchName);
				}
				if (Context.ActualStops != null)
					return Equals(LocationXX.Get(), defaultLocation) ? Context.ActualStops.OrderByDescending(Context.GetCounter).ThenBy(x => x.SearchName) :
												  Context.ActualStops.OrderBy(Distance).ThenByDescending(Context.GetCounter);
				return null;
			}
		}

		private double Distance(Stop x)
		{
			return Math.Abs(Math.Sqrt(Math.Pow(LocationXX.Get().Longitude - x.Lng, 2) + Math.Pow(LocationXX.Get().Latitude - x.Lat, 2)));
		}
	public ISettingsModelView SettingsModelView
	{
		get { return settingsModelView; }
	}

		public string DestinationStop
		{
			get
			{
				if (destinationStop == null)
					destinationStop = "";
				return destinationStop;
			}
			set
			{
				destinationStop = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
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
				OnPropertyChanged("TimeSchedule");
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
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
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
				OnPropertyChanged("TimeSchedule");
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
				OnPropertyChanged("TimeSchedule");
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
				OnPropertyChanged("TimeSchedule");
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

		public bool Trol
		{
			get { return trol; }
			set
			{
				if (value.Equals(trol)) return;
				trol = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public bool Bus
		{
			get { return bus; }
			set
			{
				if (value.Equals(bus)) return;
				bus = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		public bool Tram
		{
			get { return tram; }
			set
			{
				if (value.Equals(tram)) return;
				tram = value;
				OnPropertyChanged();
				OnPropertyChanged("TimeSchedule");
			}
		}

		virtual public IEnumerable<KeyValuePair<Rout, int>> TimeSchedule
		{
			get
			{
				if (Context.Times != null && FilteredSelectedStop != null)
				{
					IEnumerable<Schedule> dd = Context.Times.Where(time => time != null && time.Rout != null && (time.Rout.Stops.Contains(FilteredSelectedStop) &&
						time.Rout.Stops.Any(stop=> stop.SearchName.Contains(DestinationStop.Trim().ToLower()))));
					IEnumerable<KeyValuePair<Rout, int>> ss = new List<KeyValuePair<Rout, int>>();
					IEnumerable<KeyValuePair<Rout, int>> tt = new List<KeyValuePair<Rout, int>>();
					//foreach (var schedule in dd)
					//{
					//	var temp =
					//		schedule.GetListTimes(schedule.Rout.Stops.IndexOf(FilteredSelectedStop), CurDay - 1, 24 * 60 + CurTime - settingsModelView.TimeInPast).Where(x =>
					//		{
					//			if (x.Key.Transport == Rout.TransportType.Bus)
					//				return Bus;
					//			if (x.Key.Transport == Rout.TransportType.Trol)
					//				return Trol;
					//			if (x.Key.Transport == Rout.TransportType.Tram)
					//				return Tram;
					//			return true;
					//		});

					//	tt = tt.Concat(temp);
					//}

					//tt.OrderBy(x => x.Value);

					foreach (Schedule sched in dd)
					{
						IEnumerable<KeyValuePair<Rout, int>> temp =
							sched.GetListTimes(sched.Rout.Stops.IndexOf(FilteredSelectedStop), CurDay, CurTime - settingsModelView.TimeInPast).Where(x =>
							{
								if (x.Key.Transport == TransportType.Bus)
									return Bus;
								if (x.Key.Transport == TransportType.Trol)
									return Trol;
								if (x.Key.Transport == TransportType.Tram)
									return Tram;
								return true;
							});

						ss = ss.Concat(temp);

						
					}
					ss = ss.OrderBy(x => x.Value);
					//ss.ToString();
					return tt.Concat(ss);
				}
				return null;
			}
		}

		

		public RelayCommand RefreshTimeSchedule
		{
			get { return new RelayCommand(() => OnPropertyChanged("TimeSchedule")); }
		}

		public RelayCommand<int> SetTimeInPast
		{
			get { return new RelayCommand<int>(x=>settingsModelView.TimeInPast = x);}
		}

		public RelayCommand<Stop> ViewStop
		{
			get { return new RelayCommand<Stop>(stop =>
			{
				OnViewStopOn();
				FilteredSelectedStop = stop;
				RefreshTimeSchedule.Execute(null);
			});}
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