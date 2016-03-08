using System.Collections.Generic;
using System.Linq;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MetroLog;
using MinskTrans.AutoRouting.AutoRouting;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;

namespace MinskTrans.Context
{
	public class Location
	{
	    public Location(double lat, double longit)
	    {
	        Latitude = lat;
	        Longitude = longit;
	    }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}

	public class ErrorMessage
	{
		public DateTime TimeEvent { get; set; }
		public string Message { get; set; }
	}

	public class BussnessLogic : GenericBussnessLogic
	{
	    //private const string urlUpdateDates = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111529&authkey=%21ADs9KNHO9TDPE3Q&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D4";
	    //private const string urlUpdateNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111532&authkey=%21AAQED1sY1RWFib8&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D8";
	    //private const string urlUpdateHotNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111531&authkey=%21AIJo-8Q4661GpiI&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D2";


	    private readonly UpdateManagerBase updateManager;
	    private readonly InternetHelperBase internetHelper;
	    private readonly FileHelperBase fileHelper;
	    private readonly NewsManagerBase newManager;
	    private readonly ISettingsModelView settings;
	    private readonly FilePathsSettings files;
	    public ILogger log { get; set; }

	    public BussnessLogic(IContext cont, UpdateManagerBase updateManager, InternetHelperBase internetHelper, FileHelperBase fileHelper, NewsManagerBase newManager, ISettingsModelView settings, IGeolocation geolocation, FilePathsSettings files, ILogManager manager = null)
            :base(cont)
	    {
	        
	        this.updateManager = updateManager;
	        this.internetHelper = internetHelper;
	        this.fileHelper = fileHelper;
	        this.newManager = newManager;
	        this.settings = settings;
	        this.files = files;
	        log = manager?.GetLogger<BussnessLogic>();
	        this.Geolocation = geolocation;
	    }

       

	    Queue<ErrorMessage> MessageToShow { get; set; }

	    public override ISettingsModelView Settings => settings;

	    private int countUpdateFail = 0;
/*
		private int maxCountUpdateFail = 10;
*/

	   


		private IEnumerable<Stop> SmartSort(IEnumerable<Stop> stops, Location location)
		{
		    IEnumerable<Stop> enumerable = stops as IList<Stop> ?? stops.ToList();
		    var byDeistance = enumerable.OrderBy(x=> Distance(x, location)).ToList();
			var result = enumerable.OrderByDescending(x => Context.GetCounter(x))
				.Select((x, i) => new { x, byCounter = i, byDistance = byDeistance.IndexOf(x) })
				.OrderBy(x => x.byCounter + x.byDistance)
				.Select(x => x.x)
				.ToList();
			return result;
		}
        
		EquirectangularDistance distance = new EquirectangularDistance();
		private double Distance(Stop x, Location location)
		{
			return distance.CalculateDistance(location.Latitude, location.Longitude, x.Lat, x.Lng);
			//return Math.Abs(Math.Sqrt(Math.Pow( - x.Lng, 2) + Math.Pow(LocationXX.Get().Latitude - x.Lat, 2)));
		}

		

		private bool updatingNewsTable = false;

	    public override async Task<bool> UpdateNewsTableAsync(CancellationToken token)
	    {
	        if (updatingNewsTable || token.IsCancellationRequested)
	            return false;
            OnUpdateDbStarted();
	        try
	        {
	            tokens.Add(token, true);
	            updatingNewsTable = true;
	            try
	            {
	                await fileHelper.DeleteFile(files.LastUpdatedFile.Folder, files.LastUpdatedFile.NewFileName);
	                await
	                    internetHelper.Download(files.LastUpdatedFile.SecondFormatedLink, files.LastUpdatedFile.NewFileName,
	                        files.LastUpdatedFile.Folder);
	            }
	            catch (Exception e)
	            {
	                log?.Error("UpdateNewsTAbleAsync");
	                log?.Error(e.Message, e);
	                return false;
	            }

	            if (token.IsCancellationRequested)
	                return false;

	            string resultStr =
	                await fileHelper.ReadAllTextAsync(files.LastUpdatedFile.Folder, files.LastUpdatedFile.NewFileName);

	            var timeShtaps = resultStr.Split('\n');
	            var utcNow = DateTime.Parse(timeShtaps[0], CultureInfo.InvariantCulture);
	            //NewsManager manager = new NewsManager();
	            //await newManager.Load();
	            DateTime oldMonthTime = settings.LastNewsTimeUtc;
	            DateTime oldDaylyTime = settings.LastUpdateHotNewsDateTimeUtc;

	            if (utcNow > oldMonthTime)
	            {
	                try
	                {
	                    await fileHelper.DeleteFile(files.MainNewsFile.Folder, files.MainNewsFile.NewFileName);
	                    await
	                        internetHelper.Download(files.MainNewsFile.SecondFormatedLink, files.MainNewsFile.NewFileName,
	                            files.MainNewsFile.Folder);
	                    await
	                        fileHelper.SafeMoveAsync(files.MainNewsFile.Folder,
	                            from: files.MainNewsFile.NewFileName,
	                            to: files.MainNewsFile.FileName);
	                    settings.LastNewsTimeUtc = utcNow;
	                    //TODO settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
	                }
	                catch (Exception e)
	                {
	                    string message =
	                        new StringBuilder("News manager download months exception").AppendLine(e.ToString())
	                            .AppendLine(e.Message)
	                            .AppendLine(e.StackTrace)
	                            .ToString();
	                    log?.Error(message);
	                    Debug.WriteLine(message);
	                }
	            }

	            if (token.IsCancellationRequested)
	                return false;

	            utcNow = DateTime.Parse(timeShtaps[1], CultureInfo.InvariantCulture);
	            if (utcNow > oldDaylyTime)
	            {
	                try
	                {
	                    await fileHelper.DeleteFile(files.HotNewsFile.Folder, files.HotNewsFile.NewFileName);
	                    await
	                        internetHelper.Download(files.HotNewsFile.SecondFormatedLink, files.HotNewsFile.NewFileName,
	                            files.HotNewsFile.Folder);
	                    await
	                        fileHelper.SafeMoveAsync(files.HotNewsFile.Folder,
	                            from: files.HotNewsFile.NewFileName,
	                            to: files.HotNewsFile.FileName);
	                    settings.LastUpdateHotNewsDateTimeUtc = utcNow;
	                    //TODO settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
	                }
	                catch (Exception e)
	                {
	                    string message =
	                        new StringBuilder("News manager download days exception").AppendLine(e.ToString())
	                            .AppendLine(e.Message)
	                            .AppendLine(e.StackTrace)
	                            .ToString();
	                    Debug.WriteLine(message);
	                    log?.Error(message, e);
	                }
	            }

	            if (token.IsCancellationRequested)
	                return false;

	            DateTime nowTimeUtc = DateTime.UtcNow;
	            //TODO
	            //var listOfDaylyNews = newManager.NewNews.Where(
	            //    key => key.PostedUtc > oldMonthTime && ((nowTimeUtc - key.PostedUtc).TotalDays < MaxDaysAgo));

	            //TODO
	            //var listOfMonthNews = newManager.AllHotNews.Where(key =>
	            //{
	            //    if (key.RepairedLineUtc != default(DateTime))
	            //    {
	            //        double totalminutes = (nowTimeUtc.ToLocalTime() - key.RepairedLineLocal).TotalMinutes;
	            //        if (totalminutes <= MaxMinsAgo)
	            //            return true;
	            //        return false;
	            //    }
	            //    return (key.CollectedUtc > oldDaylyTime) &&
	            //           ((nowTimeUtc - key.CollectedUtc).TotalDays < 1);
	            //});
	            await newManager.Load();
	            log?.Info("UpdateNewsTableAsync is OK, return true");
	            return true;
	        }
	        catch (Exception e)
	        {
	            log?.Fatal("UpdateNewsTableAsync", e);
	            throw;
	        }
	        finally
	        {
	            tokens.Remove(token);
                OnUpdateDbEnded();
	            updatingNewsTable = false;
	        }
	    }


	    private bool updatingTimeTable = false;

	    public override async Task<bool> UpdateTimeTableAsync(CancellationToken token, bool withLightCheck = false, bool tryOnlyOriginalLink = false)
	    {
	        if (updatingTimeTable || token.IsCancellationRequested)
	        {
                log?.Debug("UpdateTimeTableAsync: already updating or cancel requested, return false");
	            return false;
	        }
	        OnUpdateDbStarted();
	        DateTime utcNow = DateTime.UtcNow;
	        try
	        {
	            tokens.Add(token, true);
	            updatingTimeTable = true;
	            if (withLightCheck)
	            {
	                string suf = "ForDb";
	                string downloadTo = files.LastUpdatedFile.NewFileName + suf;
	                string moveTo = files.LastUpdatedFile.FileName + suf;
	                TypeFolder folder = files.LastUpdatedFile.Folder;

	                try
	                {
	                    await fileHelper.DeleteFile(folder, downloadTo);
	                    await internetHelper.Download(files.LastUpdatedFile.SecondFormatedLink, downloadTo, folder);
	                    await
	                        fileHelper.SafeMoveAsync(folder,
	                            from: downloadTo,
	                            to: moveTo);
	                }
	                catch (Exception e)
	                {
	                    log?.Error("UpdateTimeTableAsync: trying to download lastupdate file", e);
	                    countUpdateFail++;
	                    log?.Info($"UpdateTimeTableAsync: CountUpdateFail: {countUpdateFail}");
	                    return false;
	                }
	                if (token.IsCancellationRequested)
	                    return false;
	                string resultStr =
	                    await fileHelper.ReadAllTextAsync(folder, moveTo);

	                var timeShtaps = resultStr.Split('\n');

	                if (timeShtaps.Length > 2)
	                    utcNow = DateTime.Parse(timeShtaps[2], CultureInfo.InvariantCulture);
	                if (utcNow <= Settings.LastUpdateDbDateTimeUtc)
	                {
	                    log?.Info("Light check is OK, no need to update");
	                    log?.Info($"last update: {Settings.LastUpdateDbDateTimeUtc}");
	                    return false;
	                }
	                log?.Info("Light check is OK, need update");
	            }
	            if (tryOnlyOriginalLink || !await updateManager.DownloadFormatedUpdateAsync(token))
	            {
	                if (token.IsCancellationRequested)
	                    return false;
	                if (!await updateManager.DownloadOriginalUpdateAsync(token))
	                    return false;
	            }
	            var timeTable = await updateManager.GetTimeTable(token);

	            if (token.IsCancellationRequested)
	                return false;

	            if (await Context.HaveUpdate(timeTable.Routs as IList<Rout>, timeTable.Stops as IList<Stop>, timeTable.Time as IList<Schedule>))
	            {
	                await Context.ApplyUpdate(timeTable.Routs as IEnumerable<Rout>, timeTable.Stops as IList<Stop>, timeTable.Time as IList<Schedule>);
	                Context.AllPropertiesChanged();
	                await Context.Save(true);
	            }
	            Settings.LastUpdateDbDateTimeUtc = utcNow;
               
	            return true;
	        }
			catch(Exception e)
			{
                log?.Error("UpdateTimeTableAsync", e);
                countUpdateFail++;
                log?.Info($"UpdateTimeTableAsync: CountUpdateFail: {countUpdateFail}");
                throw;
			}
	        finally
	        {
	            tokens.Remove(token);
                OnUpdateDbEnded();
	            updatingTimeTable = false;
	        }
	    }

		

	    private void StopGPS()
	    {
	        throw new NotImplementedException();
	    }

	    private void StartGPS()
	    {
	        throw new NotImplementedException();
	    }


	    public Task ClearFolder(TypeFolder folder)
	    {
	        return fileHelper.ClearFolder(folder);
	    }
	
        
	}

   
}
