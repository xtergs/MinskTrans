using MyLibrary;
using System.Text;
using System.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using MapControl;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context;
using MinskTrans.DesctopClient.Model;
using CalculateRout = MinskTrans.Context.AutoRouting.CalculateRout;
using Location = MapControl.Location;
#if !(WINDOWS_PHONE_APP || WINDOWS_AP || WINDOWS_UWP)
using PositionStatus = MinskTrans.Context.Base.PositionStatus;
using MinskTrans.DesctopClient.Properties;
using System.Windows.Controls;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
#else
using Windows.UI.Xaml.Input;
using MinskTrans.Universal;
using Windows.UI.Core;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Command;
#endif
//using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

namespace MinskTrans.DesctopClient.Modelview
{
	public class MapModelView: BaseModelView
	{
		private Stop currentStop;
		private Rout currentRout;
		private Location location;
	    protected bool showAllPushpins = true;
	    protected readonly Map map;
	    protected List<PushpinLocation> allPushpins;
		private Pushpin ipushpin;
		private ObservableCollection<Pushpin> pushpins1;
		private Pushpin startStopPushpin;
		private Pushpin endStopPushpin;
		private string resultString;
		private ISettingsModelView settings;

		private IGeolocation geolocator;

		public IGeolocation Geolocator => geolocator;


	    private MapModelView(IBussnessLogics context)
			:base(context)
		{
			
		}

		public static Style StylePushpin { get; set; }

	    protected readonly PushPinBuilder pushBuilder;

		public MapModelView(IBussnessLogics context, Map map, ISettingsModelView newSettigns, IGeolocation geolocation, PushPinBuilder pushPinBuilder = null)
			: base(context)
		{
			this.map = map;
			pushBuilder = pushPinBuilder;
			Settings = newSettigns;
			map.ViewportChanged += (sender, args) => RefreshPushPinsAsync();
		    if (geolocation == null)
		        throw new ArgumentNullException(nameof(geolocation));
		    this.geolocator = geolocation;

		    map.MinZoomLevel = 11;
		    map.MaxZoomLevel = 19; 
			MaxZoomLevel = 14;
			map.ZoomLevel = 19;
			map.Center = new Location(53.898532, 27.562501);
			allPushpins = new List<PushpinLocation>();
			RegistrMap(true);
			
		}

		public void MarkPushPins(IEnumerable<Stop> stops, Style stylePushPin)
		{
			foreach (var pushpinLocation in allPushpins)
			{
				if (stops.Any(x => pushpinLocation.Stop.ID == x.ID))
					pushpinLocation.Pushpin.Style = stylePushPin;
			}
		}

		private bool isActive = true;

		public void Disable()
		{
			if (isActive)
			{
				isActive = false;
				RegistrMap(isActive);
			}
		}

		public void Activate()
		{
			if (!isActive)
			{
				isActive = true;
				RegistrMap(isActive);
			}
		}

		private void RegistrMap(bool registr)
		{
			if (registr)
			{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
				map.DoubleTapped += MapOnDoubleTapped;
				map.PointerWheelChanged += MapOnPointerWheelChanged;
#endif
				SetGPS();
			}
			else
			{
				StopGPS();
#if WINDOWS_PHONE_APP || WINDOWS_UAP

				map.DoubleTapped -= MapOnDoubleTapped;
				map.PointerWheelChanged -= MapOnPointerWheelChanged;
#endif
			}
		}
#if WINDOWS_PHONE_APP || WINDOWS_UAP

		private void MapOnPointerWheelChanged(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
		{
			
		}

		private void MapOnDoubleTapped(object sender, DoubleTappedRoutedEventArgs doubleTappedRoutedEventArgs)
		{
			map.TargetZoomLevel += 1;
		}
#endif
		public ISettingsModelView Settings
		{
			get { return settings; }
			set
			{
				if (value == null)
					return;
				if (settings != null)
					settings.PropertyChanged -= SettingsOnPropertyChanged;
				settings = value;

				settings.PropertyChanged += SettingsOnPropertyChanged;
				OnPropertyChanged();
			}
		}

		private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs.PropertyName == "UseGPS")
			{
				SetGPS();
			}
		}

		private async void SetGPS()
		{
//#if WINDOWS_UAP
//			var statusAccess = await Geolocator.RequestAccessAsync();
//			if (statusAccess == GeolocationAccessStatus.Denied)
//				return;
//#endif
			Context.SetGPS(settings.UseGPS);
				if (settings.UseGPS)
				{
					StartGPS();

				}
				else
				{
					StopGPS();
				}
			ShowICommand.RaiseCanExecuteChanged();
		}

		public void StartGPS()
		{
			try
			{
				geolocator.MovementThreshold = Settings.GPSThreshholdMeters;

				geolocator.ReportInterval = Settings.GPSInterval;
#if WINDOWS_PHONE_APP || WINDOWS_UAP
				geolocator.StatusChanged  += GeolocatorOnStatusChanged;
				geolocator.PositionChanged += GeolocatorOnPositionChanged;
#endif
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
		}

		public void StopGPS()
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			Ipushpin = null;
			ShowICommand.RaiseCanExecuteChanged();
			if (geolocator == null)
				return;
			geolocator.PositionChanged -= GeolocatorOnPositionChanged;
			geolocator.StatusChanged -= GeolocatorOnStatusChanged;
			geolocator = null;
#endif
		}

#if WINDOWS_PHONE_APP || WINDOWS_UAP
		private async void GeolocatorOnStatusChanged(object o, StatusChangedEventArgsArgs args)
		{
			if (args.Status == PositionStatus.Ready)
			{
				await map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					ipushpin = new Pushpin();
					ipushpin.Content = "Я";
				});
				ShowICommand.RaiseCanExecuteChanged();
			}
			else if (args.Status == PositionStatus.Disabled ||
					 args.Status == PositionStatus.NotAvailable)
			{
				Ipushpin = null;
				ShowICommand.RaiseCanExecuteChanged();
			}
		}

		private async void GeolocatorOnPositionChanged(object o, PositionChangedEventArgsArgs args)
		{
			await map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				if (Ipushpin == null)
					return;
				MapPanel.SetLocation(Ipushpin,
					new Location(args.NewLocation.Latitude, args.NewLocation.Longitude));
				RefreshPushPinsAsync();
			});
		}
#endif

		public Pushpin StartStopPushpin
		{
			get { return startStopPushpin; }
			set
			{
				startStopPushpin = value;
				OnPropertyChanged(nameof(StartStop));
			}
		}

		public Pushpin EndStopPushpin
		{
			get { return endStopPushpin; }
			set
			{
				endStopPushpin = value; 
				OnPropertyChanged(nameof(EndStop));
			}
		}

		public Stop StartStop => (Stop) StartStopPushpin?.Tag;

	    public Stop EndStop => (Stop) EndStopPushpin?.Tag;

	    protected void ShowOnMap()
		{
            ShowOnMap(Pushpins.ToArray());
		}

	    protected void ShowOnMap(Pushpin[] pins)
	    {
            var temp = map.Children.OfType<Pushpin>().ToArray();
            var except = temp.Except(pins).ToList();
            foreach (var pushpin in except)
            {
                map.Children.Remove(pushpin);
            }
            //map.Children.RemoveAt(i);
            except = pins.Except(temp).ToList();

            foreach (var pushpin in except)
            {
                map.Children.Add(pushpin);
            }
        }

		public ObservableCollection<Pushpin> Pushpins
		{
		    get
		    {
		        return pushpins1 ?? (pushpins1 = new ObservableCollection<Pushpin>());
		    }
		    set
			{
				if (Equals(value, pushpins1)) return;
				pushpins1 = value;
				OnPropertyChanged();
			}
		}

		public int MaxZoomLevel { get; set; }

		protected virtual PushpinLocation CreatePushpin(Stop st)
		{
			return new PushpinLocation
			{
				Location = new Location(st.Lat, st.Lng),
				Stop = st,
				
			};
		}

		protected virtual void PreperPushpinsForView(IEnumerable<Stop> needStops)
		{
			foreach (var needShowStop in needStops)
			{
				var tempPushpin = allPushpins.FirstOrDefault(push => push.Stop.ID == needShowStop.ID);
				if (tempPushpin == null)
				{
					tempPushpin = CreatePushpin(needShowStop);
					allPushpins.Add(tempPushpin);
				}
				Pushpins.Add(tempPushpin.Pushpin);
			}
		}

		public virtual void RefreshPushPinsAsync()
		{

			if (showAllPushpins && map != null && Context.Context.ActualStops != null)
			{
				double zoomLevel = map.ZoomLevel;
				Pushpins.Clear();
				if (zoomLevel <= MaxZoomLevel)
				{
					ShowOnMap();
					return;
				}
#if !(WINDOWS_PHONE_APP || WINDOWS_AP || WINDOWS_UWP)
                var northWest = map.ViewportPointToLocation(new Point(0, 0));
				var southEast = map.ViewportPointToLocation(new Point(map.ActualWidth, map.ActualHeight));
#else
				var northWest = map.ViewportPointToLocation(new Windows.Foundation.Point(0, 0));
				var southEast = map.ViewportPointToLocation(new Windows.Foundation.Point(map.ActualWidth, map.ActualHeight));
				
#endif

				var needShowStops =
					Context.Context.ActualStops.Where(child => child.Lat <= northWest.Latitude && child.Lng >= northWest.Longitude &&
					                                   child.Lat >= southEast.Latitude && child.Lng <= southEast.Longitude).ToList();

				PreperPushpinsForView(needShowStops);
				if (Ipushpin != null)
					Pushpins.Add(Ipushpin);
				ShowOnMap();
			}
		}

	    public string SearchPattern
	    {
	        get { return searchPattern; }
	        set
	        {
	            searchPattern = value;
	            OnPropertyChanged(nameof(FilteredStops));
	        }
	    }

	    public IList<Stop> FilteredStops
	    {
	        get
	        {
	            if (SearchPattern == null)
	                SearchPattern = "";
	            string filterPat = SearchPattern.ToLowerInvariant();
	            return Context.FilteredStops(filterPat,  location: Context.Geolocation.CurLocation).ToList();
	        }
	    }

		public Pushpin Ipushpin
		{
			get
			{
				return ipushpin;
			}
			set { ipushpin = value; }
		}

		public Stop CurrentStop
		{
			get { return currentStop; }
			set
			{
				if (Equals(value, currentStop)) return;
				currentStop = value;
				OnPropertyChanged();
			    if (value != null)
			        ShowStopCommand.Execute(currentStop);
			}
		}

		public Rout CurrentRout
		{
			get { return currentRout; }
			set
			{
				if (Equals(value, currentRout)) return;
				currentRout = value;
				OnPropertyChanged();
			}
		}

		public Location Location
		{
			get { return location; }
			set
			{
				//if (Equals(value, location)) return;
				location = value;
				OnPropertyChanged();
			}
		}

		public string ResultString
		{
			get { return resultString; }
			set
			{
				resultString = value; 
				OnPropertyChanged();
			}
		}

		public RelayCommand ShowAllStops
		{
			get
			{
				return new RelayCommand(() =>
				{
					showAllPushpins = true;
				});
			}
		}

		private RelayCommand showICommand;

		public RelayCommand ShowICommand
		{
			get
			{
			    return showICommand ?? 
                    (showICommand = new RelayCommand(() =>
                    {
                        ShowPushpin(Ipushpin);
                    }, () => Ipushpin != null));
			}
		}

		async void ShowPushpin(Pushpin push)
		{
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			await map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				map.TargetCenter = MapPanel.GetLocation(push);
			});
#endif
		}

		public RelayCommand<Rout> ShowRoutCommand
		{
			get
			{
				return new RelayCommand<Rout>(rout =>
				{

					showAllPushpins = false;
					
					Pushpins.Clear();
					PreperPushpinsForView(rout.Stops);
					ShowOnMap();
					map.Center = new Location(rout.StartStop.Lat, rout.StartStop.Lng);
				});
			}
		}

		public RelayCommand<Stop> ShowStopCommand
		{
			get
			{
				return new RelayCommand<Stop>(stop =>
				{
					showAllPushpins = true;
					map.Center = new Location(stop.Lat, stop.Lng);
					map.ZoomLevel = 19;
				});
			}
		}

		public RelayCommand<Pushpin> SetStartStop
		{
			get
			{
				return new RelayCommand<Pushpin>(pushpin =>
				{
					if (StartStopPushpin != null)
						StartStopPushpin.Style = StylePushpin;
					StartStopPushpin = pushpin;
					OnStartStopSeted();
				});
			}
		}
		public RelayCommand<Pushpin> SetEndtStop
		{
			get
			{
				return new RelayCommand<Pushpin>(pushpin =>
				{
					if (EndStopPushpin != null)
						EndStopPushpin.Style = StylePushpin;
					EndStopPushpin = pushpin;
					OnEndStopSeted();
				});
			}
		}

		public RelayCommand SwitchStopsCommand { get { return new RelayCommand(() =>
		{
			var tempStop = StartStopPushpin;
			StartStopPushpin = EndStopPushpin;
			EndStopPushpin = tempStop;
		});} }

		private RelayCommand calculateCommand;
	    private string searchPattern;

	    public RelayCommand CalculateRoutCommand
		{
			get
			{
			    return calculateCommand ?? (calculateCommand = new RelayCommand(() =>
			    {
			        CalculateRout calculator = new CalculateRout(Context.Context);
			        calculator.CreateGraph();
			        if (!calculator.FindPath(StartStop, EndStop))
			            ResultString = "Bad";
			        else
			        {
			            StringBuilder builder = new StringBuilder();
			            foreach (var keyValuePair in calculator.resultRout)
			            {
			                builder.Append(keyValuePair.Key.Transport);
			                builder.Append(" ");
			                builder.Append(keyValuePair.Key);
			                builder.Append('\n');
			                foreach (var stop in keyValuePair.Value)
			                {
			                    builder.Append(stop.Name);
			                    builder.Append(", ");
			                }
			                builder.Append("\n\n");
			            }
			        }
			    }));
			}
		}

		public bool IsActive
		{
			get { return isActive; }
			set
			{
				if (isActive == value)
					return;
				isActive = value;
				RegistrMap(isActive);
				OnPropertyChanged();
			}
		}

        public bool ShowStopsList { get; set; }

	    public RelayCommand ChangeShowStopList
	    {
	        get { return new RelayCommand(() => ShowStopsList = ShowStopsList); }
	    }

#region events

		public event EventHandler MapInicialized;
		public event EventHandler StartStopSeted;
		public event EventHandler EndStopSeted;

#endregion

		protected virtual void OnMapInicialized()
		{
			var handler = MapInicialized;
		    handler?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnStartStopSeted()
		{
			var handler = StartStopSeted;
		    handler?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnEndStopSeted()
		{
			var handler = EndStopSeted;
		    handler?.Invoke(this, EventArgs.Empty);
		}
	}
}
