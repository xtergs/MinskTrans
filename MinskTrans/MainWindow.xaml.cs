using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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




		public MainWindow()
		{
			ShedulerModelView = new MainModelView(new ContextDesctop());
			BingMapsTileLayer.ApiKey = @"AixwFJQ_Vb2iTTrQjI__HkjjnECoGsCDRAR9pyA2Tz0ZqP1l4SyOZoSlwsVv-pXS";
			InitializeComponent();
			ShedulerModelView.RoutesModelview.ShowRoute += OnShowRoute;
			ShedulerModelView.RoutesModelview.ShowStop += OnShowStop;
			ShedulerModelView.StopMovelView.ShowStop += OnShowStop;

			var stop = ShedulerModelView.context.Stops.First(x => x.SearchName == "шепичи");
			
			map.Center = new Location(stop.Lat, stop.Lng);
			pushpins = new List<Pushpin>();
			foreach (var st in ShedulerModelView.context.ActualStops)
			{
				var pushpin = new Pushpin();
				pushpin.Tag = st;
				pushpin.Style = (Style)Resources["PushpinStyle1"];
				//pushpin.templ
				pushpin.Content = st.Name;
				pushpin.MouseMove += (sender, args) =>
				{
					((Pushpin)sender).BringToFront();
				};
				pushpin.MouseDown += (o, args) =>
				{
					Pushpin tempPushpin = (Pushpin)o;
					Stop tmStop = (Stop)tempPushpin.Tag;
					ShedulerModelView.StopMovelView.FilteredSelectedStop = tmStop;
					stopTabItem.Focus();
				};
				MapPanel.SetLocation(pushpin, new Location(st.Lat, st.Lng));
				pushpins.Add(pushpin);
				map.Children.Add(pushpin);

			}
			map.Children.Add(new MapPolyline()
			{
				Locations =
					ShedulerModelView.context.Stops.Where(x => x.SearchName == "шепичи").Select(x => new Location(x.Lat, x.Lng)),
				StrokeThickness = 10
			});
			map.ZoomLevel = 19;
			
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
	}
}