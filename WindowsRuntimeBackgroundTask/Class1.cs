using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using CommonLibrary;
using MinskTrans.Universal;

namespace WindowsRuntimeBackgroundTask
{
	

	public sealed class TheTask : IBackgroundTask
	{
		static readonly string LAST_RUN_TIME_SETTING = "lsr";
		static readonly string LAST_RUN_TIME_DEFAULT = "not run";

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			var deferral = taskInstance.GetDeferral();
			bool cancelled = false;

			BackgroundTaskCanceledEventHandler handler = (s, e) =>
			{
				cancelled = true;
			};

			UniversalContext context = new UniversalContext();
			await context.Load();
			
			ApplicationData.Current.LocalSettings.Values[LAST_RUN_TIME_SETTING] =
			  DateTimeOffset.Now;

			deferral.Complete();
		}
		public static string LastRunTime
		{
			get
			{
				object outValue = null;
				string lastRunTime = LAST_RUN_TIME_DEFAULT;

				if (ApplicationData.Current.LocalSettings.Values.TryGetValue(
				  LAST_RUN_TIME_SETTING, out outValue))
				{
					DateTimeOffset dateTime = (DateTimeOffset)outValue;
					lastRunTime = dateTime.ToString("f");
				}
				return (lastRunTime);
			}
		}
		public static void ClearLastRunTime()
		{
			if (ApplicationData.Current.LocalSettings.Values.ContainsKey(LAST_RUN_TIME_SETTING))
			{
				ApplicationData.Current.LocalSettings.Values.Remove(LAST_RUN_TIME_SETTING);
			}
		}
	}  
}
