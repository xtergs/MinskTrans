using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using MinskTrans.Universal;

namespace BackgroundUpdateTask
{
    public sealed class UpdateBackgroundTask : IBackgroundTask
    {
	    private string urlUpdateDates = @"";
	    private string urlUpdate = @"";

		//Dictionary<int, string> DaysLinks 

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
			BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
			
	        string resultStr = await InternetHelper.Download("dkfjsd");
	        var timeShtaps = resultStr.Split('\n');
	        DateTime time = new DateTime();
	        if (timeShtaps.Length > 0)
		        time = DateTime.Parse(timeShtaps[0]);
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
		        time = DateTime.Parse(timeShtaps[1]);
		        NewsManager manager = new NewsManager();
		        if (time > manager.LastUpdateDataDateTime)
		        {
			        foreach (var source in manager.NewNews.Where(key => key.Key > manager.LastUpdateDataDateTime))
			        {
				        ShowNotification(source.Value);
			        }
			        foreach (var source in manager.AllHotNews.Where(key => key.Key > manager.LastUpdateDataDateTime))
			        {
				        ShowNotification(source.Value);
			        }
			        manager.LastUpdateDataDateTime = time;
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


