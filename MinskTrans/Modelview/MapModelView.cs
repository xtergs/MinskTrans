using MyLibrary;
using System.Text;
using System.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
#if (WINDOWS_PHONE_APP || WINDOWS_AP || WINDOWS_UWP)
using Windows.UI.Xaml.Media;
#endif
using MapControl;
using MinskTrans.AutoRouting.AutoRouting;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context;
using MinskTrans.Context.AutoRouting;
using MinskTrans.Context.Comparer;
using MinskTrans.DesctopClient.Model;
using Newtonsoft.Json;
using PropertyChanged;
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
	public struct StartEndRout
	{
		public Stop StartStop { get; set; }
		public Stop EndStop { get; set; }
	}
	[ImplementPropertyChanged]
	public class MapModelView : BaseModelView
	{
		private string settingFile = "settings.json";
		public ObservableCollection<StartEndRout> SavedPath { get; private set; } = new ObservableCollection<StartEndRout>();
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

	    private CalculateRout routeCreator;

        private IGeolocation geolocator;

        public IGeolocation Geolocator => geolocator;

        public delegate MapModelView MapModelViewFactory(Map map, PushPinBuilder pushPinBuilder);

        private MapModelView(IBussnessLogics context)
            : base(context)
        {
        }

        public static Style StylePushpin { get; set; }

        protected readonly PushPinBuilder pushBuilder;

        //Design only!!!!
        public MapModelView()
        {
        }

        public MapModelView(IBussnessLogics context, Map map, ISettingsModelView newSettigns, IGeolocation geolocation,
            StopModelView stopModelView, PushPinBuilder pushPinBuilder = null)
            : base(context)
        {
            if (stopModelView == null)
                throw new ArgumentNullException(nameof(stopModelView));
            this.map = map;
            this.StopModelView = stopModelView;
            StopModelView.PropertyChanged += (sender, args) =>
            {
                if (StopModelView.FilteredSelectedStop != null &&
                    args.PropertyName == nameof(StopModelView.FilteredSelectedStop))
                    ShowStopCommand.Execute(StopModelView.FilteredSelectedStop);
            };
            pushBuilder = pushPinBuilder;
			pushPinBuilder.Tapped += Tapped;
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
            StopModelView.Refresh();

			routeCreator = new CalculateRout(base.context.Context);
			routeCreator.CreateGraph();

        }

#if WINDOWS_UWP && WINDOWS_PHONE_APP
		private void Tapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
		{
			throw new NotImplementedException();
		}
#else
		private void Tapped(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			var source = (Pushpin) sender;
			ContextMenu menu = new ContextMenu();
			menu.Items.Add(new MenuItem() {Header = ((Stop) source.Tag).ID});
			menu.Items.Add(new MenuItem() {Header = "start Stop", Command = SetStartStop, CommandParameter = source});
			menu.Items.Add(new MenuItem() {Header = "End stop", Command = SetEndtStop, CommandParameter = source });
			
			menu.PlacementTarget = source;
			menu.IsOpen = true;
		}
#endif

		struct MapSerializeSettings
		{
			public CalculateParameters Parameter;
			public ObservableCollection<StartEndRout> routes;
		}

		public void LoadMapSettings()
		{
			if (!File.Exists(settingFile))
			{
				routeCreator.Params = new CalculateParameters();
				return;
			}
			var json = File.ReadAllText(settingFile);
			var obj = JsonConvert.DeserializeObject<MapSerializeSettings>(json);

			routeCreator.Params = obj.Parameter;
			SavedPath = obj.routes;

			OnPropertyChanged(nameof(MaxHumanStops));
			OnPropertyChanged(nameof(MaxHumanDistanceM));
			OnPropertyChanged(nameof(HumanMultipl));

			routeCreator.CreateGraph();
		}

		public void SaveMapSettings()
		{
			var obj = new MapSerializeSettings()
			{
				Parameter = routeCreator.Params,
				routes = SavedPath
			};
			var json = JsonConvert.SerializeObject(obj);
			File.WriteAllText(settingFile, json);
		}

		public StartEndRout SelectedLine { get; set; }
		public RelayCommand AddToRoutes
		{
			get
			{
				return new RelayCommand(() =>
				{
					SavedPath.Add(new StartEndRout() { StartStop = StartStop, EndStop = EndStop});
				}, ()=>
				{
					return StartStop != null && EndStop != null &&
					       !SavedPath.Any(x => x.StartStop.ID == StartStop.ID && x.EndStop.ID == EndStop.ID);
				});
			}
		}

		public RelayCommand RemoveFromRoutes
		{
			get { return new RelayCommand(() =>
			{
				SavedPath.Remove(SelectedLine);
			}, ()=> SavedPath.Any(x=> x.StartStop?.ID == SelectedLine.StartStop?.ID && x.EndStop?.ID == SelectedLine.EndStop?.ID));}
		}

		public RelayCommand SetCurrentRout
		{
			get
			{
				return new RelayCommand(() =>
				{

					StartStopPushpin = allPushpins.First(p => p.Stop.ID == SelectedLine.StartStop?.ID).Pushpin;
					EndStopPushpin = allPushpins.First(p => p.Stop.ID == SelectedLine.EndStop?.ID).Pushpin;
				}, () =>
				{
					return allPushpins != null && allPushpins.Any(p => p.Stop.ID == SelectedLine.StartStop?.ID) &&
					       allPushpins.Any(p => p.Stop.ID == SelectedLine.EndStop?.ID);
				});
			}
		}

		public StopModelView StopModelView
        {
            get { return _stopModelView; }
            set
            {
                _stopModelView = value;
                OnPropertyChanged();
            }
        }

		private List<PushpinLocation> markedStops = new List<PushpinLocation>(); 
        public void MarkPushPins(IEnumerable<Stop> stops, Style stylePushPin)
        {
	        foreach (var p in markedStops)
	        {
		        p.Pushpin.Style = StylePushpin;
	        }
	        markedStops.Clear();
            foreach (var pushpinLocation in allPushpins)
            {
	            if (stops.Any(x => pushpinLocation.Stop.ID == x.ID))
	            {
		            pushpinLocation.Pushpin.Style = stylePushPin;
		            markedStops.Add(pushpinLocation);
	            }
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

        private void SetGPS()
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
                if (unchecked((uint) ex.HResult == 0x80004004))
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
			if (IsShowStopConnections)
				DrawStopsWeb();

        }

        protected void ShowOnMap(Pushpin[] toRemove, Pushpin[] toAdd)
        {
            foreach (var pushpin in toRemove)
            {
                map.Children.Remove(pushpin);
            }
            foreach (var pushpin in toAdd)
            {
                try
                {
                    map.Children.Add(pushpin);
                }
                catch (COMException e)
                {
                    Debug.WriteLine("Error while updating map\n");
                    Debug.WriteLine(e.Message);
                    //StopsOnMap.Add(pushpin);
                }
            }
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
            //if (Ipushpin != null)
            //    map.Children.Add(Ipushpin);
        }

        public ObservableCollection<Pushpin> Pushpins
        {
            get { return pushpins1 ?? (pushpins1 = new ObservableCollection<Pushpin>()); }
            set
            {
                if (Equals(value, pushpins1)) return;
                pushpins1 = value;
                OnPropertyChanged();
            }
        }

        protected Pushpin[] stopsOnMap = new Pushpin[0];

        public int MaxZoomLevel { get; set; }

        protected virtual PushpinLocation CreatePushpin(Stop st)
        {
            var xx =  new PushpinLocation
            {
                Location = new Location(st.Lat, st.Lng),
                Stop = st,
			
            };

	        xx.Pushpin.MouseRightButtonUp += Tapped;

	        return xx;
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
                    Context.Context.ActualStops.Where(
                        child => child.Lat <= northWest.Latitude && child.Lng >= northWest.Longitude &&
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
                return Context.FilteredStops(filterPat, location: Context.Geolocation.CurLocation).ToList();
            }
        }

        public Pushpin Ipushpin
        {
            get { return ipushpin; }
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
            get { return new RelayCommand(() => { showAllPushpins = true; }); }
        }

        private RelayCommand showICommand;

        public RelayCommand ShowICommand
        {
            get
            {
                return showICommand ??
                       (showICommand = new RelayCommand(() => { ShowPushpin(Ipushpin); }, () => Ipushpin != null));
            }
        }

        private async void ShowPushpin(Pushpin push)
        {
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			await map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				map.TargetCenter = MapPanel.GetLocation(push);
			});
#endif
        }

		public bool IsShowStopConnections
		{
			get { return _isShowStopConnections; }
			set
			{
				if (!value)
					ClearDrawedStops();
				else
					DrawStopsWeb();
				_isShowStopConnections = value;
			}
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

        public RelayCommand SwitchStopsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var tempStop = StartStopPushpin;
                    StartStopPushpin = EndStopPushpin;
                    EndStopPushpin = tempStop;
                });
            }
        }

        private RelayCommand calculateCommand;
        private string searchPattern;
        private StopModelView _stopModelView;
		private bool _isShowStopConnections;
		private int _maxHumanStops;
		private int _thickness = 1;

		public RelayCommand RecreateGraphCommand
		{
			get { return new RelayCommand(() =>
			{
				routeCreator.CreateGraph();
			});}
		}

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

	    public RelayCommand DrawStopsWebCommand => new RelayCommand(DrawStopsWeb);

	    List<string> RoutsToList(IEnumerable<Rout> list)
	    {
		    var grouped = list.GroupBy(x => x.Transport).ToList();
			List<string> strings = new List<string>(grouped.Count());
		    foreach (var group in grouped)
		    {
			    string str = "\n" + group.Key.ToString() + "\n";
			    foreach (var tr in group.Distinct(new RoutNameComparer()).ToList())
			    {
				    str += $"{tr.RouteNum},";
			    }
				strings.Add(str);
		    }
		    return strings;
	    }

		public CalculateRout RouteCreator {
			get { return routeCreator; }
		}

		public int MaxHumanStops
		{
			get { return routeCreator.Params.MaxHumanStops; }
			set
			{
				routeCreator.Params.MaxHumanStops = value;
			}
		}

		public double HumanMultipl
		{
			get { return routeCreator.Params.HumanMultipl; }
			set { routeCreator.Params.HumanMultipl = value; }
		}
		public int MaxHumanDistanceM
		{
			get { return routeCreator.Params.MaxHumanDistanceM; }
			set { routeCreator.Params.MaxHumanDistanceM = value; }
		}

		public int Thickness
		{
			get { return _thickness; }
			set
			{
				_thickness = value;
				ReWrawStopsWeb();
			}
		}
		

		public void ReWrawStopsWeb()
		{
			if (!IsShowStopConnections)
				return;
			foreach(var line in linesOnMap)
			{
				map.Children.Remove(line);
			}
			linesOnMap.Clear();
			DrawStopsWeb();
		}

		private List<MapPolylineEx> tracedLines = new List<MapPolylineEx>();

		public async void TraceStops(List<Stop> stops)
		{
			foreach (var line in tracedLines.Where(x=> !stops.Any(s=> s.ID == x.StopStart.ID)).ToList())
			{
				map.Children.Remove(line);
				tracedLines.Remove(line);
			}

			tracedLines = await DrawStopsWeb(stops);
		}
		private async Task<List<MapPolylineEx>> DrawStopsWeb(List<Stop> stops)
		{
			var currentLInes = map.Children.OfType<MapPolylineEx>().ToList();
			List<MapPolylineEx> toDelete = null;
			IEnumerable<MapPolylineEx> noNeedToAdd = null;
			List<MapPolylineEx> needLines = new List<MapPolylineEx>();

			Random rand = new Random();
			byte[] rgb = new byte[3];

			try
			{
				foreach (var st in stops)

					foreach (var stopConnection in routeCreator.StopConnections[st.ID])
					{
						rand.NextBytes(rgb);
						var routs = Context.GetIntersectionRoutsByStops(st, stopConnection.Stop);
						MapPolylineEx poly = new MapPolylineEx()
						{
							Locations =
								new[]
								{
									new Location(st.Lat, st.Lng),
									new Location() {Latitude = stopConnection.Stop.Lat, Longitude = stopConnection.Stop.Lng}
								},
							StrokeThickness = Thickness,

							Stroke = new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2]))
							,
							StopStart = st,
							StopEnd = stopConnection.Stop,
							ContextMenu = new ContextMenu()
							{
								ItemsSource = RoutsToList(routs).Select(x => new MenuItem() {Header = x}).Concat(new[]
								{
									new MenuItem() {Header = $"Distance: {stopConnection.Distance}"},
									new MenuItem() {Header = $"Time: {Context.Context.GetAvgRoutTime(st, stopConnection.Stop)}"},
									new MenuItem()
									{
										Header = $"Distance by Time: {Context.Context.GetAvgRoutTime(st, stopConnection.Stop)*(20*1000/60)}"
									},
								}).ToList(),

							}
						};

						if (stopConnection.Type == ConnectionType.Human)
							poly.StrokeDashArray = new DoubleCollection() {2, 2};

						needLines.Add(poly);
					}
			}
			catch (NullReferenceException e)
			{

				throw;
			}

			var comparer = new PolylineComparer();

			var intersect = needLines.Intersect(currentLInes, comparer).ToList();
			toDelete = intersect.Except(currentLInes, comparer).ToList();
			noNeedToAdd = needLines.Except(intersect, comparer);
			foreach (var mapPolyline in toDelete)
			{
				map.Children.Remove(mapPolyline);
			}
			foreach (var source in noNeedToAdd)
			{
				map.Children.Add(source);
			}

			return needLines;
		}

		private List<MapPolylineEx> linesOnMap = new List<MapPolylineEx>();
		public async void DrawStopsWeb()
	    {
			var puhs = map.Children.OfType<Pushpin>().ToList();
			linesOnMap = await DrawStopsWeb(puhs.Select(p => allPushpins.First(x => x.Pushpin == p).Stop).ToList());
	    }

		public void ClearDrawedStops()
		{
			var currentLInes = map.Children.OfType<MapPolylineEx>().ToList();
			foreach (var line in currentLInes)
			{
				map.Children.Remove(line);
			}
		}

        public bool ShowStopsList { get; set; }

        public RelayCommand ChangeShowStopList
        {
            get { return new RelayCommand(() => ShowStopsList = ShowStopsList); }
        }

        protected Pushpin[] StopsOnMap
        {
            get { return stopsOnMap; }
            set { Interlocked.Exchange(ref stopsOnMap, value); }
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

	class PolylineComparer : IEqualityComparer<MapPolylineEx>
	{
		public bool Equals(MapPolylineEx x, MapPolylineEx y)
		{
			return x.StopStart.ID == y.StopStart.ID && x.StopEnd.ID == y.StopEnd.ID;
		}

		public int GetHashCode(MapPolylineEx obj)
		{
			return 0;
		}
	}
}