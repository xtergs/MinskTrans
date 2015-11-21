using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MapControl;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient.Annotations;
using MinskTrans.DesctopClient.Modelview;

namespace MinskTrans.DesctopClient
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private readonly Timer timerr;

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

		private TimeSpan updateTimerInterval = new TimeSpan(0, 1, 0, 0, 0);
		private Timer checkUpdateTimer;
		DispatcherTimer updateTime = new DispatcherTimer();
		

		public MainWindow()
		{
			InitializeComponent();
			//TileImageLoader.Cache = new FileDbCache("map", "cache");
			MapModelView.StylePushpin = (Style) Resources["PushpinStyle1"];
			ShedulerModelView = new MainModelView(map);
			ShedulerModelView.Context.LoadEnded += (sender, args) => { Dispatcher.Invoke(() => { ShedulerModelView.Context.Context.AllPropertiesChanged(); }); };
			ShedulerModelView.Context.LoadDataBase();

			//Map model view events
			ShedulerModelView.MapModelView.StartStopSeted +=
				(sender, args) => ShedulerModelView.MapModelView.StartStopPushpin.Style = (Style) Resources["PushpinStyleSelected"];
			ShedulerModelView.MapModelView.EndStopSeted +=
				(sender, args) => ShedulerModelView.MapModelView.EndStopPushpin.Style = (Style) Resources["PushpinStyleSelected"];
			ShedulerModelView.MapModelView.MapInicialized += (sender, args) =>
			{
				foreach (var pushpin in ShedulerModelView.MapModelView.Pushpins)
				{
					pushpin.MouseLeftButtonDown += (o, argsr) =>
					{
						Pushpin tempPushpin = (Pushpin) o;
						Stop tmStop = (Stop) tempPushpin.Tag;
						ShedulerModelView.StopMovelView.FilteredSelectedStop = tmStop;
						stopTabItem.Focus();
					};
					//pushpin.MouseRightButtonDown += (o, eventArgs) =>
					//{
					//	Pushpin tempPushpin = (Pushpin)o;
					//	tempPushpin.ContextMenu.IsOpen = true;
					//	currentPushpin = (Pushpin)o;
					//};
				}
			};

			//BingMapsTileLayer.ApiKey = @"AixwFJQ_Vb2iTTrQjI__HkjjnECoGsCDRAR9pyA2Tz0ZqP1l4SyOZoSlwsVv-pXS";
			
			updateTime.Interval = new TimeSpan(0, 0, 1);
			updateTime.Tick += (sender, args) =>
			{
				if (updateTimerInterval == new TimeSpan(0, 0, 0))
				{
					ResetUpdateTimer();
					if (ShedulerModelView.UpdateDataCommand.CanExecute(null))
						ShedulerModelView.UpdateDataCommand.Execute(null);
				}
				updateTimerInterval = updateTimerInterval.Subtract(new TimeSpan(0, 0, 1));
				tickTimer.Content = updateTimerInterval;
			};
			updateTime.Start();
			
			ShedulerModelView.ShowRoute += OnShowRoute;
			ShedulerModelView.ShowStop += OnShowStop;
			ShedulerModelView.ShowStop += OnShowStop;

			ShedulerModelView.UpdateManager.DataBaseDownloadStarted += (sender, args) => statusMessages.Dispatcher.Invoke(() =>
			{
				statusMessages.Content = "Data had started downloading";
			});
			ShedulerModelView.UpdateManager.DataBaseDownloadEnded += (sender, args) => statusMessages.Dispatcher.Invoke(() =>
			{
				statusMessages.Content = "Data has been downloaded";
			});
			ShedulerModelView.Context.LoadStarted += (sender, args) => StopUpdateTimer();
			ShedulerModelView.Context.LoadEnded += (sender, args) => StartUpdateTimer();
			//ShedulerModelView.Context.ErrorLoading += (sender, args)=> StartUpdateTimer();

			ShedulerModelView.UpdateManager.ErrorDownloading += (sender, args) => MessageBox.Show("Error to download");
			ShedulerModelView.Context.Context.Load();
			//var stop = ShedulerModelView.IContext.Stops.First(x => x.SearchName == "шепичи");

			//map.CredentialsProvider = new ApplicationIdCredentialsProvider(@"AoQ8eu3GasAHHCCsUjs25t6Os80fC_sx4wXi_tzY9hKwRV8U-lTkC5AcQzhFL9uk");


			DataContext = ShedulerModelView;
			//timer = new Timer((x)=>{});

			timerr = new Timer(10000);
			timerr.Elapsed += (sender, args) => ShedulerModelView.StopMovelView.RefreshTimeSchedule.Execute(null);
			timerr.Start();

			//ShedulerModelView.IContext.Save();
			//ShedulerModelView.IContext.Load();
		}

		void StopUpdateTimer()
		{
			updateTime.Stop();
		}

		private void ResetUpdateTimer()
		{
			updateTimerInterval = new TimeSpan(0, 1, 0, 0, 0);
		}

		void StartUpdateTimer()
		{
			ResetUpdateTimer();
			updateTime.Start();
		}
		private Pushpin currentPushpin { get; set; }

		private void OnShowStop(object sender, ShowArgs args)
		{
			mapTabItem.Focus();
			ShedulerModelView.MapModelView.ShowStopCommand.Execute(args.SelectedStop);
		}

		private void OnShowRoute(object sender, ShowArgs args)
		{
			mapTabItem.Focus();
			ShedulerModelView.MapModelView.ShowRoutCommand.Execute(args.SelectedRoute);
		}

		private MainModelView ShedulerModelView { get; }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//if (ShedulerModelView.IContext.UpdateDataCommand.CanExecute(null))
			//	ShedulerModelView.IContext.UpdateDataCommand.Execute(null);
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
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			ListView.ItemsSource = ShedulerModelView.StopMovelView.Context.Context.Stops.Where(
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

		private async void Window_Closed(object sender, EventArgs e)
		{
			await ShedulerModelView.Context.Save();
			timerr.Stop();
			timerr.Dispose();
		}

		private void routeFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			routeNumsListView.SelectedIndex = 0;
		}

		private void routeNumsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			routeNamesListView.SelectedIndex = 0;
		}

		private void routeNamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

		private bool isShowBusStops;
		private bool isShowTrolStops;
		private bool isShowTramStops;

		private void map_ViewportChanged(object sender, EventArgs e)
		{
			//RefreshPushPins();
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
			var window = new GroupAddWindow(ShedulerModelView.Context, ShedulerModelView.SettingsModelView);
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
			var window = new GroupAddWindow(ShedulerModelView.Context, ShedulerModelView.SettingsModelView);
			window.Group = ShedulerModelView.GroupStopsModelView.SelectedGroupStop;
			var result = window.ShowDialog();
			if (result.Value)
			{
				if (
					ShedulerModelView.GroupStopsModelView.RemoveGorup.CanExecute(
						ShedulerModelView.GroupStopsModelView.SelectedGroupStop))
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
			if (
				ShedulerModelView.GroupStopsModelView.RemoveGorup.CanExecute(ShedulerModelView.GroupStopsModelView.SelectedGroupStop))
				ShedulerModelView.GroupStopsModelView.RemoveGorup.Execute(ShedulerModelView.GroupStopsModelView.SelectedGroupStop);
		}

		private void Button_Click_7(object sender, RoutedEventArgs e)
		{
			//CalculateRout calculate = new CalculateRout(ShedulerModelView.Context);
			//calculate.CreateGraph();
			//calculate.FindPath(ShedulerModelView.Context.ActualStops.First(x => x.ID == 15757),
			//	ShedulerModelView.Context.ActualStops.First(x => x.ID == 15754));
		}

		private void Button_Click_8(object sender, RoutedEventArgs e)
		{
			//CalculateRout calculaterout = new CalculateRout(ShedulerModelView.Context);
			//calculaterout.CreateGraph();
			//calculaterout.FindPath(ShedulerModelView.Context.ActualStops.First(stop => stop.ID == 15757),
			//	ShedulerModelView.Context.ActualStops.First(stop => stop.ID == 15628));
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			//CalculateRout calculator = new CalculateRout(ShedulerModelView.Context);
			//string ResultString;
			//calculator.CreateGraph();
			//List<string> reusltList = new List<string>();
			//if (!calculator.FindPath(ShedulerModelView.MapModelView.StartStop, ShedulerModelView.MapModelView.EndStop))
			//	reusltList.Add("Bad");
			//else
			//{
			//	StringBuilder builder = new StringBuilder();
			//	foreach (var keyValuePair in calculator.resultRout)
			//	{
			//		builder.Append(keyValuePair.Key.Transport);
			//		builder.Append(" ");
			//		builder.Append(keyValuePair.Key);
			//		builder.Append('\n');
			//		foreach (var stop in keyValuePair.Value)
			//		{
			//			builder.Append(stop.Name);
			//			builder.Append(", ");
			//		}
			//		reusltList.Add(builder.ToString());
			//		builder.Clear();

			//	}
			//	ResultString = builder.ToString();
			//}
			//ResulTextBox.ItemsSource = reusltList;
			//if (oldStops != null)
			//	;
			//oldStops = calculator.resultRout.SelectMany(x => x.Value).Distinct().ToList();
			//MainModelView.Get().MapModelView.MarkPushPins(oldStops, (Style)Resources["PushpinStyleMarket"]);
		}

		private List<Stop> oldStops;


	}
}