using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.Annotations;

namespace MinskTrans
{
	public class Context :INotifyPropertyChanged
	{
		private ObservableCollection<Stop> stops;
		private ObservableCollection<Rout> routs;
		private ObservableCollection<Schedule> times;

		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>()
		{
			new KeyValuePair<string, string>("stops.txt",@"http://www.minsktrans.by/city/minsk/stops.txt"),
			new KeyValuePair<string, string>( "routes.txt", @"http://www.minsktrans.by/city/minsk/routes.txt"),
			new KeyValuePair<string, string>("times.txt", @"http://www.minsktrans.by/city/minsk/times.txt")
		}; 


		public Context()
		{
			Create();
		}

		public void Create()
		{
			WebClient client = new WebClient();

			client.DownloadFile(list[0].Value, list[0].Key);
			Stops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(File.ReadAllText(list[0].Key)));

			client.DownloadFile(list[1].Value,list[1].Key);
			Routs = new ObservableCollection<Rout>(ShedulerParser.ParsRout(File.ReadAllText(list[1].Key)));

			client.DownloadFile(list[2].Value,list[2].Key );
			Times = new ObservableCollection<Schedule>(ShedulerParser.ParsTime(File.ReadAllText(list[2].Key)));
			

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
		
		

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
