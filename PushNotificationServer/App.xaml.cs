using System.Windows;
using System.Windows.Navigation;
using Microsoft.Build.Utilities;
using Microsoft.HockeyApp;
using UniversalMinskTransRelease.Helpers;
using Task = System.Threading.Tasks.Task;

namespace PushNotificationServer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	    public App()
	    {
            HockeyClient.Current.Configure(AppServerConstants.HockeyAppId);
            Application.Current.DispatcherUnhandledException += (sender, args) =>
            {
                //HockeyClient.Current.TrackException(args.Exception);
                //MessageBox.Show(args.Exception.Message + "\n" + args.Exception.StackTrace);
	        };

	    }
		private void Application_LoadCompleted(object sender, NavigationEventArgs e)
		{
			//ServerEngine.Engine.InicializeAsync();
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
            Task.WhenAll(ServerEngine.Engine.InicializeAsync(),
		        HockeyClient.Current.SendCrashesAsync());
		}

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //HockeyClient.Current.TrackException(e.Exception);
            //MessageBox.Show(e.Exception.Message);
        }
    }
}
