using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Email;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using GalaSoft.MvvmLight.Command;
using MapControl;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.Annotations;
using MinskTrans.Universal.Model;
using MinskTrans.Universal.ModelView;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MinskTrans.Universal
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page, INotifyPropertyChanged
	{
		private MainModelView model;

		enum States
		{
			Stops,
			Routs,
			StopView,
			RoutList,
			RoutView
		}
		private List<PushpinLocation> pushpins;
		States state = States.Stops;

		

		public MainPage()
		{
			this.InitializeComponent();
			//TileImageLoader.Cache = new MapControl.Caching.ImageFileCache();
			//TileImageLoader.DefaultCacheExpiration = new TimeSpan(10, 0, 0, 0,0);
			

			//model = MainModelView.Create(new UniversalContext());
			model = MainModelView.MainModelViewGet;
			//MapModelView.StylePushpin = (Style) App.Current.Resources["PushpinStyle1"];
			model.Context.ShowRoute += OnShowRoute;
			model.Context.ShowStop += OnShowStop;

			

			VisualStateGroup.CurrentStateChanged += (sender, args) =>
			{
				if (args.NewState == ShowStopVisualState)
				{
					StopsListView.SelectedIndex = -1;
				} else if (args.NewState == RoutsListVisualState)
					RoutsListView.SelectedIndex = -1;
				else if (args.NewState == ShowRoutVisualState)
					ShowRoutsListView.SelectedIndex = -1;
				
			};

			FavouriteVisualStateGroup.CurrentStateChanged += (sender, args) =>
			{
				if (args.NewState == FavouriteShowStopVisualState)
					FavouriteStopsListView.SelectedIndex = -1;
				else if (args.NewState == FavouriteShowRoutVisualState)
					ShowFavouriteRoutsListView.SelectedIndex = -1;
				else if (args.NewState == FavouriteRoutsListVisualState)
					FavouriteRoutsListView.SelectedIndex = -1;
			};

			GroupsVisualStateGroup.CurrentStateChanged += (sender, args) =>
			{
				if (args.NewState == ListGroupsVisualState || args.NewState == SelectToDeleteVisualState)
					GroupsListView.SelectedIndex = -1;
			};
			
			//model.ShowStop += OnShowStop;

			

			this.NavigationCacheMode = NavigationCacheMode.Required;
			//model.Context.Save();
			//model.Context.Load();

			model.Context.ErrorDownloading += (sender, args) =>
			{
				ProgressBar.Visibility = Visibility.Collapsed;
				//ProgressBar.IsIndeterminate = false;
			};
			model.Context.UpdateStarted += (sender, args) =>
			{
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = true;
			};
			model.Context.UpdateEnded += async (senderr, args) =>
			{
				//FlyoutBase.GetAttachedFlyout(GgGrid).Hide();
				
				//listBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add("Data downloaded"));
				
				
				//DataContext = model;
#if _DEBUG
				model.Context.FavouriteStops.Add(model.Context.ActualStops.First(x => x.SearchName.Contains("шепичи")));
				model.Context.FavouriteRouts.Add(new RoutWithDestinations(model.Context.Routs.First(x=>x.RouteNum.Contains("20")), model.Context));
				model.Context.AllPropertiesChanged();
				string str = "s";
#endif
				ProgressBar.Visibility = Visibility.Collapsed;
				//ProgressBar.IsIndeterminate = false;
				pushpins = null;
				//Dispatcher.RunAsync(CoreDispatcherPriority.Normal, InicializeMap);
				//model.MapModelView.Inicialize();
				//Flyout.Show();
				//ProgressRing.Visibility = Visibility.Collapsed;

			};
			//model.Context.LogMessage += (o, args) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add(args.Message));
			model.Context.LoadStarted += (sender, args) =>
			{
				ProgressBar.IsIndeterminate = true;
				ProgressBar.Visibility = Visibility.Visible;
			};
			model.Context.LoadEnded += (sender, args) =>
			{
				//ProgressBar.IsIndeterminate = false;
				ProgressBar.Visibility = Visibility.Collapsed;
				//Dispatcher.RunAsync(CoreDispatcherPriority.Normal, InicializeMap);
				//model.MapModelView.Inicialize();
			};
			
			ShowFavouriteStop.AddGroup += ShowAddGroup;
			ShowStop.AddGroup += ShowAddGroup;

			//model.MapModelView = new MapModelView(model.Context, map);

			DataContext = model;


			
		}


		private void ShowAddGroup(object sender, EventArgs args)
		{
			Pivot.SelectedItem = GroupsPivtoItem;
		}

		private MapPanel mappanel;



		public void InicializeMap()
		{
			if (model != null && model.Context.ActualStops != null)
			{
				pushpins = new List<PushpinLocation>(model.Context.ActualStops.Count);
				foreach (var st in model.Context.ActualStops)
				{
					var tempPushPin = new PushpinLocation(){Location = new Location(st.Lat, st.Lng)};
					//var pushpin = new Pushpin { Tag = st, Content = st.Name };
					tempPushPin.Style = (Style)mainPage.Resources["PushpinStyle1"];
					tempPushPin.Stop = st;
#if WINDOWS_PHONE_APP
					//pushpin.Tapped += (sender, argss) =>
					//{
					//	((Pushpin)sender).BringToFront();
					//};
					//pushpin.Tapped += (o, argss) =>
					//{
					//	Pushpin tempPushpin = (Pushpin)o;
					//	Stop tmStop = (Stop)tempPushpin.Tag;
					//	//model.StopMovelView.FilteredSelectedStop = tmStop;
					//	//MapPivotItem.Focus(FocusState.Programmatic);
					//};
#endif
					//MapPanel.SetLocation(tempPushPin.Pushpin, tempPushPin.Location);
					pushpins.Add(tempPushPin);					
				}
				map.Center = new Location(model.Context.ActualStops.First().Lat, model.Context.ActualStops.First().Lng);
			}
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		
		

		async private void Button_Click(object sender, RoutedEventArgs e)
		{
			
		}

		async private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			//DataContext = model;
			
			//testListview.ItemsSource = model.StopMovelView.TimeSchedule;
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			

#if WINDOWS_PHONE_APP
			Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#else
				// Keyboard and mouse navigation only apply when occupying the entire window
				if (this.Page.ActualHeight == Window.Current.Bounds.Height &&
					this.Page.ActualWidth == Window.Current.Bounds.Width)
				{
					// Listen to the window directly so focus isn't required
					Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
						CoreDispatcher_AcceleratorKeyActivated;
					Window.Current.CoreWindow.PointerPressed +=
						this.CoreWindow_PointerPressed;
				}
#endif
			
		}

		private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
		{
			if (Pivot.SelectedItem == SearchPivotItem)
			{
				e.Handled = true;
				if (VisualStateGroup.CurrentState == ShowStopVisualState && !StopsHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "StopsVisualState", true);
				else if (VisualStateGroup.CurrentState == RoutsListVisualState && !RoutsHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "TransportListVisualState", true);
				else if (VisualStateGroup.CurrentState == ShowRoutVisualState && !RoutsHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "RoutsListVisualState", true);
				else
				{
					e.Handled = false;
				}

			}
			else if (Pivot.SelectedItem == FavourPivotItem)
			{
				e.Handled = true;

				if (FavouriteVisualStateGroup.CurrentState == FavouriteShowStopVisualState && !FavouriteStopsHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "FavouriteStopsVisualState", true);
				else if (FavouriteVisualStateGroup.CurrentState == FavouriteRoutsListVisualState && !FavouriteRoutssHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "FavouriteRoutsVisualState", true);
				else if (FavouriteVisualStateGroup.CurrentState == FavouriteShowRoutVisualState &&
				         !FavouriteRoutssHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "FavouriteRoutsListVisualState", true);
				else
					e.Handled = false;

			}else if (Pivot.SelectedItem == GroupsPivtoItem)
			{
				e.Handled = true;
				if (GroupsVisualStateGroup.CurrentState == ShowGroupVisualState)
					VisualStateManager.GoToState(mainPage, "ListGroupsVisualState", true);
				else if (GroupsVisualStateGroup.CurrentState == SelectToDeleteVisualState)
					VisualStateManager.GoToState(mainPage, "ListGroupsVisualState", true);
				else
				{
					e.Handled = false;
				}
			}
			
		}

	private bool pushpinsAll = true;
		private bool isShowBusStops;
		private bool isShowTrolStops;
		private bool isShowTramStops;
		private bool Is_Connected;
		private bool Is_InternetAvailable;
		private bool Is_Roaming;
		private bool Is_LowOnData;
		private bool Is_OverDataLimit;
		private bool Is_Wifi_Connected;

		private void OnShowStop(object sender, ShowArgs args)
		{
			pushpinsAll = true;
			Pivot.SelectedItem = MapPivotItem;
			
			//model.MapModelView.ShowStop.Execute(args.SelectedStop);

			var temp = args.SelectedStop;
			map.Center = new Location(temp.Lat, temp.Lng);
			map.ZoomLevel = 19;
		}

		private void OnShowRoute(object sender, ShowArgs args)
		{
			Pivot.SelectedItem = MapPivotItem;
			var x = args.SelectedRoute;
			pushpinsAll = false;
			if (pushpins == null)
				InicializeMap();
			//foreach (var child in pushpins)
			//{
				
			//	//map.Children.Remove(child);
			//	//child.Visibility = Visibility.Collapsed;
			//}
			//var tempRoute = args.SelectedRoute;
			Pushpins.Clear();
			foreach (var child in pushpins.Where(d => x.Stops.Any(p => p.ID == ((Stop)d.Pushpin.Tag).ID)).Select(d=>d.Pushpin))
			{
				Pushpins.Add(child);
				//map.Children.Add(child);
				//child.Visibility = Visibility.Visible;
			}
			ShowOnMap();
			map.Center = new Location(x.StartStop.Lat, x.StartStop.Lng);
			ShowAllPushPins.Visibility = Visibility.Visible;
			//model.MapModelView.ShowRout.Execute(args.SelectedRoute);
		}

		void ShowOnMap()
		{
			for (int i = 1; i < map.Children.Count; i++)
				if (!Pushpins.Contains(map.Children.ElementAt(i)))
					map.Children.RemoveAt(i);
			foreach (var pushpin in Pushpins.Where(x => !map.Children.Contains(x)))
			{
				try
				{
					map.Children.Add(pushpin);
				}
				catch (System.Exception ex)
				{ }
			}
		}

		public ObservableCollection<Pushpin> Pushpins
		{
			get
			{
				if (pushpins1 == null)
					pushpins1 = new ObservableCollection<Pushpin>();
				return pushpins1;
			}
			set
			{
				if (Equals(value, pushpins1)) return;
				pushpins1 = value;
				OnPropertyChanged();
			}
		}

		public async void RefreshPushPinsAsync()
		{
			
			if (pushpins == null)
				InicializeMap();
			if (pushpinsAll && map!=null && pushpins!=null)
			{
				var northWest = map.ViewportPointToLocation(new Windows.Foundation.Point(0, 0));
				var southEast = map.ViewportPointToLocation(new Windows.Foundation.Point(map.ActualWidth, map.ActualHeight));
				double zoomLevel = map.ZoomLevel;
				Pushpins.Clear();

				//await Task.Run(() =>
				//{
					foreach (var child in pushpins.AsParallel())
					{
						if (zoomLevel <= 14)
						{
							ShowOnMap();
							return;
							//map.Children.Remove(child);
							//child.Visibility = Visibility.Collapsed;
						}
						else
						{
							//var x = MapPanel.GetLocation(child);
							if (child.Location.Latitude <= northWest.Latitude && child.Location.Longitude >= northWest.Longitude &&
							    child.Location.Latitude >= southEast.Latitude && child.Location.Longitude <= southEast.Longitude)
							{
								Pushpins.Add(child.Pushpin);
							}
						}
					}
				//});
				ShowOnMap();
			}

			
		}

		private void map_ViewportChanged(object sender, EventArgs e)
		{
			if (map != null) RefreshPushPinsAsync();
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
#if WINDOWS_PHONE_APP
			Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#else
				Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
					CoreDispatcher_AcceleratorKeyActivated;
				Window.Current.CoreWindow.PointerPressed -=
					this.CoreWindow_PointerPressed;
#endif
			//model.Context.Save();
		}

		private void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			var flyoutBase = ((AppBarButton)sender).Flyout;
			if (flyoutBase != null)
				flyoutBase.ShowAt((FrameworkElement)sender);
			//AddGroupAppBarButton.Flyout.ShowAt(this);
		}

		private void GroupSelectedListView(object sender, SelectionChangedEventArgs e)
		{
			if (((Selector)sender).SelectedIndex == -1 || ((ListBox)sender).SelectionMode != SelectionMode.Single)
				;
			else
				VisualStateManager.GoToState(mainPage, "ShowGroupVisualState", true);
		}

		private Pushpin ipushpin;
		private ObservableCollection<Pushpin> pushpins1;

		public Pushpin Ipushpin
		{
			get
			{
				if (ipushpin == null)
					ipushpin = new Pushpin(){Content = "Я"};
				return ipushpin;
			}
			set { ipushpin = value; }
		}

		private async void AppBarButton_Click_1(object sender, RoutedEventArgs e)
		{
			Geolocator geolocator = new Geolocator();
			geolocator.DesiredAccuracy = PositionAccuracy.High;
			//geolocator.DesiredAccuracyInMeters = 5;

			try
			{
				Geoposition geoposition = await geolocator.GetGeopositionAsync(
					maximumAge: TimeSpan.FromMinutes(5),
					timeout: TimeSpan.FromSeconds(10)
					);

				map.Center = new Location(geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
				MapPanel.SetLocation(Ipushpin, map.Center);
				map.Children.Add(Ipushpin);

			}
			catch (Exception ex)
			{
				if ((uint)ex.HResult == 0x80004004)
				{
					// the application does not have the right capability or the location master switch is off
					MessageDialog dialog = new MessageDialog("location  is disabled in phone settings.");
					dialog.ShowAsync();
					
				}
				//else
				{
					// something else happened acquring the location
				}
			}
		}


		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			pushpinsAll = true;
			RefreshPushPinsAsync();
			ShowAllPushPins.Visibility = Visibility.Collapsed;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void map_TileGridChanged(object sender, EventArgs e)
		{
			
		}

		private void ZoomIn(object sender, DoubleTappedRoutedEventArgs e)
		{
			
			
		}

		private void StartEmailToDeveloper(object sender, RoutedEventArgs e)
		{
			EmailManager.ShowComposeNewEmailAsync(new EmailMessage()
			{
				Subject = "Минский общественный транспорт",
				To = { new EmailRecipient("xtergs@gmail.com") }
			});
		}

		private void ShowInStore(object sender, RoutedEventArgs e)
		{
			Launcher.LaunchUriAsync(new Uri("http://www.windowsphone.com/s?appid=0f081fb8-a7c4-4b93-b40b-d71e64dd0412"));
		}

		private async void Test(object sender, RoutedEventArgs e)
		{
			await model.Context.Save();
			var file = await ApplicationData.Current.LocalFolder.GetFileAsync("data.dat");
			//var result = await FileIO.ReadTextAsync(file);
			//var sizeFile = await file.Properties.GetDocumentPropertiesAsync();
			await model.Context.Load();
		}
	}
}
