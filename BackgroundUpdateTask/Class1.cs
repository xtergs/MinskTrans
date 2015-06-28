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
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace BackgroundUpdateTask
{
    public sealed class UpdateBackgroundTask : IBackgroundTask
    {
		private string urlUpdateDates = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111529&authkey=%21ADs9KNHO9TDPE3Q&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D4";
		//private string urlUpdate = @"https://onedrive.live.com/redir?resid=27EDF63E3C801B19!11529&authkey=!ADs9KNHO9TDPE3Q&ithint=file%2ctxt";
		private string urlUpdateNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111532&authkey=%21AAQED1sY1RWFib8&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D8";
		private string urlUpdateHotNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111531&authkey=%21AIJo-8Q4661GpiI&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D2";

		static string SettingsToStr([CallerMemberName] string propertyName = null)
		{
			return propertyName;
		}

	    private DateTime LastUpdateDBDatetime
	    {

		    get
		    {
			    if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
				    LastUpdateDBDatetime = new DateTime();
			    return (DateTime) ApplicationData.Current.LocalSettings.Values[SettingsToStr()];
		    }

		    set
		    {
			    if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(SettingsToStr()))
				    ApplicationData.Current.LocalSettings.Values.Add(SettingsToStr(), value);
			    else
				    ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value.ToString();
		    }
	    }

		int MaxDaysAgo { get; set; }
		int MaxMinsAgo { get; set; }

	    //Dictionary<int, string> DaysLinks 

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			Debug.WriteLine("Background task started");
			BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
			SettingsModelView settings = new SettingsModelView();
			settings.LastUpdatedDataInBackground = SettingsModelView.TypeOfUpdate.None;
			InternetHelper.UpdateNetworkInformation();
			if (!settings.HaveConnection())
				_deferral.Complete();
	        MaxDaysAgo = 30;
	        MaxMinsAgo = 20;
			string resultStr = await InternetHelper.Download(urlUpdateDates);
	        var timeShtaps = resultStr.Split('\n');
	        DateTime time = new DateTime();
	        if (timeShtaps.Length > 0)
		        time = DateTime.Parse(timeShtaps[2]);
			UniversalContext context = new UniversalContext();
	        if (context.LastUpdateDataDateTime < time)
	        {
		        await context.UpdateAsync();
		        context.LastUpdateDataDateTime = time;
				settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.Db;
		        //await context.Save(false);
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
				if (time > manager.LastUpdateDataDateTime)
		        {
					await InternetHelper.Download(urlUpdateNews, manager.fileNameNews, ApplicationData.Current.LocalFolder);
			        manager.LastUpdateDataDateTime = time;
					settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
					await manager.Load(NewsManager.TypeLoad.LoadNews);
			        DateTime nowTime = DateTime.UtcNow;
					foreach (var source in manager.NewNews.Where(key => key.Posted > manager.LastUpdateDataDateTime && ((nowTime - key.Posted).TotalDays < MaxDaysAgo)))
			        {
				        ShowNotification(source.Message);
			        }
		        }
				time = DateTime.Parse(timeShtaps[1]);
		        if (time > manager.LastUpdateHotDataDateTime)
		        {
					await InternetHelper.Download(urlUpdateHotNews, manager.fileNameHotNews, ApplicationData.Current.LocalFolder);
			        manager.LastUpdateHotDataDateTime = time;
					//await FileIO.WriteTextAsync(await ApplicationData.Current.LocalFolder.CreateFileAsync(manager.fileNameHotNews, CreationCollisionOption.ReplaceExisting),
					//	resultStr);
			        await manager.Load(NewsManager.TypeLoad.LoadHotNews);
			        DateTime nowDateTime = DateTime.UtcNow;
					settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
					//int todayDay = nowDateTime.Day;
					//int prevday = nowDateTime.Subtract(new TimeSpan(1, 0, 0, 0)).Day;
			        foreach (var source in manager.AllHotNews.Where(key =>
			        {
						if (key.RepairedLIne != default(DateTime))
						{
							double totalminutes = (nowDateTime.ToLocalTime() - key.RepairedLIneLocal).TotalMinutes;
							if ( totalminutes <= MaxMinsAgo)
								return true;
						}
				        return (key.Collected > manager.LastUpdateHotDataDateTime) &&
				               ((nowDateTime- key.Collected).TotalMinutes < MaxMinsAgo);
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


