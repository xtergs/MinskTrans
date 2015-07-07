using MinskTrans.DesctopClient.Model;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient.Update
{
	public struct TimeTable
	{
		public IList<Rout> Routs;
		public IList<Stop> Stops;
		public IList<Schedule> Time;
	}
	public abstract class UpdateManagerBase
	{
		protected readonly List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>
		{
			new KeyValuePair<string, string>("stops.txt", @"http://www.minsktrans.by/city/minsk/stops.txt"),
			new KeyValuePair<string, string>("routes.txt", @"http://www.minsktrans.by/city/minsk/routes.txt"),
			new KeyValuePair<string, string>("times.txt", @"http://www.minsktrans.by/city/minsk/times.txt")
		};

		public abstract Task<bool> DownloadUpdate();

		protected IList<Rout> Routs { get; set; }
		protected IList<Stop> Stops { get; set; }
		protected IList<Schedule> Time { get; set; }

		protected readonly FileHelperBase fileHelper;
		public UpdateManagerBase(FileHelperBase helper)
		{
			fileHelper = helper;
		}

		public async Task<TimeTable> GetTimeTable()
		{
			IList<Stop> newStops = null;
			IList<Rout> newRoutes = null;
			IList<Schedule> newSchedule = null;
			try
			{
				//#if DEBUG


				await Task.WhenAll(Task.Run(async () =>
				{
					//StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileStops);
					newStops = new List<Stop>(ShedulerParser.ParsStops(await fileHelper.ReadAllTextAsync(TypeFolder.Current, list[0].Key)));
				}),
					Task.Run(async () =>
					{
						//StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileRouts);
						newRoutes = new List<Rout>(ShedulerParser.ParsRout(await fileHelper.ReadAllTextAsync(TypeFolder.Current, list[1].Key)));

					}),
					Task.Run(async () =>
					{
						//StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileTimes);
						newSchedule = new List<Schedule>(ShedulerParser.ParsTime(await fileHelper.ReadAllTextAsync(TypeFolder.Current, list[2].Key)));

					}));
				Debug.WriteLine("All threads ended");
				//OnLogMessage("All threads ended");
			}
			catch (FileNotFoundException e)
			{
				throw;
			}
			catch (Exception e)
			{
				throw;
			}

			return new TimeTable()
			{
				Routs = newRoutes,
				Stops = newStops,
				Time = newSchedule
			};
		}

		public event EventHandler DataBaseDownloadStarted;
		public event EventHandler DataBaseDownloadEnded;
		public event EventHandler ErrorDownloading;

		protected virtual void OnDataBaseDownloadStarted()
		{
			var handler = DataBaseDownloadStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnDataBaseDownloadEnded()
		{
			var handler = DataBaseDownloadEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnErrorDownloading()
		{
			var handler = ErrorDownloading;
			if (handler != null) handler(this, EventArgs.Empty);
		}
	}
}
