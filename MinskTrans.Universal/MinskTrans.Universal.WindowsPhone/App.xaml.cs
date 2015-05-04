using System;
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
using MinskTrans.DesctopClient;
using MinskTrans.Universal.ModelView;


// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace MinskTrans.Universal
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public sealed partial class App : Application
	{
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
			this.InitializeComponent();
			this.Suspending += this.OnSuspending;
#if BETA
			this.UnhandledException += OnUnhandledException;
#endif
			MainModelView.Create(new UniversalContext());
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
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
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				// TODO: change this value to a cache size that is appropriate for your application
				rootFrame.CacheSize = 1;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					// TODO: Load state from previously suspended application
					//MainModelView.MainModelViewGet.Context.Load();
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
				if (e.PreviousExecutionState != ApplicationExecutionState.Running)
				{
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
						UpdateNetworkInformation();
						if (Is_Connected && (Is_InternetAvailable || Is_Wifi_Connected == model.SettingsModelView.UpdateOnWiFi))
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
			var deferral = e.SuspendingOperation.GetDeferral();
			var model = MainModelView.MainModelViewGet;
			model.Context.Save();
			if (!model.SettingsModelView.KeepTracking)
				model.MapModelView.StopGPS();
			deferral.Complete();
		}

		public void UpdateNetworkInformation()
		{
			// Get current Internet Connection Profile.
			ConnectionProfile internetConnectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
			Is_Connected = true;
			//air plan mode is on...
			if (internetConnectionProfile == null)
			{
				Is_Connected = false;
				return;
			}

			//if true, internet is accessible.
			this.Is_InternetAvailable = internetConnectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;

			// Check the connection details.
			if (internetConnectionProfile.NetworkAdapter.IanaInterfaceType != 71)// Connection is not a Wi-Fi connection. 
			{
				Is_Roaming = internetConnectionProfile.GetConnectionCost().Roaming;

				/// user is Low on Data package only send low data.
				Is_LowOnData = internetConnectionProfile.GetConnectionCost().ApproachingDataLimit;

				//User is over limit do not send data
				Is_OverDataLimit = internetConnectionProfile.GetConnectionCost().OverDataLimit;

			}
			else //Connection is a Wi-Fi connection. Data restrictions are not necessary. 
			{
				Is_Wifi_Connected = true;
			}
		}

		public bool Is_Wifi_Connected { get; private set; }

		public bool Is_OverDataLimit { get; private set; }

		public bool Is_LowOnData { get; private set; }

		public bool Is_Roaming { get; private set; }

		public bool Is_InternetAvailable { get; private set; }

		public bool Is_Connected { get; private set; }
	}
}