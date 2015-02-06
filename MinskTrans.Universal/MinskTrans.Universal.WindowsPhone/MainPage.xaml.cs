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
				model.MapModelView.Inicialize();
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
				model.MapModelView.Inicialize();
			};
			//model.Context.Load();


			//model.Context.DownloadUpdate();
			//model.Context.UpdateAsync();
			model.MapModelView = new MapModelView(model.Context, map);

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
			if (Pivot.SelectedItem == SearchPivotItem)
			{
				e.Handled = true;
				if (VisualStateGroup.CurrentState == ShowStopVisualState && !StopsHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "StopsVisualState", true);
				else if (VisualStateGroup.CurrentState == RoutsListVisualState && !RoutsHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "TransportListVisualState", true);
				else if (VisualStateGroup.CurrentState == ShowRoutVisualState && !RoutsHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "RoutsListVisualState", true);
				
			}
			else if (Pivot.SelectedItem == FavourPivotItem)
			{
				e.Handled = true;

				if (FavouriteVisualStateGroup.CurrentState == FavouriteShowStopVisualState && !FavouriteStopsHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "FavouriteStopsVisualState", true);
				else if (FavouriteVisualStateGroup.CurrentState == FavouriteRoutsListVisualState && !FavouriteRoutssHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "FavouriteRoutsVisualState", true);
				else if (FavouriteVisualStateGroup.CurrentState == FavouriteShowRoutVisualState && !FavouriteRoutssHyperlinkButton.IsEnabled)
					VisualStateManager.GoToState(mainPage, "FavouriteRoutsListVisualState", true);

			}
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			listview.ItemsSource = model.Context.Routs;
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
			MapPivotItem.Focus(FocusState.Pointer);
			model.MapModelView.ShowRout.Execute(args.SelectedRoute);
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

		
	}
}
