using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;
using CommonLibrary;
using CommonLibrary.IO;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;
using MyLibrary;

namespace MinskTrans.BackgroundUpdateTask
{
    public sealed class UpdateBackgroundTask : IBackgroundTask
    {

		private string urlUpdateDates = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111529&authkey=%21ADs9KNHO9TDPE3Q&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D4";
		private string urlUpdateNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111532&authkey=%21AAQED1sY1RWFib8&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D8";
		private string urlUpdateHotNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111531&authkey=%21AIJo-8Q4661GpiI&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D2";
	    private string fileNews = "datesNews.dat";
		int MaxDaysAgo { get; set; }
		int MaxMinsAgo { get; set; }

	    //Dictionary<int, string> DaysLinks 

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
#if _DEBUG
	        urlUpdateDates =
		        @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111667&authkey=%21AKygfncKSi8je9M&canary=t3n9UqNfnwhSuGIRQt3HE7V3dRh0GsrkOOz1BGrzuZE%3D9";
	        urlUpdateNews =
		        @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111668&authkey=%21ANMtQUGlZfobwFk&canary=t3n9UqNfnwhSuGIRQt3HE7V3dRh0GsrkOOz1BGrzuZE%3D3";
	        urlUpdateHotNews =
		        @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111666&authkey=%21AEsBwSeL-AGmwg0&canary=t3n9UqNfnwhSuGIRQt3HE7V3dRh0GsrkOOz1BGrzuZE%3D9";
#endif
			Debug.WriteLine("Background task started");
			BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
			SettingsModelView settings = new SettingsModelView();
			InternetHelperBase helper = new InternetHelperBase(new FileHelper());
            settings.LastUpdatedDataInBackground = SettingsModelView.TypeOfUpdate.None;
			InternetHelperBase.UpdateNetworkInformation();
			if (!settings.HaveConnection())
				_deferral.Complete();
	        MaxDaysAgo = 30;
	        MaxMinsAgo = 20;

	        await helper.Download(urlUpdateDates, fileNews, TypeFolder.Temp);
			string resultStr = await FileIO.ReadTextAsync(await ApplicationData.Current.TemporaryFolder.GetFileAsync(fileNews));
	        var timeShtaps = resultStr.Split('\n');
	        DateTime time = new DateTime();
	        if (timeShtaps.Length > 2)
		        time = DateTime.Parse(timeShtaps[2]);
		        UniversalContext context = new UniversalContext(new FileHelper());
	        if (context.LastUpdateDataDateTime < time)
	        {
		        try
		        {
			        await context.UpdateAsync();
			        context.LastUpdateDataDateTime = time;
			        settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.Db;
			        //await context.Save(false);
		        }
		        catch (Exception e)
		        {
			        Logger.Log("BackgroundUpdate, updateDb Exception");
		        }
	        }

	        if (timeShtaps.Length < 1)
	        {
		        _deferral.Complete();
		        return;
	        }

	        try
	        {
		        time = DateTime.Parse(timeShtaps[0]);
		        NewsManager manager = new NewsManager();
				if (time > manager.LastUpdateMainNewsDateTime)
		        {
					await helper.Download(urlUpdateNews, manager.fileNameNews, TypeFolder.Local);
			        DateTime oldTime = manager.LastUpdateMainNewsDateTime;
			        manager.LastUpdateMainNewsDateTime = time;
					settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
			        DateTime nowTimeUtc = DateTime.UtcNow;
					foreach (var source in manager.NewNews.Where(key => key.PostedUtc > oldTime && ((nowTimeUtc - key.PostedUtc).TotalDays < MaxDaysAgo)))
			        {
				        ShowNotification(source.Message);
			        }
		        }
				time = DateTime.Parse(timeShtaps[1]);
		        if (time > manager.LastUpdateHotNewsDateTime)
		        {
					await helper.Download(urlUpdateHotNews, manager.fileNameHotNews, TypeFolder.Local);
			        DateTime oldTime = manager.LastUpdateHotNewsDateTime;
			        manager.LastUpdateHotNewsDateTime = time;
					//await FileIO.WriteTextAsync(await ApplicationData.Current.LocalFolder.CreateFileAsync(manager.fileNameHotNews, CreationCollisionOption.ReplaceExisting),
					//	resultStr);
			        DateTime nowDateTimeUtc = DateTime.UtcNow;
					settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
					//int todayDay = nowDateTime.Day;
					//int prevday = nowDateTime.Subtract(new TimeSpan(1, 0, 0, 0)).Day;
			        foreach (var source in manager.AllHotNews.Where(key =>
			        {
						if (key.RepairedLineUtc != default(DateTime))
						{
							double totalminutes = (nowDateTimeUtc.ToLocalTime() - key.RepairedLIneLocal).TotalMinutes;
							if ( totalminutes <= MaxMinsAgo)
								return true;
							return false;
						}
						return (key.CollectedUtc > oldTime) &&
				               ((nowDateTimeUtc- key.CollectedUtc).TotalDays < 1);
			        }))
			        {
				        ShowNotification(source.Message);
			        }
		        }
	        }
	        catch (Exception e)
	        {
		        string message =
			        new StringBuilder("Background task exception").AppendLine(e.ToString())
				        .AppendLine(e.Message)
				        .AppendLine(e.StackTrace)
				        .ToString();
		        Debug.WriteLine(message);

		        throw;
	        }

			Debug.WriteLine("Background task ended");
	        _deferral.Complete();
			
        }

	    void ShowNotification(string text)
	    {
			var notifi = ToastNotificationManager.CreateToastNotifier();

			var xaml = ToastNotificationManager.GetTemplateContent(Windows.UI.Notifications.ToastTemplateType.ToastText04);
			var textNode = xaml.GetElementsByTagName("text");
			textNode.Item(0).AppendChild(xaml.CreateTextNode(text));
			//value.appendChild(toastXml.createTextNode(text));
			ToastNotification notification = new ToastNotification(xaml);
			notifi.Show(notification);
	    }
    }
}


