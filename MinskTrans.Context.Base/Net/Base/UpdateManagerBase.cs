using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MetroLog;
using MinskTrans.Context;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using Newtonsoft.Json;

namespace MinskTrans.Net.Base
{
	public struct TimeTable
	{
		public IList<Rout> Routs;
		public IList<Stop> Stops;
		public IList<Schedule> Time;
	}

    struct Pair
    {
        public Pair(string fileName, TypeFolder folder, string link)
        {
            FileName = fileName;
            Folder = folder;
            Link = link;
        }
        public string FileName { get;  }
        public TypeFolder Folder { get; }
        public string Link { get; }
    }

    
	public class UpdateManagerBase
	{
	    private FilePathsSettings filesPath;
        
		//protected readonly List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>
		//{
		//	new KeyValuePair<string, string>("stops.txt", @"http://www.minsktrans.by/city/minsk/stops.txt"),
		//	new KeyValuePair<string, string>("routes.txt", @"http://www.minsktrans.by/city/minsk/routes.txt"),
		//	new KeyValuePair<string, string>("times.txt", @"http://www.minsktrans.by/city/minsk/times.txt")
		//};

		//public TypeFolder Folder { get; set; } //= TypeFolder.Temp;

	    Task ClearDownloadedFiles(List<Pair> list)
	    {
            List<Task> tasks = new List<Task>(list.Count);
	        tasks.AddRange(list.Select(line => fileHelper.DeleteFile(line.Folder, line.FileName)));

	        return Task.WhenAll(tasks);
	    }

	    public Task ApproveDonloadedFilesAsync()
	    {
	        return Task.WhenAll(
	            fileHelper.SafeMoveAsync(folder: filesPath.StopsFile.Folder,
	                from: filesPath.StopsFile.NewFileName,
	                to: filesPath.StopsFile.FileName),
                fileHelper.SafeMoveAsync(folder: filesPath.RouteFile.Folder,
                    from: filesPath.RouteFile.NewFileName,
                    to: filesPath.RouteFile.FileName),
                fileHelper.SafeMoveAsync(folder: filesPath.TimeFile.Folder,
                    from: filesPath.TimeFile.NewFileName,
                    to: filesPath.TimeFile.FileName)
                );
	    }

	    public Task<bool> DownloadFormatedUpdateAsync(CancellationToken cancelToken)
	    {
	        try
	        {
	            Logger?.Debug($"{nameof(DownloadFormatedUpdateAsync)} is started");
                List<Pair> list = new List<Pair>()
                {
                    new Pair(filesPath.StopsFile.NewFileName, filesPath.StopsFile.Folder, filesPath.StopsFile.SecondFormatedLink),
                    new Pair(filesPath.RouteFile.NewFileName, filesPath.RouteFile.Folder, filesPath.RouteFile.SecondFormatedLink),
                    new Pair(filesPath.TimeFile.NewFileName, filesPath.TimeFile.Folder, filesPath.TimeFile.SecondFormatedLink)
                };
	            return DownloadUpdateAsync(list, cancelToken);

	        }
	        finally
	        {
	        Logger?.Debug($"{nameof(DownloadFormatedUpdateAsync)} is ended");
	            
	        }
        }

        public Task<bool> DownloadOriginalUpdateAsync(CancellationToken cancelToken)
        {
            try
            {
                Logger?.Debug($"{nameof(DownloadOriginalUpdateAsync)} is started");
                List<Pair> list = new List<Pair>()
                {
                    new Pair(filesPath.StopsFile.NewFileName, filesPath.StopsFile.Folder, filesPath.StopsFile.OriginalLink),
                    new Pair(filesPath.RouteFile.NewFileName, filesPath.RouteFile.Folder, filesPath.RouteFile.OriginalLink),
                    new Pair(filesPath.TimeFile.NewFileName, filesPath.TimeFile.Folder, filesPath.TimeFile.OriginalLink)
                };
                return DownloadUpdateAsync(list, cancelToken);

            }
            finally
            {
                Logger?.Debug($"{nameof(DownloadOriginalUpdateAsync)} is ended");

            }
        }

        async Task<bool> DownloadUpdateAsync(List<Pair> list,  CancellationToken cancelToken)
		{
			OnDataBaseDownloadStarted();
            Logger.Info("UpdadteManagerBase.DownloadUpdadte started");
			//var folder = Folder;
		    if (cancelToken.IsCancellationRequested)
		        return false;
			try
			{
                await ClearDownloadedFiles(list);
			    await Task.WhenAll(list.Select(x => internetHelper.Download(x.Link, x.FileName, x.Folder)));
				OnDataBaseDownloadEnded();

			}
			catch (HttpRequestException)
			{
				OnErrorDownloading();
                /*await*/
			    ClearDownloadedFiles(list);

                return false;
			}
			catch (System.Net.WebException)
			{
				OnErrorDownloading();
                /*await*/
                ClearDownloadedFiles(list);
                return false;
			}
			catch (Exception e)
			{
				OnErrorDownloading();
                Logger.Info("UpdadteManagerBase.DownloadUpdadte error");
                Logger.Fatal("UpdadteManagerBase.DownloadUpdadte", e);
                /*await*/
                ClearDownloadedFiles(list);
                throw;
			}
		    //await
		    //    fileHelper.SafeMoveAsync(folder: filesPath.StopsFile.Folder, 
      //                                  from: filesPath.StopsFile.NewFileName,
      //                                  to: filesPath.StopsFile.FileName);

		    if (cancelToken.IsCancellationRequested)
		    {
		        ClearDownloadedFiles(list);
		        return false;
		    }
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
		public UpdateManagerBase(FileHelperBase helper, InternetHelperBase internet, ITimeTableParser parser, ILogManager logger, FilePathsSettings filesPath)
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
			//Folder = TypeFolder.Temp;
			timeTableParser = parser;
		    this.filesPath = filesPath;
		    this.Logger = logger.GetLogger<UpdateManagerBase>();
		}

	    private async Task<List<string>> ReadFilesAsync()
	    {
	        string[] resList = new string[3];
	        await Task.WhenAll(Task.Run(async () =>
	        {
	            resList[0] = await fileHelper.ReadAllTextAsync(filesPath.StopsFile.Folder, filesPath.StopsFile.NewFileName);
	        }), Task.Run(async () =>
	        {
	            resList[1] = await fileHelper.ReadAllTextAsync(filesPath.RouteFile.Folder, filesPath.RouteFile.NewFileName);
	        }), Task.Run(async () =>
	        {
	            resList[2] = await fileHelper.ReadAllTextAsync(filesPath.TimeFile.Folder, filesPath.TimeFile.NewFileName);

	        })
	            );
	        return resList.ToList();
	    }

	    public async Task<TimeTable> GetTimeTableAsync()
	    {

            var list = await ReadFilesAsync();
	        TimeTable table;
	        try
	        {
	            table = GetTimeTableFromFormatedFile(list);
	        }
	        catch (Exception)
	        {
	            table = await GetTimeTable(list);
	        }
	        return table;
	    }

	    TimeTable GetTimeTableFromFormatedFile(List<string> list)
	    {
	        IList<Stop> newStops = JsonConvert.DeserializeObject<IList<Stop>>(list[0]);
            IList<Rout> newRoutes = JsonConvert.DeserializeObject<IList<Rout>>(list[1]);
            IList<Schedule> newSchedule = JsonConvert.DeserializeObject<IList<Schedule>>(list[2]);

            return new TimeTable()
            {
                Routs = newRoutes,
                Stops = newStops,
                Time = newSchedule
            };
        }

        async Task<TimeTable> GetTimeTableFromFormatedFile()
        {
            var list = await ReadFilesAsync();
            return GetTimeTableFromFormatedFile(list);
        }

        public async Task<TimeTable> GetTimeTable(List<string> list )
        {
            Logger.Info("UpdadteManagerBase.GetTimeTable started");
            IList<Stop> newStops = null;
            IList<Rout> newRoutes = null;
            IList<Schedule> newSchedule = null;
            try
            {
                //#if DEBUG


                await Task.WhenAll(Task.Run(() =>
                {
                    //StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileStops);
                    try
                    {
                        newStops =
                            timeTableParser.ParsStops(list[0])
                                .ToList();
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal("UpdadteManagerBase: ParseStops", e);
                        throw;
                    }
                }),
                    Task.Run(() =>
                    {
                        //StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileRouts);
                        try
                        {
                            newRoutes =
                                timeTableParser.ParsRout(list[1])
                                    .ToList();
                        }
                        catch (Exception e)
                        {
                            Logger.Fatal("UpdadteManagerBase: ParseStops", e);
                            throw;
                        }

                    }),
                    Task.Run(() =>
                    {
                        //StorageFile file = await ApplicationData.Current.RoamingFolder.GetFileAsync(fileTimes);
                        try
                        {
                            newSchedule =
                                timeTableParser.ParsTime(list[2]).ToList();
                        }
                        catch (Exception e)
                        {
                            Logger.Fatal("UpdadteManagerBase: ParseStops", e);
                            throw;
                        }

                    }));
                Logger?.Info("UpdadteManagerBase: All threads ended");
                //Debug.WriteLine("All threads ended");
                //OnLogMessage("All threads ended");
            }
            catch (FileNotFoundException e)
            {
                Logger?.Fatal("UpdadteManagerBase.GetTimeTable", e);
                throw;
            }
            catch (Exception e)
            {
                Logger?.Fatal("UpdadteManagerBase.GetTimeTable Error", e);
                throw;
            }

            Logger?.Info("UpdadteManagerBase.GetTimeTable ended");

            return new TimeTable()
            {
                Routs = newRoutes,
                Stops = newStops,
                Time = newSchedule
            };
        }

        public async Task<TimeTable> GetTimeTable(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return new TimeTable();
            var list =await ReadFilesAsync();
            if (token.IsCancellationRequested)
                return new TimeTable();
            TimeTable table;
            try
            {
                table = GetTimeTableFromFormatedFile(list);
            }
            catch (Exception)
            {
                table = await GetTimeTable(list);
            }
            return table;

        }

		public event EventHandler DataBaseDownloadStarted;
		public event EventHandler DataBaseDownloadEnded;
		public event EventHandler ErrorDownloading;

		protected virtual void OnDataBaseDownloadStarted()
		{
			Logger.Debug("OnDBDownloadStarted");
			var handler = DataBaseDownloadStarted;
		    handler?.Invoke(this, EventArgs.Empty);
		}
		protected virtual void OnDataBaseDownloadEnded()
		{
			Logger.Debug("OnDBDownloadEnded");
			var handler = DataBaseDownloadEnded;
		    handler?.Invoke(this, EventArgs.Empty);
		}
		protected virtual void OnErrorDownloading()
		{
			Logger.Debug("OnErrorDonloading");
			var handler = ErrorDownloading;
		    handler?.Invoke(this, EventArgs.Empty);
		}
	}
}
