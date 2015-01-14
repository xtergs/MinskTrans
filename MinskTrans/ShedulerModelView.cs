using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MinskTrans.Annotations;

namespace MinskTrans
{
	public class ShedulerModelView : INotifyPropertyChanged
	{
		private ObservableCollection<Stop> stops;
		private ObservableCollection<Rout> routs;
		private ObservableCollection<Schedule> times;
		private string routNum;
		private int stopIndex;
		private ObservableCollection<Time> timeT;
		private ObservableCollection<Rout> routT;
		private ObservableCollection<Stop> stopT;

		private ObservableCollection<string> routeNums;
		private int selectedRouteNumIndex;
		private new ObservableCollection<Rout> routeObservableCollection;

		private CollectionViewSource RoutView;
		private int routeNamesIndex;
		private List<Stop> stopsObservableCollection;
		private List<Time> timesObservableCollection;
		private int stopsIndex;

		public ObservableCollection<string> RouteNums
		{
			get
			{
				routeNums =	new ObservableCollection<string>(Routs.Select(x => x.RouteNum).Distinct());
				return routeNums;
			}
		}

		public int SelectedRouteNumIndex
		{
			get { return selectedRouteNumIndex; }
			set
			{
				if (value < 0)
					value = 0;
				selectedRouteNumIndex = value;
				OnPropertyChanged();
				OnPropertyChanged("RouteNames");
			}
		}

		public ObservableCollection<Rout> RouteNames
		{
			get
			{
				routeObservableCollection = new ObservableCollection<Rout>(Routs.Where(x=>x.RouteNum == routeNums[SelectedRouteNumIndex]));
				return routeObservableCollection;
			}
		}

		public int RouteNamesIndex
		{
			get { return routeNamesIndex; }
			set
			{
				if (value == routeNamesIndex) return;
				if (value < 0)
					value = 0;
				routeNamesIndex = value;
				OnPropertyChanged();
				OnPropertyChanged("StopsObservableCollection");
			}
		}

		public List<Stop> StopsObservableCollection
		{
			get
			{
				stopsObservableCollection = (RouteNames[RouteNamesIndex].Stops);
				return stopsObservableCollection;
			}
			set
			{
				if (Equals(value, stopsObservableCollection)) return;
				stopsObservableCollection = value;
				OnPropertyChanged();
			}
		}

		public int StopsIndex
		{
			get { return stopsIndex; }
			set
			{
				if (value == stopsIndex) return;
				if (value < 0)
					value = 0;
				stopsIndex = value;
				OnPropertyChanged();
				OnPropertyChanged("TimesObservableCollection");
			}
		}

		public List<Time> TimesObservableCollection
		{
			get
			{
				var tempList = RouteNames[RouteNamesIndex].Time;
				if (tempList == null)
					return null;
				timesObservableCollection = tempList.TimesDictionary[StopsIndex];
				return timesObservableCollection;
			}
			set
			{
				if (Equals(value, timesObservableCollection)) return;
				timesObservableCollection = value;
				OnPropertyChanged();
			}
		}

		public CollectionViewSource RoutViewSource
		{
			get { return RoutView; }
			set
			{
				RoutView = value;
				OnPropertyChanged();
			}
		}

		public string RoutNum
		{
			get { return routNum; }
			set
			{
				if (value == routNum) return;
				routNum = value;
				OnPropertyChanged();
			}
		}

		public int StopIndex
		{
			get { return stopIndex; }
			set
			{
				if (value == stopIndex) return;
				stopIndex = value;
				OnPropertyChanged();
			}
		}

		public ShedulerModelView()
		{
			WebClient client = new WebClient();

			client.DownloadFile(@"http://www.minsktrans.by/city/minsk/stops.txt", "stops.txt");
			ParseStopsCommand.Execute(File.ReadAllText("stops.txt"));

			client.DownloadFile(@"http://www.minsktrans.by/city/minsk/routes.txt", "routes.txt");
			ParseRoutsCommand.Execute(File.ReadAllText("routes.txt"));

			client.DownloadFile(@"http://www.minsktrans.by/city/minsk/times.txt", "times.txt");
			ParseTimesCommand.Execute(File.ReadAllText("times.txt"));

			client.Dispose();

			foreach (var rout in Routs)
			{
				rout.Time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
				if (rout.Time != null)
					rout.Time.Rout = rout;

				rout.Stops = new List<Stop>();
				foreach (var st in rout.RouteStops)
				{
					rout.Stops.Add(Stops.First(x => x.ID == st));
				}
			}

			RoutView = new CollectionViewSource();
			RoutView.GroupDescriptions.Add(new PropertyGroupDescription("RoutNum"));
			RoutView.Source = Routs;
		}

		public ObservableCollection<Time> Time
		{
			get { return timeT; }
			set
			{
				timeT = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<Rout> Rout
		{
			get { return routT; }
			set
			{
				routT = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<Stop> Stop
		{
			get { return stopT; }
			set
			{
				stopT = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<Schedule> Times
		{
			get { return times; }
			set
			{
				times = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Stop> Stops
		{
			get { return stops; }
			set
			{
				if (Equals(value, stops)) return;
				stops = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Rout> Routs
		{
			get { return routs; }
			set
			{
				if (Equals(value, routs))
					return;
				routs = value;
				OnPropertyChanged();
			}
		}

		public bool ParsStops(string stops)
		{
			Stops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(stops));
			return true;
		}

		public ActionCommand ShowRoutCommand
		{
			get
			{
				return  new ActionCommand(
					x =>
					{
						RoutViewSource.Source = new ObservableCollection<Rout>(Routs.Where(r => r.RouteNum.Contains(RoutNum)));
						
					});
			}
		}

		public ActionCommand ParseStopsCommand
		{
			get { return new ActionCommand(x => Stops = new ObservableCollection<Stop>(ShedulerParser.ParsStops((string) x))); }
		}

		public ActionCommand ParseRoutsCommand
		{
			get { return new ActionCommand(x => Routs = new ObservableCollection<Rout>(ShedulerParser.ParsRout((string)x))); }
		}

		public ActionCommand ParseTimesCommand
		{
			get
			{
				return new ActionCommand(x => Times = new ObservableCollection<Schedule>(ShedulerParser.ParsTime((string)x)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
