using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MinskTrans.DesctopClient.Modelview;

using MapControl;
using MapControl.Caching;
using MinskTrans.DesctopClient.Annotations;


namespace MinskTrans.DesctopClient
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private readonly System.Timers.Timer timerr;
		private Timer timer;
		private List<Pushpin> pushpins;

		public bool IsShowBusStops
		{
			get { return isShowBusStops; }
			set
			{
				if (value.Equals(isShowBusStops)) return;
				isShowBusStops = value;
				OnPropertyChanged();
			}
		}

		public bool IsShowTrolStops
		{
			get { return isShowTrolStops; }
			set
			{
				if (value.Equals(isShowTrolStops)) return;
				isShowTrolStops = value;
				OnPropertyChanged();
			}
		}

		public bool IsShowTramStops
		{
			get { return isShowTramStops; }
			set
			{
				if (value.Equals(isShowTramStops)) return;
				isShowTramStops = value;
				OnPropertyChanged();
			}
		}

		private TimeSpan secs = new TimeSpan(0,1,0,0,0);
		private System.Timers.Timer checkUpdateTimer;

		public MainWindow()
		{
			ShedulerModelView = new MainModelView(new ContextDesctop());
			//BingMapsTileLayer.ApiKey = @"AixwFJQ_Vb2iTTrQjI__HkjjnECoGsCDRAR9pyA2Tz0ZqP1l4SyOZoSlwsVv-pXS";
			DispatcherTimer tm = new DispatcherTimer();
			tm.Interval = new TimeSpan(0, 0, 1);
			tm.Tick += (sender, args) =>
			{
				if (secs == new TimeSpan(0, 0, 0))
				{
					secs = new TimeSpan(0, 1, 0, 0, 0);
					if (ShedulerModelView.Context.UpdateDataCommand.CanExecute(null))
						ShedulerModelView.Context.UpdateDataCommand.Execute(null);
				}
				secs = secs.Subtract(new TimeSpan(0, 0, 1));
				tickTimer.Content = secs;
			};
			tm.Start();
			checkUpdateTimer = new System.Timers.Timer();
			checkUpdateTimer.Interval = 1000*60*60;
			checkUpdateTimer.Enabled = true;
			checkUpdateTimer.Start();
			InitializeComponent();
			ShedulerModelView.RoutesModelview.ShowRoute += OnShowRoute;
			ShedulerModelView.RoutesModelview.ShowStop += OnShowStop;
			ShedulerModelView.StopMovelView.ShowStop += OnShowStop;

			ShedulerModelView.Context.DataBaseDownloadStarted += (sender, args) => statusMessages.Dispatcher.Invoke(()=>
			{
				statusMessages.Content = "Data had started downloading";
			});
			ShedulerModelView.Context.DataBaseDownloadEnded += (sender, args) => statusMessages.Dispatcher.Invoke(() =>
			{
				statusMessages.Content = "Data has been downloaded";
			});

			ShedulerModelView.Context.ErrorDownloading += (sender, args) => MessageBox.Show("Error to download");

			//var stop = ShedulerModelView.Context.Stops.First(x => x.SearchName == "шепичи");
			ShedulerModelView.Context.UpdateEnded += (sender, args) =>
			{
				map.Center = new Location(53, 27);
				pushpins = new List<Pushpin>();
				if (ShedulerModelView.Context.ActualStops != null)
					foreach (var st in ShedulerModelView.Context.ActualStops)
					{
						var pushpin = new Pushpin();
						pushpin.Tag = st;
						pushpin.Style = (Style) Resources["PushpinStyle1"];
						//pushpin.templ
						pushpin.Content = st.ID;
						pushpin.MouseMove += (senderr, argsr) =>
						{
							((Pushpin) senderr).BringToFront();
						};
						pushpin.MouseDown += (o, argsr) =>
						{
							Pushpin tempPushpin = (Pushpin) o;
							Stop tmStop = (Stop) tempPushpin.Tag;
							ShedulerModelView.StopMovelView.FilteredSelectedStop = tmStop;
							stopTabItem.Focus();
						};
						MapPanel.SetLocation(pushpin, new Location(st.Lat, st.Lng));
						pushpins.Add(pushpin);
						map.Children.Add(pushpin);

					}
				//map.
				//MapControl.Caching.ImageFileCache
				map.ZoomLevel = 19;
			};
			//map.CredentialsProvider = new ApplicationIdCredentialsProvider(@"AoQ8eu3GasAHHCCsUjs25t6Os80fC_sx4wXi_tzY9hKwRV8U-lTkC5AcQzhFL9uk");
			
			
			DataContext = ShedulerModelView;
			//timer = new Timer((x)=>{});
			
			timerr = new System.Timers.Timer(10000);
			timerr.Elapsed += (sender, args) => ShedulerModelView.StopMovelView.RefreshTimeSchedule.Execute(null);
			timerr.Start();

			//ShedulerModelView.Context.Save();
			//ShedulerModelView.Context.Load();
		}

		private void OnShowStop(object sender, ShowArgs args)
		{
			pushpinsAll = true;
			mapTabItem.Focus();
			var temp = args.SelectedStop;
			map.Center = new Location(temp.Lat, temp.Lng);
			map.ZoomLevel = 19;
		}

		private void OnShowRoute(object sender, ShowArgs args)
		{
			pushpinsAll = false;
			mapTabItem.Focus();
			foreach (var child in pushpins)
			{
				child.Visibility = Visibility.Collapsed;
			}
			var tempRoute = args.SelectedRoute;
			foreach (var child in pushpins.Where(x=>tempRoute.Stops.Contains((Stop)x.Tag)))
			{
				child.Visibility = Visibility.Visible;
			}
			map.Center = new Location(tempRoute.Stops[0].Lat, tempRoute.Stops[0].Lng);
			map.ZoomLevel = 15;
		}

		private MainModelView ShedulerModelView { get; set; }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (ShedulerModelView.Context.UpdateDataCommand.CanExecute(null))
				ShedulerModelView.Context.UpdateDataCommand.Execute(null);
		}

		private int w(char x, char y)
		{
			if (x == y)
				return 0;

			return 1;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string y = text1.Text;
			string x = text2.Text;
			var a = new int[x.Length][];
			for (int i = 0; i < a.Length; i++)
				a[i] = new int[y.Length + 1];

			for (int j = 0; j < a[0].Length; j++)
				a[0][j] = 0;

			for (int i = 1; i < a.Length; i++)
				a[i][0] = a[i - 1][0] + 1;

			for (int i = 1; i < a.Length; i++)
				for (int j = 1; j < a[0].Length; j++)
					a[i][j] = Math.Min(Math.Min(a[i - 1][j] + 1, a[i][j - 1] + 1), a[i - 1][j - 1] + w(y[j - 1], x[i - 1]));
			result.Text = "";
			for (int i = 0; i < a.Length; i++)
			{
				for (int j = 0; j < a[i].Length; j++)
					result.Text += a[i][j].ToString("D2") + " ";
				result.Text += "\n";
			}
			int str = 23;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			ListView.ItemsSource = ShedulerModelView.StopMovelView.Context.Stops.Where(
				x => Comparer(x.SearchName, ShedulerModelView.StopMovelView.StopNameFilter.ToLower())).Distinct();
		}


		private bool Comparer(string x, string y)
		{
			var a = new int[x.Length][];
			for (int i = 0; i < a.Length; i++)
				a[i] = new int[y.Length + 1];

			for (int j = 0; j < a[0].Length; j++)
				a[0][j] = 0;

			for (int i = 1; i < a.Length; i++)
				a[i][0] = a[i - 1][0] + 1;

			for (int i = 1; i < a.Length; i++)
				for (int j = 1; j < a[0].Length; j++)
					a[i][j] = Math.Min(Math.Min(a[i - 1][j] + 1, a[i][j - 1] + 1), a[i - 1][j - 1] + w(y[j - 1], x[i - 1]));
			result.Text = "";

			for (int j = a[a.Length - 1].Length - 1; j >= 0; j--)
				if (a[a.Length - 1][j] <= 2)
					return true;

			return false;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			ShedulerModelView.Context.Save();
			timerr.Stop();
			timerr.Dispose();
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			
		}

		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			
		}

		private void routeFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			routeNumsListView.SelectedIndex = 0;
		}

		private void routeNumsListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			routeNamesListView.SelectedIndex = 0;
		}

		private void routeNamesListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			stopsListView.SelectedIndex = 0;
		}

		private void BusButton_Checked(object sender, RoutedEventArgs e)
		{
			if (BusButton.IsChecked == true)
				return;
			TrolButton.IsChecked = false;
			TramButton.IsChecked = false;
		}

		private void TrolButton_Checked(object sender, RoutedEventArgs e)
		{
			if (TrolButton.IsChecked == true)
				return;
			BusButton.IsChecked = false;
			TramButton.IsChecked = false;
		}

		private void TramButton_Checked(object sender, RoutedEventArgs e)
		{
			if (TramButton.IsChecked == true)
				return;
			TrolButton.IsChecked = false;
			BusButton.IsChecked = false;
		}

		private bool pushpinsAll = true;
		private bool isShowBusStops;
		private bool isShowTrolStops;
		private bool isShowTramStops;

		public void RefreshPushPins()
		{
			if (pushpins == null)
				return;
			if (pushpinsAll)
				foreach (var child in pushpins)
				{
					if (map.ZoomLevel <= 13)
					{
						child.Visibility = Visibility.Collapsed;
					}
					else
					{
						child.Visibility = Visibility.Visible;
					}



				}
		}

		private void map_ViewportChanged(object sender, EventArgs e)
		{
			RefreshPushPins();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Button_Click_4(object sender, RoutedEventArgs e)
		{
			var window = new GroupAddWindow(ShedulerModelView.Context);
			var result = window.ShowDialog();
			if (result.Value)
			{
				if (ShedulerModelView.GroupStopsModelView.AddGroup.CanExecute(window.Group))
					ShedulerModelView.GroupStopsModelView.AddGroup.Execute(window.Group);
			}
			window = null;
		}

		private void Button_Click_5(object sender, RoutedEventArgs e)
		{
			var window = new GroupAddWindow(ShedulerModelView.Context);
			window.Group = ShedulerModelView.GroupStopsModelView.SelectedGroupStop;
			var result = window.ShowDialog();
			if (result.Value)
			{
				if (ShedulerModelView.GroupStopsModelView.RemoveGorup.CanExecute(ShedulerModelView.GroupStopsModelView.SelectedGroupStop))
					if (ShedulerModelView.GroupStopsModelView.AddGroup.CanExecute(window.Group))
					{
						ShedulerModelView.GroupStopsModelView.RemoveGorup.Execute(ShedulerModelView.GroupStopsModelView.SelectedGroupStop);
						ShedulerModelView.GroupStopsModelView.AddGroup.Execute(window.Group);
					}
			}
			window = null;
		}

		private void Button_Click_6(object sender, RoutedEventArgs e)
		{
			if (ShedulerModelView.GroupStopsModelView.RemoveGorup.CanExecute(ShedulerModelView.GroupStopsModelView.SelectedGroupStop))
				ShedulerModelView.GroupStopsModelView.RemoveGorup.Execute(ShedulerModelView.GroupStopsModelView.SelectedGroupStop);
		}

		private void Button_Click_7(object sender, RoutedEventArgs e)
		{
			CalculateRout calculate = new CalculateRout(ShedulerModelView.Context);
			calculate.CreateGraph();
			calculate.FindPath(ShedulerModelView.Context.ActualStops.First(x => x.ID == 15757), ShedulerModelView.Context.ActualStops.First(x => x.ID == 15754));
		}
	}
}