using System;
using System.Collections.Generic;
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
using Windows.Storage;
using System.Text;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.UI.Popups;
using GalaSoft.MvvmLight.Command;
using MyLibrary;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Utilites;
#if WINDOWS_PHONE_APP
using Windows.UI.Xaml.Media.Animation;
#endif

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace MinskTrans.Universal
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public sealed partial class App : Application
	{



        /// <summary>
        /// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
        /// </summary>
        //public static Microsoft.ApplicationInsights.TelemetryClient TelemetryClient;

        private ILogger log;

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
                    // The user didn't explicitly disable or enable access and updates. 
                    var updateTaskRegistration = RegisterBackgroundTask(backgroundAssembly,
                        backgroundName, new TimeTrigger(15, false), new SystemCondition(SystemConditionType.InternetAvailable));

                    updateTaskRegistration.Completed += async (sender, args) =>
                    {
                        log?.Debug("\nBackground task complited");
                        try
                        {
                            if (
                                MainModelView.MainModelViewGet.SettingsModelView.LastUpdatedDataInBackground.HasFlag(
                                    TypeOfUpdate.Db))
                            {
                                await MainModelView.MainModelViewGet.Context.Save(false);
                                await MainModelView.MainModelViewGet.Context.LoadDataBase(LoadType.LoadAll);
                                log?.Info("Background task complited, DB reloaded");
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
                            //MainModelView.MainModelViewGet.NotifyHelper.ShowMessageAsync()
                        }
                        log?.Info("Background complited, OK");
                    };
                    break;
            }



            var model = MainModelView.MainModelViewGet;

            Frame rootFrame = Window.Current.Content as Frame;

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

                    model.Context.NeedUpdadteDB += async (sender, args) =>
                    {
                        log?.Info("App Need Update");
                        try
                        {
                            await rootFrame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                            {
                                try
                                {
                                    model.IsNeesUpdate = true;
                                    return;
                                    await
                                        model.NotifyHelper.ShowMessageAsync("Необходимо обновить базу данных",
                                            new List<KeyValuePair<string, RelayCommand>>
                                            {
                                                model.NotifyHelper.CreateCommand("Обновить", model.UpdateDataCommand, null)
                                            });
                                }
                                catch (Exception ee)
                                {
                                    log?.Fatal($"App need update: {ee.Message}", ee);
                                    throw;
                                }
                                // Windows.UI.Popups.MessageDialog dialog = new MessageDialog("Необходимо обновить базу данных")
                                // {
                                //  Commands =
                                //	  {
                                //new UICommand("Обновить", command =>
                                //{
                                //	if (model.UpdateDataCommand.CanExecute(null))
                                //		model.UpdateDataCommand.Execute(null);

                                //})
                                //	  }
                                // };
                                // await dialog.ShowAsync();

                            });

                        }
                        catch (Exception ex)
                        {
                            log?.Fatal($"App need update: {ex.Message}", ex);
                            throw;
                        }
                    };


#pragma warning disable 4014
                    model.Context.LoadDataBase();
#pragma warning restore 4014


                    //					timer = new Timer(state =>
                    //					{
                    //#if BETA
                    //						Logger.Log("autoupdate timer elapsed");
                    //#endif
                    //						InternetHelper.UpdateNetworkInformation();
                    //						if ( model.SettingsModelView.HaveConnection())
                    //							if (model.Context.UpdateDataCommand.CanExecute(null))
                    //								model.Context.UpdateAsync();
                    //					}, null, model.SettingsModelView.InvervalAutoUpdateTimeSpan, model.SettingsModelView.InvervalAutoUpdateTimeSpan);

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
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            log.Debug("Activate window");
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
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
            await model.NewsManager.SaveToFile();
            model.SettingsModelView.TypeError = Error.None;
            if (!model.SettingsModelView.KeepTracking)
                model.MapModelView.StopGPS();
            deferral.Complete();

            log.Info("Onsuspending2");
        }



        readonly TimeSpan maxDifTime = new TimeSpan(0, 1, 0, 0);
        private string backgroundAssembly = "MinskTrans.BackgroundUpdateTask.UpdateBackgroundTask";
        private string backgroundName = "UpdateBackground";

        void CallBackReconnectPushServerTimer(object state)
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
                                                                        IBackgroundCondition condition)
        {
            //
            // Check for existing registrations of this background task.
            //
            BackgroundTaskRegistration registered = null;
            bool isRegistered = false;
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {

                if (cur.Value.Name == taskName)
                {
                    // 
                    // The task is already registered.
                    // 
                    isRegistered = true;

                    registered = (BackgroundTaskRegistration)(cur.Value);
                }
                else
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
            var configuration = new LoggingConfiguration();
#if DEBUG
            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new DebugTarget());
#endif
            configuration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget());
            configuration.IsEnabled = true;

            LogManagerFactory.DefaultConfiguration = configuration;
            //         LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new FileStreamingTarget());
            //LogManagerFactory.DefaultConfiguration.IsEnabled = true;
            log = LogManagerFactory.DefaultLogManager.GetLogger<App>();

            log.Debug("\n\nApp constructor started");

            //TelemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();

            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
#if !WINDOWS_PHONE_APP
            GlobalCrashHandler.Configure();
#endif

            this.UnhandledException += OnUnhandledException;



            //MainModelView.Create(new UniversalContext(new FileHelper()));

            log.Debug("App ended");

        }

        async Task SaveToFile(string str)
        {
            var storage = await ApplicationData.Current.LocalFolder.CreateFileAsync("Error.txt");
            await FileIO.AppendTextAsync(storage, str);
        }

        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var settings = MainModelView.MainModelViewGet.SettingsModelView;

            log?.Fatal($"App.OnUnhadledException: {unhandledExceptionEventArgs.Message}\n", unhandledExceptionEventArgs.Exception);

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
            await MainModelView.MainModelViewGet.Context.LoadDataBase();

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