






using System.Text;
using System.ComponentModel;
using System;

using MinskTrans.DesctopClient.Model;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MapControl;
using MinskTrans.Universal;



#if (WINDOWS_PHONE_APP )
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Command;

using Windows.Devices.Geolocation;
#else
using MinskTrans.DesctopClient.Properties;
using System.Windows.Controls;
using System.Windows.Controls;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
#endif
//using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;

namespace MinskTrans.DesctopClient.Modelview
{
	public class MapModelView: BaseModelView
	{
		private Stop currentStop;
		private Rout currentRout;
		private Location location;
		private bool pushpinsAll = true;
		private readonly Map map;
		private List<PushpinLocation> pushpins;
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


		private MapModelView(Context context)
			:base(context)
		{
			
		}

		public static Style StylePushpin { get; set; }

		public MapModelView(Context context, Map map, SettingsModelView newSettigns = null)
			: base(context)
		{
			this.map = map;
			Settings = newSettigns;
			map.ViewportChanged += (sender, args) => RefreshPushPinsAsync();
			geolocator = new Geolocator();

			MaxZoomLevel = 14;
			map.ZoomLevel = 19;
			map.Center = new Location(53.55, 27.33);
			
			SetGPS();
		}

		public SettingsModelView Settings
		{
			get { return settings; }
			set
			{
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
			if (settings.UseGPS)
			{
				try
				{
					if (geolocator == null)
						geolocator = new Geolocator();
					geolocator.MovementThreshold = Settings.GPSThreshholdMeters;

					geolocator.ReportInterval = Settings.GPSInterval;
#if WINDOWS_PHONE_APP
					geolocator.StatusChanged += GeolocatorOnStatusChanged;
					geolocator.PositionChanged += GeolocatorOnPositionChanged;
#endif
				}
				catch (Exception ex)
				{
					if (unchecked ((uint)ex.HResult == 0x80004004))
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
			else
			{
				StopGPS();
			}
			ShowICommand.RaiseCanExecuteChanged();
		}

		public void StopGPS()
		{
#if WINDOWS_PHONE_APP
			geolocator.PositionChanged -= GeolocatorOnPositionChanged;
			geolocator.StatusChanged -= GeolocatorOnStatusChanged;
			Ipushpin = null;
			ShowICommand.RaiseCanExecuteChanged();
			geolocator = null;
#endif
		}

#if WINDOWS_PHONE_APP
		private void GeolocatorOnStatusChanged(Geolocator sender, StatusChangedEventArgs args)
		{
			if (args.Status == PositionStatus.Ready)
			{
				map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

		private void GeolocatorOnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
		{
			map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
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
				catch (System.Exception ex)
				{ }
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

		public async void RefreshPushPinsAsync()
		{

			if (pushpins == null)
				InicializeMap();
			if (pushpinsAll && map != null && pushpins != null)
			{
#if WINDOWS_PHONE_APP
				var northWest = map.ViewportPointToLocation(new Windows.Foundation.Point(0, 0));
				var southEast = map.ViewportPointToLocation(new Windows.Foundation.Point(map.ActualWidth, map.ActualHeight));
#else
				var northWest = map.ViewportPointToLocation(new Point(0, 0));
				var southEast = map.ViewportPointToLocation(new Point(map.ActualWidth, map.ActualHeight));
				
#endif
				double zoomLevel = map.ZoomLevel;
				Pushpins.Clear();

				//await Task.Run(() =>
				//{
				foreach (var child in pushpins.AsParallel())
				{
					if (zoomLevel <= MaxZoomLevel)
					{
						ShowOnMap();
						return;
						//map.Children.Remove(child);
						//child.Visibility = Visibility.Collapsed;
					}
					else
					{
						//var x = MapPanel.GetLocation(child);
						if (child.Location.Latitude <= northWest.Latitude && child.Location.Longitude >= northWest.Longitude &&
							child.Location.Latitude >= southEast.Latitude && child.Location.Longitude <= southEast.Longitude)
						{
							Pushpins.Add(child.Pushpin);
						}
					}
				}
				if (Ipushpin != null)
					Pushpins.Add(Ipushpin);
				//});
				ShowOnMap();
			}


		}



		public void InicializeMap()
		{
			if (Context != null && Context.ActualStops != null)
			{
				pushpins = new List<PushpinLocation>(Context.ActualStops.Count);
				foreach (var st in Context.ActualStops)
				{
					var tempPushPin = new PushpinLocation() { Location = new Location(st.Lat, st.Lng) };
					//var pushpin = new Pushpin { Tag = st, Content = st.Name };
					tempPushPin.Style = StylePushpin;
					tempPushPin.Stop = st;
#if WINDOWS_PHONE_APP
					//pushpin.Tapped += (sender, argss) =>
					//{
					//	((Pushpin)sender).BringToFront();
					//};
					//pushpin.Tapped += (o, argss) =>
					//{
					//	Pushpin tempPushpin = (Pushpin)o;
					//	Stop tmStop = (Stop)tempPushpin.Tag;
					//	//model.StopMovelView.FilteredSelectedStop = tmStop;
					//	//MapPivotItem.Focus(FocusState.Programmatic);
					//};
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
					pushpins.Add(tempPushPin);
				}
				
				map.Center = new Location(Context.ActualStops.First().Lat, Context.ActualStops.First().Lng);
				OnMapInicialized();
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
					pushpinsAll = true;
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
					}, () =>
					{
						return Ipushpin != null;
					});
				return showICommand;
			}
		}

		void ShowPushpin(Pushpin push)
		{
#if WINDOWS_PHONE_APP
			map.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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

					pushpinsAll = false;
					if (pushpins == null)
						InicializeMap();
					Pushpins.Clear();
					foreach (var child in pushpins.Where(d => rout.Stops.Any(p => p.ID == ((Stop)d.Pushpin.Tag).ID)).Select(d => d.Pushpin))
					{
						Pushpins.Add(child);
					}
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
					pushpinsAll = true;
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


		#region events

		public event Context.EmptyDelegate MapInicialized;
		public event Context.EmptyDelegate StartStopSeted;
		public event Context.EmptyDelegate EndStopSeted;

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
