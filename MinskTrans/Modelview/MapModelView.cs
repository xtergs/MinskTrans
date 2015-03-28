



using System.Windows.Controls;
using RelayCommand = GalaSoft.MvvmLight.CommandWpf.RelayCommand;
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
using GalaSoft.MvvmLight.CommandWpf;


#if (WINDOWS_PHONE_APP )
using Windows.UI.Xaml;
using MinskTrans.DesctopClient.Properties;
using System.Windows.Controls;

#else
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

		private MapModelView(Context context)
			:base(context)
		{
			
		}

		public static Style StylePushpin { get; set; }

		public MapModelView(Context context, Map map)
			: base(context)
		{
			this.map = map;
			map.ViewportChanged += (sender, args) => RefreshPushPinsAsync();
			

			MaxZoomLevel = 14;
			map.ZoomLevel = 19;
			map.Center = new Location(53.55, 27.33);
			
		}

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
				var northWest = map.ViewportPointToLocation(new Point(0, 0));
				var southEast = map.ViewportPointToLocation(new Point(map.ActualWidth, map.ActualHeight));
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
#endif
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
				if (ipushpin == null)
					ipushpin = new Pushpin() { Content = "Я" };
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
