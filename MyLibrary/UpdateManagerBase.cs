﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Model;
using MyLibrary;

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

		public TypeFolder Folder { get; set; } //= TypeFolder.Temp;

		public abstract Task<bool> DownloadUpdate();

		protected IList<Rout> Routs { get; set; }
		protected IList<Stop> Stops { get; set; }
		protected IList<Schedule> Time { get; set; }

		protected readonly FileHelperBase fileHelper;
		protected readonly InternetHelperBase internetHelper;
		public UpdateManagerBase(FileHelperBase helper, InternetHelperBase internet)
		{
			fileHelper = helper;
			internetHelper = internet;
			Folder = TypeFolder.Temp;
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
					try {
						newStops = new List<Stop>(ShedulerParser.ParsStops(await fileHelper.ReadAllTextAsync(Folder, list[0].Key)));
					}
					catch(Exception)
					{
						throw;
					}
				}),
					Task.Run(async () =>
					{
						//StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileRouts);
						try {
							newRoutes = new List<Rout>(ShedulerParser.ParsRout(await fileHelper.ReadAllTextAsync(Folder, list[1].Key)));
						}
						catch(Exception)
						{
							throw;
						}

					}),
					Task.Run(async () =>
					{
						//StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileTimes);
						try {
							newSchedule = new List<Schedule>(ShedulerParser.ParsTime(await fileHelper.ReadAllTextAsync(Folder, list[2].Key)));
						}
						catch(Exception)
						{
							throw;
						}

					}));
				Debug.WriteLine("All threads ended");
				//OnLogMessage("All threads ended");
			}
			catch (FileNotFoundException)
			{
				throw;
			}
			catch (Exception)
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
