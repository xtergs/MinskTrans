



using System.Collections.Generic;
using System.Linq;
using MapControl;
#if (WINDOWS_PHONE_APP )
using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml;
using MinskTrans.Universal.Common;
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
		private List<Pushpin> pushpins;

		private MapModelView(Context context)
			:base(context)
		{
			
		}

		public static Style StylePushpin { get; set; }

		public MapModelView(Context context, Map map)
			: base(context)
		{
			this.map = map;

			Inicialize();

			//Context.PropertyChanged += (sender, args) => Inicialize();
			map.ZoomLevel = 19;
			map.Center = new Location(53.55, 27.33);
		}

		public void Inicialize()
		{
			if (Context.ActualStops != null)
			{
				pushpins = new List<Pushpin>(Context.ActualStops.Count);
				foreach (var st in Context.ActualStops)
				{
					var pushpin = new Pushpin {Tag = st, Content = st.Name};
					//pushpin.templ
#if WINDOWS_PHONE_APP
					pushpin.Tapped += (sender, argss) =>
					{
						((Pushpin) sender).BringToFront();
					};
					pushpin.Tapped += (o, argss) =>
					{
						Pushpin tempPushpin = (Pushpin) o;
						Stop tmStop = (Stop) tempPushpin.Tag;
						//model.StopMovelView.FilteredSelectedStop = tmStop;
						//MapPivotItem.Focus(FocusState.Programmatic);
					};
#endif
					MapPanel.SetLocation(pushpin, new Location(st.Lat, st.Lng));
					pushpins.Add(pushpin);
					map.Children.Add(pushpin);


				}
			}
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

		public void RefreshPushPins()
		{
			if (pushpins == null)
				return;
			if (pushpinsAll)
				foreach (var child in pushpins)
				{
					if (map.ZoomLevel <= 20)
					{
						child.Visibility = Visibility.Collapsed;
					}
					else
					{
						child.Visibility = Visibility.Visible;
					}



				}
		}

		public RelayCommand<Rout> ShowRout
		{
			get { return new RelayCommand<Rout>(x =>
			{
				CurrentRout = x;
				pushpinsAll = false;
				foreach (var child in pushpins)
				{
					child.Visibility = Visibility.Collapsed;
				}
				//var tempRoute = args.SelectedRoute;
				foreach (var child in pushpins.Where(d => x.Stops.Any(p =>p.ID == ((Stop)d.Tag).ID)))
				{
					child.Visibility = Visibility.Visible;
				}
				Location = new Location(x.StartStop.Lat, x.StartStop.Lng);
			}, x=> x != null);}
		}

		public RelayCommand<Stop> ShowStop
		{
			get
			{
				return new RelayCommand<Stop>(x => {
					                                   CurrentStop = x;
													   map.ZoomLevel = 19;
					Location = new Location(x.Lat, x.Lng);
				}, x => x != null);
			}
		}
		
	}
}
