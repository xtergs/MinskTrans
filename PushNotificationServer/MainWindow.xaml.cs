using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using MinskTrans.DesctopClient;

using PushNotificationServer.Properties;

namespace PushNotificationServer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{

		//private ContextDesctop context;
		
		public IContext Context
		{
			get { return ServerEngine.Engine.Context1; }
		}

		private Timer timerNewsAutoUpdate;
		public MainWindow()
		{
			InitializeComponent();

			Browser.Navigate(@"https://login.live.com/oauth20_authorize.srf?pretty=false&client_id=0000000040158EFF&scope=wl.basic+wl.signin+wl.skydrive&response_type=code&redirect_uri=");
			ServerEngine.Engine.Context1.UpdateEnded += (sender, args) => { };
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
				timerNewsAutoUpdate.Change(uint.MaxValue, uint.MaxValue);
			};

			ServerEngine.Engine.StopChecknews += (sender, args) =>
			{
				Dispatcher.Invoke(() =>
				{
					Progress.Visibility = Visibility.Collapsed;
					if (sender is UIElement)
						((UIElement) sender).IsEnabled = true;
					
					
				});
				Time = new TimeSpan();
				timerNewsAutoUpdate.Change(new TimeSpan(), addingTime);
			};

			SetAutoUpdateTimer(NewsAutoUpdate);
		}

		

		

		public bool Autorun
		{
			get
			{
				if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run").GetValue("PushServerAutorun") == null)
					return false;
				return true;
			}
			set
			{
				if (value)
					Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)
					.SetValue("PushServerAutorun", Assembly.GetExecutingAssembly().Location);
				else
				{
					Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true).DeleteValue("PushServerAutorun");
				}
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
			}

		public ServerEngine Engine
		{
			get { return ServerEngine.Engine; }
		}
		public bool NewsAutoUpdate
		{
			get { return Settings.Default.NewsAutoUpdate; }
			set
			{
				Settings.Default.NewsAutoUpdate = value;
				Settings.Default.Save();
				SetAutoUpdateTimer(NewsAutoUpdate);
				OnPropertyChanged();
			}
		}

		public TimeSpan Time
		{
			get { return time; }
			private set
			{
				time = value;
				OnPropertyChanged();
			}
		}

		TimeSpan time = new TimeSpan();
		private readonly static TimeSpan addingTime = new TimeSpan(0, 0, 0, 1);

		public void SetAutoUpdateTimer(bool turnOn)
		{
			timerNewsAutoUpdate = new Timer((key) =>
			{
				Time = time.Add(addingTime);
			}, null, new TimeSpan(), addingTime);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		
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
			//await ServerEngine.Engine.TestOndeDrive();
		}
	}
}
