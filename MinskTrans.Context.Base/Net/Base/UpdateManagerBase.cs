using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MetroLog;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;

namespace MinskTrans.Net.Base
{
	public struct TimeTable
	{
		public IList<Rout> Routs;
		public IList<Stop> Stops;
		public IList<Schedule> Time;
	}
	public class UpdateManagerBase
	{
		protected readonly List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>
		{
			new KeyValuePair<string, string>("stops.txt", @"http://www.minsktrans.by/city/minsk/stops.txt"),
			new KeyValuePair<string, string>("routes.txt", @"http://www.minsktrans.by/city/minsk/routes.txt"),
			new KeyValuePair<string, string>("times.txt", @"http://www.minsktrans.by/city/minsk/times.txt")
		};

		public TypeFolder Folder { get; set; } //= TypeFolder.Temp;

		public async Task<bool> DownloadUpdate()
		{
			OnDataBaseDownloadStarted();
            Logger.Info("UpdadteManagerBase.DownloadUpdadte started");
			var folder = Folder;
			try
			{

				await Task.WhenAll(
					internetHelper.Download(list[0].Value, list[0].Key + FileHelperBase.NewExt, folder),
					internetHelper.Download(list[1].Value, list[1].Key + FileHelperBase.NewExt, folder),
					internetHelper.Download(list[2].Value, list[2].Key + FileHelperBase.NewExt, folder));
				OnDataBaseDownloadEnded();

			}
			catch (HttpRequestException)
			{
				OnErrorDownloading();
				/*await*/
				fileHelper.DeleteFiels(folder, list.Select(x => x.Key + FileHelperBase.NewExt));
				return false;
			}
			catch (System.Net.WebException e)
			{
				OnErrorDownloading();
				/*await*/
				fileHelper.DeleteFiels(folder, list.Select(x => x.Key + FileHelperBase.NewExt));
				return false;
			}
			catch (Exception e)
			{
				OnErrorDownloading();
                Logger.Info("UpdadteManagerBase.DownloadUpdadte error");
                Logger.Fatal("UpdadteManagerBase.DownloadUpdadte", e);
                /*await*/
                fileHelper.DeleteFiels(folder, list.Select(x => x.Key + FileHelperBase.NewExt));
				throw;
			}
			await fileHelper.SafeMoveFilesAsync(folder, list.Select(x => x.Key + FileHelperBase.NewExt), list.Select(x => x.Key));
            //await Task.WhenAll(
            //fileHelper.SafeMoveAsync(folder, list[0].Key + FileHelperBase.NewExt, list[0].Key),
            //fileHelper.SafeMoveAsync(folder, list[1].Key + FileHelperBase.NewExt, list[1].Key),
            //fileHelper.SafeMoveAsync(folder, list[2].Key + FileHelperBase.NewExt, list[2].Key));
            Logger.Info("UpdadteManagerBase.DownloadUpdadte ended");
            return true;
		}

		protected IList<Rout> Routs { get; set; }
		protected IList<Stop> Stops { get; set; }
		protected IList<Schedule> Time { get; set; }

		protected readonly FileHelperBase fileHelper;
		protected readonly InternetHelperBase internetHelper;
		protected readonly ITimeTableParser timeTableParser;
	    protected readonly ILogger Logger;
		public UpdateManagerBase(FileHelperBase helper, InternetHelperBase internet, ITimeTableParser parser, ILogManager logger)
		{
			if (helper == null)
				throw new ArgumentNullException(nameof(helper));
			if (internet == null)
				throw new ArgumentNullException(nameof(internet));
			if (parser == null)
				throw new ArgumentNullException(nameof(parser));
		    if (logger == null)
		        throw new ArgumentNullException(nameof(logger));
			fileHelper = helper;
			internetHelper = internet;
			Folder = TypeFolder.Temp;
			timeTableParser = parser;
		    this.Logger = logger.GetLogger<UpdateManagerBase>();
		}

		public async Task<TimeTable> GetTimeTable()
		{
            Logger.Info("UpdadteManagerBase.GetTimeTable started");
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
						newStops = new List<Stop>(timeTableParser.ParsStops(await fileHelper.ReadAllTextAsync(Folder, list[0].Key)));
					}
					catch(Exception e)
					{
                        Logger.Fatal("UpdadteManagerBase: ParseStops", e);
                        throw;
					}
				}),
					Task.Run(async () =>
					{
						//StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileRouts);
						try {
							newRoutes = new List<Rout>(timeTableParser.ParsRout(await fileHelper.ReadAllTextAsync(Folder, list[1].Key)));
						}
						catch(Exception e)
						{
                            Logger.Fatal("UpdadteManagerBase: ParseStops",e);
                            throw;
						}

					}),
					Task.Run(async () =>
					{
						//StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileTimes);
						try {
							newSchedule = new List<Schedule>(timeTableParser.ParsTime(await fileHelper.ReadAllTextAsync(Folder, list[2].Key)));
						}
						catch(Exception e)
						{
                            Logger.Fatal("UpdadteManagerBase: ParseStops", e);
                            throw;
						}

					}));
                Logger.Info("UpdadteManagerBase: All threads ended");
                //Debug.WriteLine("All threads ended");
                //OnLogMessage("All threads ended");
            }
			catch (FileNotFoundException e)
			{
                Logger.Fatal("UpdadteManagerBase.GetTimeTable", e);
                throw;
			}
			catch (Exception e)
			{
                Logger.Fatal("UpdadteManagerBase.GetTimeTable started", e);
                throw;
			}

            Logger.Info("UpdadteManagerBase.GetTimeTable ended");

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
			Logger.Debug("OnDBDownloadStarted");
			var handler = DataBaseDownloadStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnDataBaseDownloadEnded()
		{
			Logger.Debug("OnDBDownloadEnded");
			var handler = DataBaseDownloadEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnErrorDownloading()
		{
			Logger.Debug("OnErrorDonloading");
			var handler = ErrorDownloading;
			if (handler != null) handler(this, EventArgs.Empty);
		}
	}
}
