using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Windows.ApplicationModel.Background;

namespace BackgroundUpdateTask
{
    public sealed class UpdateBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
			BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();

			//
			// TODO: Insert code to start one or more asynchronous methods using the
			//       await keyword, for example:
			//
			// await ExampleMethodAsync();
			//

			_deferral.Complete();
			
        }        
    }
}


