﻿using MyLibrary;
using System;
using System.Collections.Generic;

using MapControl;
using System.Threading.Tasks;
using System.Windows;
using MinskTrans.AutoRouting.AutoRouting;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Utilites.FuzzySearch;
using Location = MinskTrans.Context.Location;
#if !WINDOWS_PHONE_APP && !WINDOWS_AP && !WINDOWS_UAP

using GalaSoft.MvvmLight.CommandWpf;
using MinskTrans.DesctopClient.Properties;

#else
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Command;
using Windows.Devices.Geolocation;

#endif

namespace MinskTrans.DesctopClient.Modelview
{
	public class StopModelView : StopModelViewBase
	{
		private struct LocationXX
		{
			private static Location loc;
			private static readonly object o = new object();

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

		private TransportType selectedTransport = TransportType.Bus | TransportType.Metro | TransportType.Tram |
		                                          TransportType.Trol;

		public StopModelView(IContext newContext, SettingsModelView settings, bool UseGPS = false)
			: base(newContext, settings)
		{
			Bus = Trol = Tram = Metro = AutoDay = AutoNowTime = true;
			settingsModelView = settings;

			settingsModelView.PropertyChanged += async (sender, args) =>
			{
				switch (args.PropertyName)
				{
					case "TimeInPast":
						Refresh();
						break;
					case "UseGPS":
						await SetGPS();
						OnStatusGPSChanged();
						break;
					default:
						break;
				}
			};
			if (UseGPS)
#pragma warning disable 4014
				SetGPS();
#pragma warning restore 4014
		}

		private async Task SetGPS()
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

				//geolocator = null;

				LocationXX.Get().Latitude = defaultLocation.Latitude;
				LocationXX.Get().Longitude = defaultLocation.Longitude;
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

		private static readonly Location defaultLocation = new Location(0, 0);
		public bool fuzzySearch;

	    public override IEnumerable<Stop> FilteredStops
	    {
	        get { return context.FilteredStops(StopNameFilter, selectedTransport, LocationXX.Get(), FuzzySearch); }

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
            get { return selectedTransport.HasFlag(TransportType.Trol); }
            set
            {
                if (value)
                    selectedTransport |= TransportType.Trol;
                else
                    selectedTransport ^= TransportType.Trol;
                OnPropertyChanged();
                OnPropertyChanged("FilteredStops");
                OnPropertyChanged("TimeSchedule");

            }
        }

		public bool Bus
		{
			get { return selectedTransport.HasFlag(TransportType.Bus); }
			set
			{
				if (value)
					selectedTransport |= TransportType.Bus;
				else
					selectedTransport ^= TransportType.Bus; 
				OnPropertyChanged();
				OnPropertyChanged("FilteredStops");
                OnPropertyChanged("TimeSchedule");

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
                if (value)
                    selectedTransport |= TransportType.Tram;
                else
                    selectedTransport ^= TransportType.Tram;
                OnPropertyChanged();
                OnPropertyChanged("FilteredStops");
                OnPropertyChanged("TimeSchedule");
            }
        }

		private bool metro;
	    private Visibility showDetailViewStop;

	    public bool Metro
		{
            get { return selectedTransport.HasFlag(TransportType.Metro); }
            set
            {
                if (value)
                    selectedTransport |= TransportType.Metro;
                else
                    selectedTransport ^= TransportType.Metro;
                OnPropertyChanged();
                OnPropertyChanged("FilteredStops");
                OnPropertyChanged("TimeSchedule");

            }
        }

	    public virtual IEnumerable<TimeLineModel> TimeSchedule
	    {
	        get
	        {
	            return Context.GetStopTimeLine(FilteredSelectedStop, CurDay, CurTime - SettingsModelView.TimeInPast,
	                selectedTransport);
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

		public bool FuzzySearch
		{
			get { return fuzzySearch; }
			set
			{
				fuzzySearch = value; 
				OnPropertyChanged("FilteredStops");
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