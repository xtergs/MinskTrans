using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.Networking.PushNotifications;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Messaging;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Annotations;
using PushNotificationServer.Properties;

namespace PushNotificationServer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{

		private ContextDesctop context;
		public MainWindow()
		{
			InitializeComponent();
			context = new ContextDesctop();
			context.UpdateEnded += (sender, args) => { };
			InicializeSettings();
		}

		void InicializeSettings()
		{
			
			updateTimer = new Timer((x)=> context.UpdateAsync(), null, new TimeSpan(0), new TimeSpan(0,CheckEveryMin,0) );
		}

		public bool Autorun
		{
			get
			{
				if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run").GetValue("") == null)
					return false;
				return true;
			}
			set
			{
				Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)
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


		private async void SendRawPushNotification(object sender, RoutedEventArgs e)
		{
			PushNotificationChannel channel = null;

			try
			{
				channel = await  PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
			}

			catch (Exception ex)
			{
				// Could not create a channel. 
			}

			HttpWebRequest sendNotificationRequest = HttpWebRequest.CreateHttp(channel.Uri);
			sendNotificationRequest.Method = "POST";
			sendNotificationRequest.ContentType = "text/xml";
			sendNotificationRequest.Headers.Add("X-NotificationClass", "[batching interval]");
			var responce = sendNotificationRequest.GetResponse();
			
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
