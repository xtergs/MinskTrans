






using MyLibrary;
using System.Text;
using System.ComponentModel;
using System;

using MinskTrans.DesctopClient.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Command;
using MapControl;
using MinskTrans.AutoRouting.AutoRouting;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Universal;
#if (WINDOWS_PHONE_APP || WINDOWS_UAP)
using Windows.UI.Xaml.Input;
using MinskTrans.Universal;
using Windows.UI.Core;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Command;

using Windows.Devices.Geolocation;
#else

using MinskTrans.DesctopClient.Properties;
using System.Windows.Controls;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
#endif
//using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

namespace MinskTrans.DesctopClient.Modelview
{
	public class MapModelView: BaseModelView
	{
		private Stop currentStop;
		private Rout currentRout;
		private Location location;
		private bool showAllPushpins = true;
		private readonly Map map;
		private List<PushpinLocation> allPushpins;
		private Pushpin ipushpin;
		private ObservableCollection<Pushpin> pushpins1;
		private Pushpin startStopPushpin;
		private Pushpin endStopPushpin;
		private string resultString;
		private SettingsModelView settings;

		private Geolocator geolocator;

		public Geolocator  Geolocator
		{
			get { return geolocator;}
		}


		private MapModelView(IContext context)
			:base(context)
		{
			
		}

		public static Style StylePushpin { get; set; }

		private PushPinBuilder pushBuilder;

		public MapModelView(IContext context, Map map, SettingsModelView newSettigns, PushPinBuilder pushPinBuilder = null)
			: base(context)
		{
			this.map = map;
			pushBuilder = pushPinBuilder;
			Settings = newSettigns;
			map.ViewportChanged += (sender, args) => RefreshPushPinsAsync();
			

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
		public SettingsModelView Settings
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
#if WINDOWS_UAP
			var statusAccess = await Geolocator.RequestAccessAsync();
			if (statusAccess == GeolocationAccessStatus.Denied)
				return;
#endif
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
				if (geolocator == null)
				{
					geolocator = new Geolocator();
				}
				geolocator.MovementThreshold = Settings.GPSThreshholdMeters;

				geolocator.ReportInterval = Settings.GPSInterval;
#if WINDOWS_PHONE_APP || WINDOWS_UAP
				geolocator.StatusChanged += GeolocatorOnStatusChanged;
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
		private async void GeolocatorOnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
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

		private async void GeolocatorOnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
		{
			await map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				if (Ipushpin == null)
					return;
				MapPanel.SetLocation(Ipushpin,
					new Location(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude));
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
				OnPropertyChanged("StartStop");
			}
		}

		public Pushpin EndStopPushpin
		{
			get { return endStopPushpin; }
			set
			{
				endStopPushpin = value; 
				OnPropertyChanged("EndStop");
			}
		}

		public Stop StartStop
		{
			get
			{
				if (StartStopPushpin!= null)
					return (Stop) StartStopPushpin.Tag;
				return null;
			}
		}

		public Stop EndStop
		{
			get
			{
				if (EndStopPushpin != null)
					return (Stop) EndStopPushpin.Tag;
				return null;
			}
		}

		void ShowOnMap()
		{
			var temp = map.Children.OfType<Pushpin>();
			var except =  temp.Except(Pushpins).ToList();
			foreach (var pushpin in except)
			{
				map.Children.Remove(pushpin);
			}
					//map.Children.RemoveAt(i);
			except = Pushpins.Except(temp).ToList();
			
			foreach (var pushpin in except)
			{
				try
				{
					map.Children.Add(pushpin);
				}
				catch (System.Exception)
				{
					throw;
				}
			}
		}

		public ObservableCollection<Pushpin> Pushpins
		{
			get
			{
				if (pushpins1 == null)
					pushpins1 = new ObservableCollection<Pushpin>();
				return pushpins1;
			}
			set
			{
				if (Equals(value, pushpins1)) return;
				pushpins1 = value;
				OnPropertyChanged();
			}
		}

		public int MaxZoomLevel { get; set; }

		PushpinLocation CreatePushpin(Stop st)
		{
			var tempPushPin = new PushpinLocation
			{
				Location = new Location(st.Lat, st.Lng),
				Stop = st,
				
			};
			//var pushpin = new Pushpin { Tag = st, Content = st.Name };
#if WINDOWS_PHONE_APP || WINDOWS_UAP
			if (pushBuilder != null)
			{
				tempPushPin.Pushpin = pushBuilder.CreatePushPin(tempPushPin.Location);
				tempPushPin.Pushpin.Tag = st;
				tempPushPin.Pushpin.Content = st.Name;
			}
#else
					tempPushPin.Pushpin.ContextMenu = new ContextMenu();
					var menuItem = new MenuItem();
					menuItem.Command = SetStartStop;
					menuItem.CommandParameter = tempPushPin.Pushpin;
					//menuItem.Click += ContextClickStartStop;
					menuItem.Header = "Start";
					tempPushPin.Pushpin.ContextMenu.Items.Add(menuItem);
					menuItem = new MenuItem();
					menuItem.Command = SetEndtStop;
					menuItem.CommandParameter = tempPushPin.Pushpin;
					//menuItem.Click += ContextClickEndStop;
					menuItem.Header = "End";
					tempPushPin.Pushpin.ContextMenu.Items.Add(menuItem);
					tempPushPin.Pushpin.MouseMove += (senderr, argsr) =>
					{
						((Pushpin)senderr).BringToFront();
					};
#endif
			//tempPushPin.Pushpin.MouseLeftButtonDown += (o, argsr) =>
			//{
			//	Pushpin tempPushpin = (Pushpin)o;
			//	Stop tmStop = (Stop)tempPushpin.Tag;
			//	ShedulerModelView.StopMovelView.FilteredSelectedStop = tmStop;
			//	stopTabItem.Focus();
			//};
			//tempPushPin.Pushpin.MouseRightButtonDown += (o, eventArgs) =>
			//{
			//	Pushpin tempPushpin = (Pushpin)o;
			//	tempPushpin.ContextMenu.IsOpen = true;
			//	currentPushpin = (Pushpin)o;
			//};
			//MapPanel.SetLocation(tempPushPin.Pushpin, tempPushPin.Location);
			return tempPushPin;
		}

		void PreperPushpinsForView(IEnumerable<Stop> needStops)
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

		public void RefreshPushPinsAsync()
		{

			if (showAllPushpins && map != null && Context.ActualStops != null)
			{
				double zoomLevel = map.ZoomLevel;
				Pushpins.Clear();
				if (zoomLevel <= MaxZoomLevel)
				{
					ShowOnMap();
					return;
				}
#if WINDOWS_PHONE_APP || WINDOWS_UAP
				var northWest = map.ViewportPointToLocation(new Windows.Foundation.Point(0, 0));
				var southEast = map.ViewportPointToLocation(new Windows.Foundation.Point(map.ActualWidth, map.ActualHeight));
#else
				var northWest = map.ViewportPointToLocation(new Point(0, 0));
				var southEast = map.ViewportPointToLocation(new Point(map.ActualWidth, map.ActualHeight));
				
#endif

				var needShowStops =
					Context.ActualStops.Where(child => child.Lat <= northWest.Latitude && child.Lng >= northWest.Longitude &&
					                                   child.Lat >= southEast.Latitude && child.Lng <= southEast.Longitude).ToList();

				PreperPushpinsForView(needShowStops);
				if (Ipushpin != null)
					Pushpins.Add(Ipushpin);
				ShowOnMap();
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
				if (showICommand == null)
					showICommand = new RelayCommand(() =>
					{
						ShowPushpin(Ipushpin);
					}, () => Ipushpin != null);
				return showICommand;
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

		public RelayCommand CalculateRoutCommand
		{
			get
			{

				if (calculateCommand == null)
				{
					calculateCommand = new RelayCommand(() =>
					{
						CalculateRout calculator = new CalculateRout(Context);
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
					});
				}
				return calculateCommand;
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

#region events

		public event EventHandler MapInicialized;
		public event EventHandler StartStopSeted;
		public event EventHandler EndStopSeted;

#endregion

		protected virtual void OnMapInicialized()
		{
			var handler = MapInicialized;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnStartStopSeted()
		{
			var handler = StartStopSeted;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnEndStopSeted()
		{
			var handler = EndStopSeted;
			if (handler != null) handler(this, EventArgs.Empty);
		}
	}
}
