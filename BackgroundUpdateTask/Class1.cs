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
	    private string urlUpdateDates = @"";
	    private string urlUpdate = @"";
	    private string urlUpdateNews = @"";
	    private string urlUpdateHotNews = @"";

		static string SettingsToStr([CallerMemberName] string propertyName = null)
		{
			return propertyName;
		}

	    public DateTime LastUpdateDBDatetime
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
			        foreach (var source in manager.NewNews.Where(key => key.Key > manager.LastUpdateDataDateTime))
			        {
				        ShowNotification(source.Value);
			        }
			        manager.LastUpdateDataDateTime = time;
		        }
				time = DateTime.Parse(timeShtaps[1]);
		        if (time > manager.LastUpdateHotDataDateTime)
		        {
					resultStr = await InternetHelper.Download(urlUpdateHotNews);
			        foreach (var source in manager.AllHotNews.Where(key => key.Key > manager.LastUpdateHotDataDateTime))
			        {
				        ShowNotification(source.Value);
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


