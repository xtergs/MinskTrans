using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MetroLog;
using MetroLog.Targets;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MinskTrans.Universal;
using Windows.UI.Xaml.Navigation;
using MinskTrans.Universal.ModelView;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Core;
using GoogleAnalytics;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.WindowsAzure.Messaging;
using MyLibrary;
using MinskTrans.Context.Base;
using Microsoft.HockeyApp;
using UniversalMinskTransRelease.Helpers;
using UnhandledExceptionEventArgs = Windows.UI.Xaml.UnhandledExceptionEventArgs;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=402347&clcid=0x409

namespace UniversalMinskTrans
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		/// <summary>
		/// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
		/// </summary>
		public static Microsoft.ApplicationInsights.TelemetryClient TelemetryClient;

		private ILogger log;

		private async Task<bool> InitNotificationsAsync()
		{
			var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

			var hub = new NotificationHub(AppConstants.PushNotificationChanelHubName, AppConstants.PushNotificationChanelEndPoint);
			var result = await hub.RegisterNativeAsync(channel.Uri);
			return result.RegistrationId != null;

			// Displays the registration ID so you know it was successful
			//if (result.RegistrationId != null)
			//{
			//    var dialog = new MessageDialog("Registration successful: " + result.RegistrationId);
			//    dialog.Commands.Add(new UICommand("OK"));
			//    await dialog.ShowAsync();
			//}

		}

        public static string GetAppVersion()
        {

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

        }

        private void SetupAnalitics()
	    {
	        var config = new EasyTrackerConfig();
            //config.AppName = "MinskTransUWP";
            //config.AppVersion = GetAppVersion();
#if !DEBUG
            config.TrackingId = AppConstants.GoogleAnalitics;
#endif
#if DEBUG
            config.Debug = true;
#endif
            GoogleAnalytics.EasyTracker.Current.Config = config;

	    }

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override async void OnLaunched(LaunchActivatedEventArgs e)
		{
			log?.Debug("App.OnLaunched started");

			

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
			Frame rootFrame = null;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			InitNotificationsAsync().ContinueWith(async (state) =>
			{
				if (!state.IsCompleted)
					return;
				var isPushNotificationHubRegistered = state.Result;

				var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
				switch (backgroundAccessStatus)
				{
					case BackgroundAccessStatus.Denied:
						log?.Warn("BackgroundAccessStatus:Denied");
						// Windows: Background activity and updates for this app are disabled by the user.
						//
						// Windows Phone: The maximum number of background apps allowed across the system has been reached or
						// background activity and updates for this app are disabled by the user.
						break;

					case BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity:
                    // Windows: Added to list of background apps; set up background tasks; 
                    // can use the network connectivity broker.
                    //
                    // Windows Phone: This value is never used on Windows Phone.
                    //break;
					case BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity:
					case BackgroundAccessStatus.Unspecified:
                    default:
						// The user didn't explicitly disable or enable access and updates. 
						BackgroundTaskRegistration updateTaskRegistration = null;
#if DEBUG
                        updateTaskRegistration = RegisterBackgroundTask(backgroundAssembly,
                                backgroundName, new TimeTrigger(15, false),
                                new SystemCondition(SystemConditionType.InternetAvailable), new[] { backgroundPushName });
                        updateTaskRegistration.Completed += UpdateTaskRegistrationOnCompleted;
                        updateTaskRegistration = RegisterBackgroundTask(backgroundAssembly,
                                backgroundPushName, new PushNotificationTrigger(),
                                new SystemCondition(SystemConditionType.InternetAvailable), new[] { backgroundName });
#else
                        if (!isPushNotificationHubRegistered)
							updateTaskRegistration = RegisterBackgroundTask(backgroundAssembly,
								backgroundName, new TimeTrigger(15, false),
								new SystemCondition(SystemConditionType.InternetAvailable), new[] {""});
						else
							updateTaskRegistration = RegisterBackgroundTask(backgroundAssembly,
								backgroundPushName, new PushNotificationTrigger(),
								new SystemCondition(SystemConditionType.InternetAvailable), new[] {""});
#endif
                        updateTaskRegistration.Completed += UpdateTaskRegistrationOnCompleted;


						break;
				}
			}).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed


			var model = MainModelView.MainModelViewGet;


			rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
				log?.Info("RestoreAppState");

				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				// TODO: change this value to a cache size that is appropriate for your application
				rootFrame.CacheSize = 3;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					log?.Info("Load after suspending");
					// TODO: Load state from previously suspended application
					//MainModelView.MainModelViewGet.Context.Load();
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
				if (e.PreviousExecutionState != ApplicationExecutionState.Running)
				{
					log?.Info("Prev state != Running");

					model.Context.NeedUpdadteDB += TimeTabletOnNeedUpdadteDb;
					model.Context.LoadStarted += TimeTableOnLoadStarted;
					model.Context.LoadEnded += TimeTableOnLoadEnded;
					model.Context.UpdateDBStarted += TimeTableOnLoadStarted;
					model.Context.UpdateDBEnded += TimeTableOnUpdateDbEnded;


#pragma warning disable 4014
					Stopwatch loadStopwatch = new Stopwatch();
					loadStopwatch.Start();
					model.LoadAllData().ContinueWith(
						x =>
						{
							if (x.IsCompleted)
							{
								loadStopwatch.Stop();
								TelemetryClient.TrackMetric("StartupDataLoaded", loadStopwatch.ElapsedMilliseconds);
								loadStopwatch = null;
								return;
							}
							if (x.IsFaulted)
							{
								loadStopwatch.Stop();
								loadStopwatch = null;
								TelemetryClient.TrackException(x.Exception);
							}
						});
					
#pragma warning restore 4014
				}
			}


			if (rootFrame.Content == null)
			{
#if WINDOWS_PHONE_APP
	// Removes the turnstile navigation for startup.
				if (rootFrame.ContentTransitions != null)
				{
					this.transitions = new TransitionCollection();
					foreach (var c in rootFrame.ContentTransitions)
					{
						this.transitions.Add(c);
					}
				}

				rootFrame.ContentTransitions = null;
				rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

				// When the navigation stack isn't restored navigate to the first page,
				// configuring the new page by passing required information as a navigation
				// parameter
				if (!rootFrame.Navigate(typeof (MainPage), e.Arguments))
				{
					throw new Exception("Failed to create initial page");
				}
			}

			log.Debug("Activate window");
			// Ensure the current window is active
			Window.Current.Activate();
		}


		private async void UpdateTaskRegistrationOnCompleted(BackgroundTaskRegistration sender,
			BackgroundTaskCompletedEventArgs args)
		{
			log?.Debug("\nBackground task complited");
			try
			{
				args.CheckResult();
				if (
					MainModelView.MainModelViewGet.SettingsModelView.LastUpdatedDataInBackground.HasFlag(
						TypeOfUpdate.Db))
				{
					try
					{
						await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
							CoreDispatcherPriority.Normal, () =>
							{
								MainModelView
									.MainModelViewGet
									.IsLoading
									= true;
							});
						await MainModelView.MainModelViewGet.Context.Save(false);
						await MainModelView.MainModelViewGet.Context.LoadDataBase(LoadType.LoadAll);
						log?.Info("Background task complited, DB reloaded");
					}
					finally
					{
						await
							Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
								CoreDispatcherPriority.Normal,
								() => { MainModelView.MainModelViewGet.IsLoading = false; });
					}
				}
				if (MainModelView.MainModelViewGet.SettingsModelView.LastUpdatedDataInBackground.HasFlag(
					TypeOfUpdate.News))
				{
					await MainModelView.MainModelViewGet.NewsManager.Load();
					log?.Info("Background task complited, news loaded");
				}
				//MainModelView.MainModelViewGet.AllNews = null;
			}
			catch (Exception ex)
			{
				log?.Fatal("Backroudn complited", ex);
				TelemetryClient.TrackException(ex, new Dictionary<string, string>() {["App"]="Background task: on complited"});
				//MainModelView.MainModelViewGet.NotifyHelper.ShowMessageAsync()
			}
			log?.Info("Background complited, OK");
		}

		private async void TimeTableOnUpdateDbEnded(object sender, EventArgs eventArgs)
		{
			var model = MainModelView.MainModelViewGet;
			await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
				CoreDispatcherPriority.Normal, () =>
				{
					model.IsLoading = false;
					model.IsNeesUpdate = false;
				});
		}

		private async void TimeTabletOnNeedUpdadteDb(object sender, EventArgs eventArgs)
		{
			var model = MainModelView.MainModelViewGet;

			log?.Info("App Need Update");
			try
			{
				await
					Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
						CoreDispatcherPriority.Normal, () =>
						{
							try
							{
								model.IsNeesUpdate = true;
							}
							catch (Exception ee)
							{
								log?.Fatal($"App need update: {ee.Message}", ee);
								throw;
							}
						});
			}
			catch (Exception ex)
			{
				log?.Fatal($"App need update: {ex.Message}", ex);
				throw;
			}
		}

		private async void TimeTableOnLoadStarted(object sender, EventArgs eventArgs)
		{
			var model = MainModelView.MainModelViewGet;
			await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
				CoreDispatcherPriority.Normal, () => { model.IsLoading = true; });
		}

		private async void TimeTableOnLoadEnded(object sender, EventArgs eventArgs)
		{
			var model = MainModelView.MainModelViewGet;
			await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
				CoreDispatcherPriority.Normal, () =>
				{
					model.IsLoading = false;
					if (model.Context.Context.Stops?.Any() == true)
						model.IsNeesUpdate = false;
				});
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private async void OnSuspending(object sender, SuspendingEventArgs e)
		{
			log.Info("Onsuspending");

			var deferral = e.SuspendingOperation.GetDeferral();
			var model = MainModelView.MainModelViewGet;
			await model.Context.Save(saveAllDB: false);
			//await model.NewsManager.SaveToFile();
			//model.SettingsModelView.TypeError = Error.None;
			if (!model.SettingsModelView.KeepTracking)
				model.MapModelView.StopGPS();
			deferral.Complete();

			log.Info("Onsuspending2");
		}


		private readonly TimeSpan maxDifTime = new TimeSpan(0, 1, 0, 0);
		private string backgroundAssembly = "BackgroundUpdateTaskUniversalRuntime.UpdateBackgroundTask";
		private string backgroundName = "UpdateBackground11";
		private string backgroundPushName = "PushBackground12";

		private void CallBackReconnectPushServerTimer(object state)
		{
			//InitNotificationsAsync();
		}


		//
		// Register a background task with the specified taskEntryPoint, name, trigger,
		// and condition (optional).
		//
		// taskEntryPoint: Task entry point for the background task.
		// taskName: A name for the background task.
		// trigger: The trigger for the background task.
		// condition: Optional parameter. A conditional event that must be true for the task to fire.
		//
		public static BackgroundTaskRegistration RegisterBackgroundTask(string taskEntryPoint,
			string taskName,
			IBackgroundTrigger trigger,
			IBackgroundCondition condition,
			string[] exclusive)
		{
			//
			// Check for existing registrations of this background task.
			//
			BackgroundTaskRegistration registered = null;
			bool isRegistered = false;
			foreach (var cur in BackgroundTaskRegistration.AllTasks)
			{
				if (cur.Value.Name == taskName )
				{
					// 
					// The task is already registered.
					// 
					isRegistered = true;

					registered = (BackgroundTaskRegistration) (cur.Value);
				}
				else
					if (!exclusive.Contains(cur.Value.Name))
						cur.Value.Unregister(true);
			}

			if (isRegistered)
				return registered;

			//
			// Register the background task.
			//

			var builder = new BackgroundTaskBuilder();


			builder.Name = taskName;
			builder.TaskEntryPoint = taskEntryPoint;
			builder.SetTrigger(trigger);

			if (condition != null)
			{
				builder.AddCondition(condition);
			}

			BackgroundTaskRegistration task = builder.Register();

			return task;
		}


#if WINDOWS_PHONE_APP
		private TransitionCollection transitions;
#endif

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			HockeyClient.Current.Configure(AppConstants.HockeyAppId);
		    SetupAnalitics();
            var configuration = new LoggingConfiguration();
#if DEBUG
			configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
#endif
			configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new MetroLog.Targets.StreamingFileTarget());
			configuration.IsEnabled = true;

			LogManagerFactory.DefaultConfiguration = configuration;
			//         LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget());
			//LogManagerFactory.DefaultConfiguration.IsEnabled = true;
			log = LogManagerFactory.DefaultLogManager.GetLogger<App>();

			log.Debug("\n\nApp constructor started");
			Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = AppConstants.InsightsTelemetry;

			TelemetryClient = new Microsoft.ApplicationInsights.TelemetryClient(Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active);


			string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
			ulong version = ulong.Parse(deviceFamilyVersion);
			ulong major = (version & 0xFFFF000000000000L) >> 48;
			ulong minor = (version & 0x0000FFFF00000000L) >> 32;
			ulong build = (version & 0x00000000FFFF0000L) >> 16;
			ulong revision = (version & 0x000000000000FFFFL);
			var osVersion = $"{major}.{minor}.{build}.{revision}";

			TelemetryClient.Context.Device.OperatingSystem = osVersion;
			TelemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
			TelemetryClient.TrackEvent(new EventTelemetry("App constructor"));
			TelemetryClient.Flush();

			this.InitializeComponent();
			this.Suspending += this.OnSuspending;

			//GlobalCrashHandler.Configure();

			this.UnhandledException += OnUnhandledException;


			//MainModelView.Create(new UniversalContext(new FileHelper()));

			log.Debug("App ended");
		}

		private async Task SaveToFile(string str)
		{
			var storage = await ApplicationData.Current.LocalFolder.CreateFileAsync("Error.txt");
			await FileIO.AppendTextAsync(storage, str);
		}

		private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
			var settings = MainModelView.MainModelViewGet.SettingsModelView;

			log?.Fatal($"App.OnUnhadledException: {unhandledExceptionEventArgs.Message}\n",
				unhandledExceptionEventArgs.Exception);
			TelemetryClient.TrackException(unhandledExceptionEventArgs.Exception, new Dictionary<string,string>()
			{
				["Message"] = unhandledExceptionEventArgs.Message
			});
			if (settings.TypeError == Error.Critical)
				settings.TypeError = Error.Repeated;
			else if (settings.TypeError == Error.Repeated)
			{
				await MainModelView.MainModelViewGet.Context.Context.Recover();
			}
			else
			{
				settings.TypeError = Error.Critical;
			}
		}

#region Overrides of Application

		protected override async void OnActivated(IActivatedEventArgs args)
		{
			log?.Debug("App OnActivated");

			base.OnActivated(args);
			await MainModelView.MainModelViewGet.LoadAllData();

			log?.Debug("App OnActivated, context loaded");
		}

#endregion

#if WINDOWS_PHONE_APP
	/// <summary>
	/// Restores the content transitions after the app has launched.
	/// </summary>
	/// <param name="sender">The object where the handler is attached.</param>
	/// <param name="e">Details about the navigation event.</param>
		private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
		{
			var rootFrame = sender as Frame;
			rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
			rootFrame.Navigated -= this.RootFrame_FirstNavigated;
		}
#endif
	}
}