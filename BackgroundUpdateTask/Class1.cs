using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;
using MinskTrans.Universal;

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
				    ApplicationData.Current.LocalSettings.Values[SettingsToStr()] = value;
		    }
	    }

	    //Dictionary<int, string> DaysLinks 

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();

			string resultStr = await InternetHelper.Download(urlUpdateDates);
	        var timeShtaps = resultStr.Split('\n');
	        DateTime time = new DateTime();
	        if (timeShtaps.Length > 0)
		        time = DateTime.Parse(timeShtaps[2]);
			UniversalContext context = new UniversalContext();
	        if (context.LastUpdateDataDateTime < time)
	        {
		        await context.UpdateAsync();
		        await context.Save(false);
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
					resultStr = await InternetHelper.Download(urlUpdateNews);
					await FileIO.WriteTextAsync(await ApplicationData.Current.LocalFolder.CreateFileAsync(manager.fileNameNews, CreationCollisionOption.ReplaceExisting),
				        resultStr);
			        await manager.Load(NewsManager.TypeLoad.LoadNews);
			        foreach (var source in manager.NewNews.Where(key => key.Posted > manager.LastUpdateDataDateTime))
			        {
				        ShowNotification(source.Message);
			        }
			        manager.LastUpdateDataDateTime = time;
		        }
				time = DateTime.Parse(timeShtaps[1]);
		        if (time > manager.LastUpdateHotDataDateTime)
		        {
					resultStr = await InternetHelper.Download(urlUpdateHotNews);
			        await FileIO.WriteTextAsync(await ApplicationData.Current.LocalFolder.CreateFileAsync(manager.fileNameHotNews, CreationCollisionOption.ReplaceExisting),
				        resultStr);
			        await manager.Load(NewsManager.TypeLoad.LoadHotNews);
			        foreach (var source in manager.AllHotNews.Where(key => key.Collected > manager.LastUpdateHotDataDateTime))
			        {
				        ShowNotification(source.Message);
			        }
			        manager.LastUpdateHotDataDateTime = time;
		        }
	        }
	        catch (Exception e)
	        {
		        throw;
	        }


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


