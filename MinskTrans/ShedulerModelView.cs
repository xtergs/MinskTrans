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
