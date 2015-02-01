using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
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
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
	        //model.Context.Save();
	        //model.Context.Load();

			model = MainModelView.Create(new UniversalContext());
			model.Context.DataBaseDownloadEnded += async (senderr, args) =>
			{
				
				listBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add("Data downloaded"));
				
				if (await model.Context.HaveUpdate())
					model.Context.ApplyUpdate();
				DataContext = model;
#if DEBUG
				model.Context.FavouriteStops.Add(model.Context.ActualStops.First(x => x.SearchName.Contains("шепичи")));
				model.Context.FavouriteRouts.Add(model.Context.Routs.First(x=>x.RouteNum.Contains("20")));
#endif
				
			};
			model.Context.LogMessage += (o, args) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add(args.Message));		
			model.Context.DownloadUpdate();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

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

		private void Page_Loaded(object sender, RoutedEventArgs e)
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
			    if (StopsHyperlinkButton.IsEnabled)
			    {
				    if (ShowRoutsListView.Visibility == Visibility.Visible)
				    {
						RoutsListView.Visibility = Visibility.Visible;
						ShowRoutsListView.Visibility = Visibility.Collapsed;
				    }
			    }
				else if (RoutsHyperlinkButton.IsEnabled == true)
				{
					if (ShowStop.Visibility == Visibility.Visible)
					{
						ShowStop.Visibility = Visibility.Collapsed;
						StopsListView.Visibility = Visibility.Visible;
						FindStop.Visibility = Visibility.Visible;
					}
					ShowRoutsListView.SelectionChanged -= ListView_ShowRout_ItemClick;
					ShowRoutsListView.SelectedIndex = -1;
					ShowRoutsListView.SelectionChanged += ListView_ShowRout_ItemClick;
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

	    private void ListView_ShowStop_ItemClick(object sender, SelectionChangedEventArgs e)
	    {
		    ShowStop.Visibility = Visibility.Visible;
			FindStop.Visibility = Visibility.Collapsed;
			StopsListView.Visibility = Visibility.Collapsed;
	    }

		private void HyperLinkButton_ShowStops(object sender, RoutedEventArgs e)
		{
			StopsHyperlinkButton.IsEnabled = false;
			RoutsHyperlinkButton.IsEnabled = true;
			ShowRoutsGrid.Visibility = Visibility.Collapsed;
			ShowStopsGrid.Visibility = Visibility.Visible;
		}

	    private void HyperLinkButton_ShowRouts(object sender, RoutedEventArgs e)
	    {
			StopsHyperlinkButton.IsEnabled = true;
			RoutsHyperlinkButton.IsEnabled = false;
			ShowRoutsGrid.Visibility = Visibility.Visible;
			ShowStopsGrid.Visibility = Visibility.Collapsed;
	    }

	    private void ListView_ShowRout_ItemClick(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
	    {
			

		    Frame.Navigate(typeof(RoutView));
	    }

	    private void ListView_ShowListRout_ItemClick(object sender, SelectionChangedEventArgs e)
	    {
		    RoutsListView.Visibility = Visibility.Collapsed;
		    ShowRoutsListView.Visibility = Visibility.Visible;
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
		}
    }
}
