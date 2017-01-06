using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;
using Autofac;
using CommonLibrary;
using CommonLibrary.IO;
using CommonLibrary.ModelView;
using CommonLibrary.Notify;
using MetroLog;
using MetroLog.Targets;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Fakes;
using MinskTrans.Context.Utilites;
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
		private TelemetryClient TelemetryClient;
		//Dictionary<int, string> DaysLinks 

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
			TelemetryConfiguration.Active.InstrumentationKey = "21fc407a-013d-402a-b0de-6ab3157af6bf";
			TelemetryClient = new TelemetryClient(TelemetryConfiguration.Active);

			Stopwatch backgroundActive = new Stopwatch();
			backgroundActive.Start();
			var configuration = new LoggingConfiguration();

			configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());

			//configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget());
			configuration.IsEnabled = true;
		    try
		    {
		        LogManagerFactory.DefaultConfiguration = configuration;
		    }
            catch(Exception e)
            { }
		    ILogger Log = null;
			TypeOfUpdates updateTypes = TypeOfUpdates.All;
			var details = taskInstance.TriggerDetails as RawNotification;

			if (details != null)
			{
				TypeOfUpdates tempEnum;
				if (Enum.TryParse(details.Content, true, out tempEnum))
					updateTypes = tempEnum;
				// Perform tasks
				TelemetryClient.TrackEvent("Background task by push");
			}
			else
				TelemetryClient.TrackEvent("Background task by timer");

			BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
			try
			{
				if (_cancelRequested)
					return;
				//Debug.WriteLine("Background task started");
				var builder = new ContainerBuilder();
				builder.RegisterType<FileHelper>().As<FileHelperBase>().SingleInstance();
				//builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity3_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
				builder.RegisterType<Context>().As<IContext>().SingleInstance();
				builder.RegisterType<UpdateManagerBase>().SingleInstance();
				builder.RegisterType<NewsManager>().As<NewsManagerBase>().SingleInstance();
				builder.RegisterType<BaseNewsContext>().As<INewsContext>().SingleInstance();
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

				var context = container.Resolve<IBussnessLogics>();
				InternetHelperBase helper = container.Resolve<InternetHelperBase>();
				//UpdateManagerBase updateManager = new UpdateManagerBase(fileHelper, internetHelper, new ShedulerParser());

				settings.LastUpdatedDataInBackground = TypeOfUpdate.None;
				helper.UpdateNetworkInformation();
				if (!settings.HaveConnection())
				{
					Log?.Info("Background: Have no connection, exiting");
					//_deferral.Complete();
					return;
				}
				MaxDaysAgo = 30;
				MaxMinsAgo = 20;

				if (_cancelRequested)
				{
					Log?.Trace("Cancel requested");
					return;
				}

			    DateTime oldDaylyTime;
			    DateTime oldMonthTime;
			    try
			    {
			        oldDaylyTime = settings.LastSeenHotNewsDateTimeUtc;
			        oldMonthTime = settings.LastSeenMainNewsDateTimeUtc;
			    }
                catch (FormatException e)
                {
                    Log?.Error("Background: " + e.Message, e);
                    TelemetryClient.TrackException(e, new Dictionary<string, string>()
                    {
                        ["Background task"] = "General",
                        ["Version"] = "3"
                    });
                    settings.LastSeenHotNewsDateTimeUtc = DateTime.UtcNow;
                    settings.LastSeenMainNewsDateTimeUtc = DateTime.UtcNow;
                    oldDaylyTime = settings.LastSeenHotNewsDateTimeUtc;
                    oldMonthTime = settings.LastSeenMainNewsDateTimeUtc;
                }
                bool isHaveNews = false;
				try
				{
					if (updateTypes.HasFlag(TypeOfUpdates.TimeTable))
						if (await context.UpdateTimeTableAsync(cancellationTokenSource.Token, true))
						{
							settings.LastUpdatedDataInBackground |= TypeOfUpdate.Db;
						}
					if (_cancelRequested)
					{
						Log?.Trace("Cancel requested");
						return;
					}

					if (updateTypes.HasFlag(TypeOfUpdates.HotNews) || updateTypes.HasFlag(TypeOfUpdates.MainNews))
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
					//_deferral.Complete();
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
				if (!settings.NotifyAboutNews)
				{
					//_deferral.Complete();
					return;
				}
				var allEntries = listOfDaylyNews.Concat(listOfMonthNews);
				var newsEntries = allEntries as NewsEntry[] ?? allEntries.ToArray();
				INotifyHelper notify = container.Resolve<INotifyHelper>();
				if (newsEntries.Count() > 2)
				{
					notify.ShowNotificaton($"Новые уведомления: {newsEntries.Count()}");
				}
				else
					foreach (var source in newsEntries)
					{
						notify.ShowNotificaton(source.Message);
					}

			    try
			    {
			        settings.LastSeenMainNewsDateTimeUtc = nowTimeUtc;
			        settings.LastSeenHotNewsDateTimeUtc = nowTimeUtc;
			    }
			    catch (Exception ex)
			    {
                    TelemetryClient.TrackException(ex, new Dictionary<string, string>()
                    {
                        ["Background task"] = "General",
                        ["Background task"] = "InTheEnd setting lastUpdateTime"
                    });
                }
				Log?.Error("Background ended ");
			}
			catch (FormatException e)
			{
				Log?.Error("Background: " + e.Message, e);
				TelemetryClient.TrackException(e, new Dictionary<string, string>()
				{
				    ["Background task"]= "General",
                    ["Version"] = "3"
                });
            }
			finally
			{
				backgroundActive.Stop();
				TelemetryClient.TrackMetric("Background task contuniuty", backgroundActive.ElapsedMilliseconds,
					new Dictionary<string, string>() {["UpdateTypes"] = updateTypes.ToString()});
				TelemetryClient.Flush();
				await Task.Delay(100);
				_deferral.Complete();
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
			TelemetryClient.TrackEvent("Task has been canceled");
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