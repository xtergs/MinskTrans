using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MinskTrans.Universal;
using Windows.UI.Xaml.Navigation;
using MinskTrans.Universal.ModelView;
using CommonLibrary;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Windows.Storage;
using System.Text;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.DesctopClient;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.UI.Popups;
using MyLibrary;
using CommonLibrary.IO;

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



		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override async void OnLaunched(LaunchActivatedEventArgs e)
		{
			//InitNotificationsAsync();

#if BETA
			Logger.Log("App.OnLaunched started");
			Debug.WriteLine("App.OnLaunched started");
#endif
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
					break;

				case BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity:
				case BackgroundAccessStatus.Unspecified:
					// The user didn't explicitly disable or enable access and updates. 
					var updateTaskRegistration = RegisterBackgroundTask("MinskTrans.BackgroundUpdateTask.UpdateBackgroundTask",
						"UpdateBackgroundTasks", new TimeTrigger(15, false),
						 new SystemCondition(SystemConditionType.InternetAvailable));

					updateTaskRegistration.Completed += async (sender, args) =>
					{
						try
						{
							if (
								MainModelView.MainModelViewGet.SettingsModelView.LastUpdatedDataInBackground.HasFlag(
									SettingsModelView.TypeOfUpdate.Db))
							{
								await MainModelView.MainModelViewGet.Context.Save(false);
								await MainModelView.MainModelViewGet.Context.Load(LoadType.LoadAll);
							}
							if (MainModelView.MainModelViewGet.SettingsModelView.LastUpdatedDataInBackground.HasFlag(
								SettingsModelView.TypeOfUpdate.News))
							{
								await MainModelView.MainModelViewGet.NewsManager.Load();
							}
							//MainModelView.MainModelViewGet.AllNews = null;
						}
						catch (Exception ex)
						{
							string message =
								(new StringBuilder("BackgroundTask complited")).AppendLine()
									.AppendLine(ex.ToString())
									.AppendLine(ex.Message)
									.AppendLine(ex.StackTrace).ToString();
							Debug.WriteLine(message);
#if BETA
							Logger.Log(message);
#endif
							throw;
						}
					};
					break;
			}



			var model = MainModelView.MainModelViewGet;

			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
#if BETA
				Logger.Log("RestoreAppState");
#endif
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				// TODO: change this value to a cache size that is appropriate for your application
				rootFrame.CacheSize = 1;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
#if BETA
					Logger.Log("Load after suspending");
#endif
					// TODO: Load state from previously suspended application
					//MainModelView.MainModelViewGet.Context.Load();
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
				if (e.PreviousExecutionState != ApplicationExecutionState.Running)
				{
#if BETA
					Logger.Log("Prev state != Running");
#endif
					model.Context.ErrorLoading += async (sender, args) =>
					{

						if (args.Error == ErrorLoadingDelegateArgs.Errors.NoSourceFiles)
						{



							rootFrame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
							{

								Windows.UI.Popups.MessageDialog dialog = new MessageDialog("Необходимо обновить базу данных")
								{
									Commands =
										{
										new UICommand("Обновить", command =>
										{
											if (model.Context.UpdateDataCommand.CanExecute(null))
												model.Context.UpdateDataCommand.Execute(null);

										})
										}
								};
								await dialog.ShowAsync();

							});
						}
					};
					model.Context.Load();


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
#if BETA
			Logger.Log("Activate window");
			Debug.WriteLine("App.OnLaunched ended");
#endif

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
#if BETA
			Logger.Log("Onsuspending");
			Logger.Log().SaveToFile();
#endif
			var deferral = e.SuspendingOperation.GetDeferral();
			var model = MainModelView.MainModelViewGet;
			await model.Context.Save(saveAllDb: false);
			model.SettingsModelView.TypeError = SettingsModelView.Error.None;
			if (!model.SettingsModelView.KeepTracking)
				model.MapModelView.StopGPS();
			deferral.Complete();
		}



		readonly TimeSpan maxDifTime = new TimeSpan(0, 1, 0, 0);

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

			foreach (var cur in BackgroundTaskRegistration.AllTasks)
			{

				if (cur.Value.Name == taskName)
				{
					// 
					// The task is already registered.
					// 

					return (BackgroundTaskRegistration)(cur.Value);
				}
			}


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

#if BETA
			Logger.Log().WriteLine(Environment.NewLine+Environment.NewLine + Environment.NewLine).WriteLineTime("App");
#endif
			TelemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();

			this.InitializeComponent();
			this.Suspending += this.OnSuspending;

			this.UnhandledException += OnUnhandledException;



			MainModelView.Create(new UniversalContext(new FileHelper()));
#if BETA
			Logger.Log("App ended");
#endif
		}

		async Task SaveToFile(string str)
		{
			var storage = await ApplicationData.Current.LocalFolder.CreateFileAsync("Error.txt");
			await FileIO.AppendTextAsync(storage, str);
		}

		private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
			var settings = MainModelView.MainModelViewGet.SettingsModelView;
			StringBuilder builder = new StringBuilder(DateTime.Now.ToString());
			builder.Append(": ").Append(unhandledExceptionEventArgs.Exception.ToString()).
				AppendLine(unhandledExceptionEventArgs.Message).AppendLine(unhandledExceptionEventArgs.Exception.StackTrace);
			settings.LastUnhandeledException = builder.ToString();
#if BETA
			Logger.Log("App.OnUnhadledException:").WriteLine(unhandledExceptionEventArgs.Exception.ToString())
				.WriteLine(unhandledExceptionEventArgs.Message).WriteLineTime(unhandledExceptionEventArgs.Exception.StackTrace);
			await Logger.Log().SaveToFile();
#endif
			if (settings.TypeError == SettingsModelView.Error.Critical)
				settings.TypeError = SettingsModelView.Error.Repeated;
			else if (settings.TypeError == SettingsModelView.Error.Repeated)
			{
				MainModelView.MainModelViewGet.Context.Recover();
			}
			else
			{
				settings.TypeError = SettingsModelView.Error.Critical;
			}
		}

		#region Overrides of Application

		protected override async void OnActivated(IActivatedEventArgs args)
		{
#if BETA
			Logger.Log("App OnActivated");
#endif
			base.OnActivated(args);
			await MainModelView.MainModelViewGet.Context.Load();
#if BETA
			Logger.Log("App OnActivated, context loaded");
#endif
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

