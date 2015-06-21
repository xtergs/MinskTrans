using System.Windows;
using System.Windows.Navigation;

namespace PushNotificationServer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void Application_LoadCompleted(object sender, NavigationEventArgs e)
		{
			ServerEngine.Engine.InicializeAsync();
		}

		async private void Application_Startup(object sender, StartupEventArgs e)
		{
			await ServerEngine.Engine.InicializeAsync();
		}

	}
}
