using System.Windows;
using System.Windows.Navigation;
using Microsoft.HockeyApp;
using PushNotificationServer.Helper;
using UniversalMinskTransRelease.Helpers;

namespace PushNotificationServer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	    public App()
	    {
            Microsoft.HockeyApp.HockeyClient.Current.Configure(AppServerConstants.HockeyAppId);
            Application.Current.DispatcherUnhandledException += (sender, args) =>
	        {
	            MessageBox.Show(args.Exception.Message + "\n" + args.Exception.StackTrace);
	        };

	    }
		private void Application_LoadCompleted(object sender, NavigationEventArgs e)
		{
			//ServerEngine.Engine.InicializeAsync();
		}

		async private void Application_Startup(object sender, StartupEventArgs e)
		{
			await ServerEngine.Engine.InicializeAsync();
		}

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
    }
}
