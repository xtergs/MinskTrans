using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Messaging;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Annotations;
using PushNotificationServer.Properties;
using Microsoft.Azure.NotificationHubs;

namespace PushNotificationServer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{

		private ContextDesctop context;
		private string fileNameLastNews = "LastNews.txt";
		public ContextDesctop Context
		{
			get { return context; }
		}

		private Timer timerNewsAutoUpdate;
		public MainWindow()
		{
			InitializeComponent();
			context = new ContextDesctop();
			context.UpdateEnded += (sender, args) => { };
			InicializeSettings();

			ServerEngine.Engine.Inicialize();
			NewsTextBlock.DataContext = ServerEngine.Engine.NewsManager;
			HotNewsTextBlock.DataContext = ServerEngine.Engine.NewsManager;
			AutoUpdateNewsCheckBox.DataContext = ServerEngine.Engine;
			this.DataContext = ServerEngine.Engine;
			MainGrid.DataContext = ServerEngine.Engine;

			ServerEngine.Engine.StartCheckNews += (sender, args) =>
			{
				Dispatcher.Invoke(() =>
				{
					if (sender is UIElement)
						((UIElement)sender).IsEnabled = false;
					Progress.Visibility = Visibility.Visible;
					Progress.IsIndeterminate = true;
				});
			};

			ServerEngine.Engine.StopChecknews += (sender, args) =>
			{
				Dispatcher.Invoke(() =>
				{
					Progress.Visibility = Visibility.Collapsed;
					if (sender is UIElement)
						((UIElement) sender).IsEnabled = true;
					SaveTime();
				});
			};

			SetAutoUpdateTimer(NewsAutoUpdate);
		}

		void SaveTime()
		{
			File.WriteAllText(fileNameLastNews,
				Properties.Settings.Default.LastUpdatedNews + Environment.NewLine + Properties.Settings.Default.LastUpdatedHotNews +
				Environment.NewLine + Context.LastUpdateDataDateTime);
		}

		void InicializeSettings()
		{
			
			updateTimer = new Timer(async (x) =>
			{
				await context.UpdateAsync();
				SaveTime();
			}, null, new TimeSpan(0), new TimeSpan(0,CheckEveryMin,0) );
		}

		public bool Autorun
		{
			get
			{
				if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run").GetValue("") == null)
					return false;
				return true;
			}
			set
			{
				Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)
					.SetValue("PushServerAutorun", System.Reflection.Assembly.GetExecutingAssembly().Location);
				OnPropertyChanged();
			}
		}

		public int CheckEveryMin
		{
			get { return Settings.Default.CheckUpdateEveryMins; }
			set
			{
				if (value < 0)
					return;
				Settings.Default.CheckUpdateEveryMins = value;
				OnPropertyChanged();
			}
		}

		private Timer updateTimer;
		private static async void SendNotificationAsync()
		{
			// Define the notification hub.
			NotificationHubClient hub =
				NotificationHubClient.CreateClientFromConnectionString(
					"Endpoint=sb://minsktransbetapushnotificationhub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=9w3Mok3fk9gSUT5Ib1avqBRvPykzz0baiUD8YNNcDRI=", "MinskTransNotificationBeta", true);
			
			// Create an array of breaking news categories.
			var categories = new string[] { "World", "Politics", "Business", 
        "Technology", "Science", "Sports"};

			foreach (var category in categories)
			{
				try
				{
					// Define a Windows Store toast.
					//var wnsToast = "<toast><visual><binding template=\"ToastText01\">"
					//	+ "<text id=\"1\">Breaking " + category + " News!"
					//	+ "</text></binding></visual></toast>";
					//await hub.SendWindowsNativeNotificationAsync(wnsToast, category);

					// Define a Windows Phone toast.
					var mpnsToast =
						"<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
						"<wp:Notification xmlns:wp=\"WPNotification\">" +
							"<wp:Toast>" + 
								"<wp:Text1>Breaking " + category + " News!</wp:Text1>" +
							"</wp:Toast> " +
						"</wp:Notification>";
					await hub.SendMpnsNativeNotificationAsync(mpnsToast, category);

					
				}
				catch (ArgumentException)
				{
					// An exception is raised when the notification hub hasn't been 
					// registered for the iOS, Windows Store, or Windows Phone platform. 
				}
			}
		}

		private async void SendRawPushNotification(object sender, RoutedEventArgs e)
		{
			SendNotificationAsync();
			//PushNotificationChannel channel = null;

			//try
			//{
			//	channel = await  PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
			//}

			//catch (Exception ex)
			//{
			//	// Could not create a channel. 
			//}

			//HttpWebRequest sendNotificationRequest = HttpWebRequest.CreateHttp(channel.Uri);
			//sendNotificationRequest.Method = "POST";
			//sendNotificationRequest.ContentType = "text/xml";
			//sendNotificationRequest.Headers.Add("X-NotificationClass", "[batching interval]");
			//var responce = sendNotificationRequest.GetResponse();

		}

		public ServerEngine Engine
		{
			get { return ServerEngine.Engine; }
		}
		public bool NewsAutoUpdate
		{
			get { return Properties.Settings.Default.NewsAutoUpdate; }
			set
			{
				Properties.Settings.Default.NewsAutoUpdate = value;
				Properties.Settings.Default.Save();
				SetAutoUpdateTimer(NewsAutoUpdate);
				OnPropertyChanged();
			}
		}

		public void SetAutoUpdateTimer(bool turnOn)
		{
			if (turnOn)
			{
				if (timerNewsAutoUpdate == null)
					timerNewsAutoUpdate = new Timer(UpdateNews, UpdateNewsButton, new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 1, 0, 0));
				else
					timerNewsAutoUpdate.Change(new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 1, 0, 0));
			}
			else
			{
				timerNewsAutoUpdate.Dispose();
				timerNewsAutoUpdate = null;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		async void UpdateNews(object sender)
		{
			try
			{
				await ServerEngine.Engine.CheckNews();
			}
			catch (TaskCanceledException e)
			{

			}

		}

		async private void CheckNewsButtonClick(object sender, RoutedEventArgs e)
		{
			UpdateNews(sender);
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await ServerEngine.Engine.TestOndeDrive();
		}
	}
}
