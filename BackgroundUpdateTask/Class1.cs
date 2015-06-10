using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Windows.ApplicationModel.Background;
using MinskTrans.Universal;

namespace BackgroundUpdateTask
{
    public sealed class UpdateBackgroundTask : IBackgroundTask
    {
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

	        _deferral.Complete();
			
        }        
    }
}


