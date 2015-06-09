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
			UniversalContext context = new UniversalContext();
	        await context.UpdateAsync();
			

			_deferral.Complete();
			
        }        
    }
}


