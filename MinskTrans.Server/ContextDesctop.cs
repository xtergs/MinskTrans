using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.Library;

namespace MinskTrans.DesctopClient
{
	public class ContextDesctop : Context
	{
		#region Overrides of Context

		public override void Create()
		{
			if (!File.Exists(list[0].Key) || !File.Exists(list[1].Key) || !File.Exists(list[2].Key))
				DownloadUpdate();

			Stops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(File.ReadAllText(list[0].Key)));
			Routs = new ObservableCollection<Rout>(ShedulerParser.ParsRout(File.ReadAllText(list[1].Key)));
			Times = new ObservableCollection<Schedule>(ShedulerParser.ParsTime(File.ReadAllText(list[2].Key)));

			foreach (Rout rout in Routs)
			{
				rout.Time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
				if (rout.Time != null)
					rout.Time.Rout = rout;

				rout.Stops = new List<Stop>();
				foreach (int st in rout.RouteStops)
				{
					rout.Stops.Add(Stops.First(x => x.ID == st));
				}
			}
		}

		public override void DownloadUpdate()
		{
			using (var client = new WebClient())
			{
				client.DownloadFile(list[0].Value, list[0].Key + ".new");
				client.DownloadFile(list[1].Value, list[1].Key + ".new");
				client.DownloadFile(list[2].Value, list[2].Key + ".new");
			}

			foreach (var keyValuePair in list)
			{
				if (File.Exists(keyValuePair.Key))
					File.Delete(keyValuePair.Key);
				File.Move(keyValuePair.Key + ".new", keyValuePair.Key);
			}
		}

		public async override void UpdateAsync()
		{
			await Task.Run(() =>
			{
				DownloadUpdate();
				Stops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(File.ReadAllText(list[0].Key)));
				Routs = new ObservableCollection<Rout>(ShedulerParser.ParsRout(File.ReadAllText(list[1].Key)));
				Times = new ObservableCollection<Schedule>(ShedulerParser.ParsTime(File.ReadAllText(list[2].Key)));

				foreach (Rout rout in Routs)
				{
					rout.Time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
					if (rout.Time != null)
						rout.Time.Rout = rout;

					rout.Stops = new List<Stop>();
					foreach (int st in rout.RouteStops)
					{
						rout.Stops.Add(Stops.First(x => x.ID == st));
					}
				}

				LastUpdateDataDateTime = DateTime.Now;
			});
		}

		#endregion
	}
}
