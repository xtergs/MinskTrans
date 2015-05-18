﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.Messaging;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.ModelView;


// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace MinskTrans.Universal
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public sealed partial class App : Application
	{
		private async void InitNotificationsAsync()
		{
			//var channel = HttpNotificationChannel.Find("MyPushChannel");
			//if (channel == null)
			//{
			//	channel = new HttpNotificationChannel("MyPushChannel");
			//	channel.Open();
			//	channel.BindToShellToast();
			//}

			//channel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(async (o, args) =>
			//{
			//	var hub = new NotificationHub("<hub name>", "<connection string>");
			//	await hub.RegisterNativeAsync(args.ChannelUri.ToString());
			//});

			var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
			var hub = new NotificationHub("MinskTransNotificationBeta", "Endpoint=sb://minsktransnotificationbeta-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=GdFTAoJMnCEI3TFpI4g5Pn0jQy6lk0UEG4UatPHFX8A=");
			
			var result = await hub.RegisterNativeAsync(channel.Uri);

			// Displays the registration ID so you know it was successful
			if (result.RegistrationId != null)
			{
				var dialog = new MessageDialog("Registration successful: " + result.RegistrationId);
				dialog.Commands.Add(new UICommand("OK"));
				Popup popup = new Popup();
				TextBlock text = new TextBlock();
				
				text.FontSize = 20;
				text.Text = "Registration successful: " + result.RegistrationId;
				popup.Child = text;
				popup.IsLightDismissEnabled = true;
				
				popup.IsOpen = true;
				//await dialog.ShowAsync();
			}
		}
		private Timer timer;
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
			Logger.Log("App");
#endif
			this.InitializeComponent();
			this.Suspending += this.OnSuspending;
#if BETA
			this.UnhandledException += OnUnhandledException;
#endif
			MainModelView.Create(new UniversalContext());
		}

		async Task SaveToFile(string str)
		{
			var storage = await ApplicationData.Current.LocalFolder.CreateFileAsync("Error.txt");
			await FileIO.AppendTextAsync(storage, str);
		}

		private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
#if BETA
			Logger.Log("App.OnUnhadledException:").WriteLine(unhandledExceptionEventArgs.Exception.ToString())
				.WriteLine(unhandledExceptionEventArgs.Message);
			await Logger.Log().SaveToFile();
#endif
			var settings = MainModelView.MainModelViewGet.SettingsModelView;
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
			StringBuilder builder = new StringBuilder(unhandledExceptionEventArgs.Exception.ToString());
			builder.Append("\n");
			builder.Append(unhandledExceptionEventArgs.Message);
			builder.Append("\n");
			builder.Append(unhandledExceptionEventArgs.Exception.StackTrace);
			EmailManager.ShowComposeNewEmailAsync(new EmailMessage()
			{
				Subject = "Минский общественный транспорт",
				To =
				{
					new EmailRecipient("xtergs@gmail.com")
				},
				Body = builder.ToString()
			});
		}

		#region Overrides of Application

		protected override void OnActivated(IActivatedEventArgs args)
		{
#if BETA
			Logger.Log("App OnActivated");
#endif
			base.OnActivated(args);
			MainModelView.MainModelViewGet.Context.Load();
		}

		#endregion

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used when the application is launched to open a specific file, to display
		/// search results, and so forth.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		async protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			InitNotificationsAsync();
#if BETA
			Logger.Log("App.OnLaunched started");
#endif
			Debug.WriteLine("App.OnLaunched started");
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
			
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


					timer = new Timer(state =>
					{
#if BETA
						Logger.Log("autoupdate timer elapsed");
#endif
						InternetHelper.UpdateNetworkInformation();
						if ( model.SettingsModelView.HaveConnection())
							if (model.Context.UpdateDataCommand.CanExecute(null))
								model.Context.UpdateAsync();
					}, null, model.SettingsModelView.InvervalAutoUpdateTimeSpan, model.SettingsModelView.InvervalAutoUpdateTimeSpan);

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
#endif

			// Ensure the current window is active
			Window.Current.Activate();
			Debug.WriteLine("App.OnLaunched ended");
		}

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

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
#if BETA
			Logger.Log("Onsuspending");
			Logger.Log().SaveToFile();
#endif
			var deferral = e.SuspendingOperation.GetDeferral();
			var model = MainModelView.MainModelViewGet;
			model.Context.Save();
			model.SettingsModelView.TypeError = SettingsModelView.Error.None;
			if (!model.SettingsModelView.KeepTracking)
				model.MapModelView.StopGPS();
			deferral.Complete();
		}

		
	}
}