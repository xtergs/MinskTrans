using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MapControl;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.Model;
using MinskTrans.Universal.ModelView;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MinskTrans.Universal
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
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
		private List<Pushpin> pushpins;
		States state = States.Stops;

		

		public MainPage()
		{
			this.InitializeComponent();

			//model = MainModelView.Create(new UniversalContext());
			model = MainModelView.MainModelViewGet;
			model.Context.ShowRoute += OnShowRoute;
			model.Context.ShowStop += OnShowStop;
			//model.ShowStop += OnShowStop;

			

			this.NavigationCacheMode = NavigationCacheMode.Required;
			//model.Context.Save();
			//model.Context.Load();

			//model.Context.UpdateStarted += (sender, args) => FlyoutBase.ShowAttachedFlyout(GgGrid);
			model.Context.UpdateStarted += (sender, args) =>
			{
				ProgressBar.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = true;
			};
			model.Context.UpdateEnded += async (senderr, args) =>
			{
				//FlyoutBase.GetAttachedFlyout(GgGrid).Hide();
				
				listBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add("Data downloaded"));
				
				
				//DataContext = model;
#if DEBUG
				model.Context.FavouriteStops.Add(model.Context.ActualStops.First(x => x.SearchName.Contains("шепичи")));
				model.Context.FavouriteRouts.Add(new RoutWithDestinations(model.Context.Routs.First(x=>x.RouteNum.Contains("20")), new List<string>()));
				model.Context.AllPropertiesChanged();
				string str = "s";
#endif
				ProgressBar.Visibility = Visibility.Collapsed;
				ProgressBar.IsIndeterminate = false;
				//Flyout.Show();
				//ProgressRing.Visibility = Visibility.Collapsed;

			};
			model.Context.LogMessage += (o, args) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add(args.Message));
			model.Context.LoadStarted += (sender, args) =>
			{
				ProgressBar.IsIndeterminate = true;
				ProgressBar.Visibility = Visibility.Visible;
			};
			model.Context.LoadEnded += (sender, args) =>
			{
				ProgressBar.IsIndeterminate = false;
				ProgressBar.Visibility = Visibility.Collapsed;
			};
			//model.Context.Load();


			//model.Context.DownloadUpdate();
			//model.Context.UpdateAsync();
			DataContext = model;

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
			e.Handled = true;
			if (Pivot.SelectedItem == SearchPivotItem)
			{
				//if (state == States.RoutView)
				//{
					
				//}
				if (state == States.RoutList)
					UnShowRoutList();
				else if (state == States.StopView)
					UnShowStopView();

				//if (StopsHyperlinkButton.IsEnabled)
				//{
				//	if (ShowRoutsListGrid.Visibility == Visibility.Visible)
				//	{
				//		RoutsListView.Visibility = Visibility.Visible;
				//		ShowRoutsListGrid.Visibility = Visibility.Collapsed;
				//		FindTrans.Visibility = Visibility.Visible;
				//		FindHyperButtonGrid.Visibility = Visibility.Visible;
				//	}
				//}
				//else if (RoutsHyperlinkButton.IsEnabled == true)
				//{
				//	if (ShowStop.Visibility == Visibility.Visible)
				//	{
				//		ShowStop.Visibility = Visibility.Collapsed;
				//		StopsListView.Visibility = Visibility.Visible;
				//		FindStop.Visibility = Visibility.Visible;
				//		FindHyperButtonGrid.Visibility = Visibility.Visible;
				//	}
					
				//}
			}
			else if (Pivot.SelectedItem == FavourPivotItem)
			{
				if (FavouriteStopsHyperlinkButton.IsEnabled)
				{
					ShowFavouriteStop.Visibility = Visibility.Collapsed;
					FavouriteStopsListView.Visibility = Visibility.Visible;
				}
				else if (FavouriteRoutssHyperlinkButton.IsEnabled)
				{
					
				}
			}
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			listview.ItemsSource = model.Context.Routs;
		}

		private void ListView_ItemClick(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
		{
			((ListView)sender).Visibility = Visibility.Collapsed;
			ShowFavouriteStop.Visibility = Visibility.Visible;
		}

	

		void ShowStopView()
		{
			state = States.StopView;
			ShowStop.Visibility = Visibility.Visible;
			FindStop.Visibility = Visibility.Collapsed;
			StopsListView.Visibility = Visibility.Collapsed;
			FindHyperButtonGrid.Visibility = Visibility.Collapsed;
		}

		void UnShowStopView()
		{
			state = States.Stops;
			ShowStop.Visibility = Visibility.Collapsed;
			FindStop.Visibility = Visibility.Visible;
			StopsListView.Visibility = Visibility.Visible;
			FindHyperButtonGrid.Visibility = Visibility.Visible;
			//StopsListView.SelectionChanged -= ListView_ShowStop_ItemClick;
			//StopsListView.SelectedIndex = -1;
			//StopsListView.SelectionChanged += ListView_ShowStop_ItemClick;

		}

		private void HyperLinkButton_ShowStops(object sender, RoutedEventArgs e)
		{
			ShowStops();
		}

		void ShowStops()
		{
			state = States.Stops;
			StopsHyperlinkButton.IsEnabled = false;
			RoutsHyperlinkButton.IsEnabled = true;
			ShowRoutsGrid.Visibility = Visibility.Collapsed;
			ShowStopsGrid.Visibility = Visibility.Visible;
		}

		void ShowRouts()
		{
			state = States.Routs;
			StopsHyperlinkButton.IsEnabled = true;
			RoutsHyperlinkButton.IsEnabled = false;
			ShowRoutsGrid.Visibility = Visibility.Visible;
			ShowStopsGrid.Visibility = Visibility.Collapsed;
			ShowRoutsListView.Visibility = Visibility.Collapsed;
		}

		private void HyperLinkButton_ShowRouts(object sender, RoutedEventArgs e)
		{
			ShowRouts();
		}

		private void ListView_ShowRout_ItemClick(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
		{
			//state= States.RoutView;
			Frame.Navigate(typeof(RoutView), MainModelView.MainModelViewGet.FindModelView.RoutsModelView);
		}

		private void ListView_ShowListRout_ItemClick(object sender, SelectionChangedEventArgs e)
		{
			ShowRoutList();
		}

		void ShowRoutList()
		{
			state = States.RoutList;
			FindTrans.Visibility = RoutsListView.Visibility = Visibility.Collapsed;
			ShowRoutsListView.Visibility = Visibility.Visible;
			ShowRoutsListGrid.Visibility = Visibility.Visible;
			
		}

		void UnShowRoutList()
		{
			state = States.Routs;
			FindTrans.Visibility = RoutsListView.Visibility = Visibility.Visible;
			ShowRoutsListGrid.Visibility = Visibility.Collapsed;
			ShowRoutsListView.Visibility= Visibility.Collapsed;
			ShowRoutsListView.SelectionChanged -= ListView_ShowRout_ItemClick;
			ShowRoutsListView.SelectedIndex = -1;
			ShowRoutsListView.SelectionChanged += ListView_ShowRout_ItemClick;
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
			//pushpinsAll = true;
			MapPivotItem.Focus(FocusState.Programmatic);
			//var temp = args.SelectedStop;
			//map.Center = new Location(temp.Lat, temp.Lng);
			//map.ZoomLevel = 19;
		}

		private void OnShowRoute(object sender, ShowArgs args)
		{
			MapPivotItem.Focus(FocusState.Programmatic);
		}

		public void RefreshPushPins()
		{
			if (pushpins == null)
				return;
			if (pushpinsAll)
				foreach (var child in pushpins)
				{
					if (map.ZoomLevel <= 20)
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

		private void FavouriteRoutsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			FavouriteRoutsListView.Visibility = Visibility.Collapsed;
			ShowFavouriteRoutsListView.Visibility = Visibility.Visible;
		}

		private void ShowFavouriteRoutsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Frame.Navigate(typeof(RoutView), MainModelView.MainModelViewGet.FavouriteModelView.RoutsModelView);
		}

		private void FavouriteStopsHyperlinkButton_OnClick(object sender, RoutedEventArgs e)
		{
			FavouriteStopsHyperlinkButton.IsEnabled = false;
			FavouriteRoutssHyperlinkButton.IsEnabled = true;
			ShowFavouriteStopsGrid.Visibility = Visibility.Visible;
			ShowFavouriteRoutsGrid.Visibility = Visibility.Collapsed;
		}

		private void FavouriteRoutssHyperlinkButton_OnClick(object sender, RoutedEventArgs e)
		{
			FavouriteStopsHyperlinkButton.IsEnabled = true;
			FavouriteRoutssHyperlinkButton.IsEnabled = false;
			ShowFavouriteStopsGrid.Visibility = Visibility.Collapsed;
			ShowFavouriteRoutsGrid.Visibility = Visibility.Visible;
		}

		private void StopsListView_ItemClick(object sender, ItemClickEventArgs e)
		{
			ShowStopView();

		}
	}
}
