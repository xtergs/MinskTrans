using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;
using Autofac;
using CommonLibrary;
using CommonLibrary.IO;
using CommonLibrary.Notify;
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
using UniversalMinskTransRelease.Nofity;

namespace BackgroundUpdateTaskUniversalRuntime
{
    public sealed class UpdateBackgroundTask : IBackgroundTask
    {
        private bool _cancelRequested = false;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private BackgroundTaskCancellationReason _cancelReason;
        private int MaxDaysAgo { get; set; }
        private int MaxMinsAgo { get; set; }

        //Dictionary<int, string> DaysLinks 

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            var configuration = new LoggingConfiguration();

            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());

            //configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget());
            configuration.IsEnabled = true;
            LogManagerFactory.DefaultConfiguration = configuration;
            ILogger Log = null;

	        var details = taskInstance.TriggerDetails as RawNotification;

			if (details != null)
			{
				
				// Perform tasks
			}

			try
            {
                //Debug.WriteLine("Background task started");
                if (_cancelRequested)
                    return;
                BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
                var builder = new ContainerBuilder();
                builder.RegisterType<FileHelper>().As<FileHelperBase>().SingleInstance();
                //builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity3_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
                builder.RegisterType<Context>().As<IContext>().SingleInstance();
                builder.RegisterType<UpdateManagerBase>().SingleInstance();
                builder.RegisterType<NewsManager>().As<NewsManagerBase>().SingleInstance();
                builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>().SingleInstance();
                builder.RegisterType<ShedulerParser>().As<ITimeTableParser>().SingleInstance();
                builder.RegisterType<UniversalApplicationSettingsHelper>().As<IApplicationSettingsHelper>();
                builder.RegisterType<SettingsModelView>().As<ISettingsModelView>().SingleInstance();
                builder.RegisterType<BussnessLogic>().As<IBussnessLogics>();
                builder.RegisterType<FakeGeolocation>().As<IGeolocation>();
                builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
                builder.RegisterType<NotifyHelperUniversal>().As<INotifyHelper>();
                builder.RegisterType<FilePathsSettings>();
                var container = builder.Build();
                //Log = container.Resolve<ILogger>();
                //Log = container.Resolve<ILogManager>().GetLogger<UpdateBackgroundTask>();
                Log?.Trace("\n\nBackground task started");

                if (_cancelRequested)
                {
                    Log?.Trace("Cancel requested");
                    return;
                }
                //var fileHelper = new FileHelper();
                //InternetHelperBase internetHelper = new InternetHelperUniversal(fileHelper);
                ISettingsModelView settings = container.Resolve<ISettingsModelView>();
                INotifyHelper notify = container.Resolve<INotifyHelper>();
                var context = container.Resolve<IBussnessLogics>();
                InternetHelperBase helper = container.Resolve<InternetHelperBase>();
                //UpdateManagerBase updateManager = new UpdateManagerBase(fileHelper, internetHelper, new ShedulerParser());

                settings.LastUpdatedDataInBackground = TypeOfUpdate.None;
                helper.UpdateNetworkInformation();
                if (!settings.HaveConnection())
                {
                    Log?.Info("Background: Have no connection, exiting");
                    _deferral.Complete();
                    return;
                }
                MaxDaysAgo = 30;
                MaxMinsAgo = 20;

                if (_cancelRequested)
                {
                    Log?.Trace("Cancel requested");
                    return;
                }

                var oldDaylyTime = settings.LastSeenHotNewsDateTimeUtc;
                var oldMonthTime = settings.LastSeenMainNewsDateTimeUtc;
                bool isHaveNews = false;
                try
                {
                    if (await context.UpdateTimeTableAsync(cancellationTokenSource.Token, true))
                    {
                        settings.LastUpdatedDataInBackground |= TypeOfUpdate.Db;
                    }
                    if (_cancelRequested)
                    {
                        Log?.Trace("Cancel requested");
                        return;
                    }

                    if (await context.UpdateNewsTableAsync(cancellationTokenSource.Token))
                    {
                        settings.LastUpdatedDataInBackground |= TypeOfUpdate.News;
                        isHaveNews = true;
                    }
                }
                catch (Exception e)
                {
                    Log?.Fatal("Background: " + e.Message, e);
                    throw;
                }

                if (!isHaveNews)
                {
                    Log?.Info("No new news, exiting");
                    _deferral.Complete();
                    return;
                }
                if (_cancelRequested)
                {
                    Log?.Trace("Cancel requested");
                    return;
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
                if (_cancelRequested)
                {
                    Log?.Trace("Cancel requested");
                    return;
                }

                var allEntries = listOfDaylyNews.Concat(listOfMonthNews);
                var newsEntries = allEntries as NewsEntry[] ?? allEntries.ToArray();
                if (newsEntries.Count() > 2)
                {
                    notify.ShowNotificaton($"Новые уведомления: {newsEntries.Count()}");
                }
                else
                    foreach (var source in newsEntries)
                    {
                        notify.ShowNotificaton(source.Message);
                    }

                Log?.Info("Background.settings.LastSeenMainNewsDateTimeUtc before : " +
                         settings.LastSeenMainNewsDateTimeUtc);
                Log?.Info("Background.settings.LastSeenHotNewsDateTimeUtc before: " + settings.LastSeenHotNewsDateTimeUtc);

                settings.LastSeenMainNewsDateTimeUtc = nowTimeUtc;
                settings.LastSeenHotNewsDateTimeUtc = nowTimeUtc;

                Log?.Info("Background.settings.LastSeenMainNewsDateTimeUtc before : " +
                         settings.LastSeenMainNewsDateTimeUtc);
                Log?.Info("Background.settings.LastSeenHotNewsDateTimeUtc before: " + settings.LastSeenHotNewsDateTimeUtc);


                Log?.Error("Background ended ");
                _deferral.Complete();
            }
            catch (Exception e)
            {
                Log?.Error("Background: " + e.Message, e);
            }

            Log?.Info("Background task, OK");
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


        //      void ShowNotification(string text)
        //{
        //	var notifi = ToastNotificationManager.CreateToastNotifier();

        //	var xaml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
        //	var textNode = xaml.GetElementsByTagName("text");
        //	textNode.Item(0)?.AppendChild(xaml.CreateTextNode(text));
        //	//value.appendChild(toastXml.createTextNode(text));
        //	ToastNotification notification = new ToastNotification(xaml);
        //	notifi.Show(notification);
        //}
    }
}