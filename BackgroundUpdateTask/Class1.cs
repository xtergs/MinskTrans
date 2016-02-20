using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Autofac;
using CommonLibrary;
using CommonLibrary.IO;
using MetroLog;
using MetroLog.Targets;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Fakes;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MyLibrary;

namespace MinskTrans.BackgroundUpdateTask
{
	public sealed class UpdateBackgroundTask : IBackgroundTask
	{

/*
		private string urlUpdateDates = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111529&authkey=%21ADs9KNHO9TDPE3Q&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D4";
*/
/*
		private string urlUpdateNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111532&authkey=%21AAQED1sY1RWFib8&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D8";
*/
/*
		private string urlUpdateHotNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111531&authkey=%21AIJo-8Q4661GpiI&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D2";
*/
/*
		private string fileNews = "datesNews.dat";
*/
	    private bool _cancelRequested;
	    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private BackgroundTaskCancellationReason _cancelReason;
	    int MaxDaysAgo { get; set; }
		int MaxMinsAgo { get; set; }

		//Dictionary<int, string> DaysLinks 

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
            var configuration = new LoggingConfiguration();

            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());

            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget());
            configuration.IsEnabled = true;

            LogManagerFactory.DefaultConfiguration = configuration;
            
		    try
		    {
		        //Debug.WriteLine("Background task started");

		        BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
		        var builder = new ContainerBuilder();
		        builder.RegisterType<FileHelper>().As<FileHelperBase>().SingleInstance();
		        //builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity3_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
		        builder.RegisterType<UniversalContext>().As<IContext>().SingleInstance();
		        builder.RegisterType<UpdateManagerBase>().SingleInstance();
		        builder.RegisterType<NewsManager>().As<NewsManagerBase>().SingleInstance();
		        builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>().SingleInstance();
		        builder.RegisterType<ShedulerParser>().As<ITimeTableParser>().SingleInstance();
		        builder.RegisterType<UniversalApplicationSettingsHelper>().As<IApplicationSettingsHelper>();
		        builder.RegisterType<SettingsModelView>().As<ISettingsModelView>().SingleInstance();
		        builder.RegisterType<BussnessLogic>().As<IBussnessLogics>();
		        builder.RegisterType<FakeGeolocation>().As<IGeolocation>();
                builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
                var container = builder.Build();
		        //Log = container.Resolve<ILogger>();
		        ILogger Log = container.Resolve<ILogManager>().GetLogger<UpdateBackgroundTask>();
                Log.Trace("Background task started");
                //var fileHelper = new FileHelper();
                //InternetHelperBase internetHelper = new InternetHelperUniversal(fileHelper);
                ISettingsModelView settings = container.Resolve<ISettingsModelView>();

		        var context = container.Resolve<IBussnessLogics>();
		        InternetHelperBase helper = container.Resolve<InternetHelperBase>();
		        //UpdateManagerBase updateManager = new UpdateManagerBase(fileHelper, internetHelper, new ShedulerParser());

		        settings.LastUpdatedDataInBackground = TypeOfUpdate.None;
		        helper.UpdateNetworkInformation();
		        if (!settings.HaveConnection())
		            _deferral.Complete();
		        MaxDaysAgo = 30;
		        MaxMinsAgo = 20;


		        var oldDaylyTime = settings.LastSeenHotNewsDateTimeUtc;
		        var oldMonthTime = settings.LastSeenMainNewsDateTimeUtc;
		        try
		        {
		            
		            if (await context.UpdateTimeTableAsync(cancellationTokenSource.Token, true))
		            {
		                settings.LastUpdatedDataInBackground |= TypeOfUpdate.Db;
		            }

		            if (await context.UpdateNewsTableAsync(cancellationTokenSource.Token))
		            {
		                settings.LastUpdatedDataInBackground |= TypeOfUpdate.News;
		            }
		        }
		        catch (Exception e)
		        {
		            Log.Error("Background: " + e.Message, e);
		            throw;
		        }

		        
		        var manager = container.Resolve<NewsManagerBase>();
		        await manager.Load();
		        var nowTimeUtc = DateTime.UtcNow;
		        var listOfDaylyNews = manager.NewNews.Where(
		            key => key.PostedUtc > oldMonthTime && ((nowTimeUtc - key.PostedUtc).TotalDays < MaxDaysAgo));
		        var listOfMonthNews = manager.AllHotNews.Where(key =>
		        {
		            if (key.RepairedLineUtc != default(DateTime))
		            {
		                double totalminutes = (nowTimeUtc.ToLocalTime() - key.RepairedLineLocal).TotalMinutes;
		                if (totalminutes <= MaxMinsAgo)
		                    return true;
		                return false;
		            }
		            return (key.CollectedUtc > oldDaylyTime) &&
		                   ((nowTimeUtc - key.CollectedUtc).TotalDays < 1);
		        });


		        foreach (var source in listOfDaylyNews.Concat(listOfMonthNews))
		        {
		            ShowNotification(source.Message);
		        }

                Log.Info("Background.settings.LastSeenMainNewsDateTimeUtc before : " + settings.LastSeenMainNewsDateTimeUtc);
                Log.Info("Background.settings.LastSeenHotNewsDateTimeUtc before: " + settings.LastSeenHotNewsDateTimeUtc);

                settings.LastSeenMainNewsDateTimeUtc = nowTimeUtc;
		        settings.LastSeenHotNewsDateTimeUtc = nowTimeUtc;

                Log.Info("Background.settings.LastSeenMainNewsDateTimeUtc before : " + settings.LastSeenMainNewsDateTimeUtc);
                Log.Info("Background.settings.LastSeenHotNewsDateTimeUtc before: " + settings.LastSeenHotNewsDateTimeUtc);

                
                Log.Error("Background ended " );
                _deferral.Complete();
		    }
		    catch (Exception)
		    {
		        //Log.Error("Background: " + e.Message, e);
                
		    }

		}

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // 
            // Indicate that the background task is canceled. 
            // 
            _cancelRequested = true;
            _cancelReason = reason;
            cancellationTokenSource.Cancel();

            Debug.WriteLine("Background " + sender.Task.Name + " Cancel Requested...");
        }


        void ShowNotification(string text)
		{
			var notifi = ToastNotificationManager.CreateToastNotifier();

			var xaml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
			var textNode = xaml.GetElementsByTagName("text");
			textNode.Item(0)?.AppendChild(xaml.CreateTextNode(text));
			//value.appendChild(toastXml.createTextNode(text));
			ToastNotification notification = new ToastNotification(xaml);
			notifi.Show(notification);
		}


	}
}


