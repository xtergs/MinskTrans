using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
			model.StopMovelView.FilteredSelectedStop = model.Context.Stops.First(x => x.SearchName == "шепичи" && x.Routs.Any());
			model.Context.FavouriteStops.Add(model.Context.ActualStops.First(x=>x.SearchName.Contains("шепичи")));
			//testListview.ItemsSource = model.StopMovelView.TimeSchedule;
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{

		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			model = new MainModelView(new UniversalContext());
			DataContext = model;
			model.Context.DataBaseDownloadEnded += async (senderr, args) =>
			{
				//model.Context.HaveUpdate();
				listBox.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add("Data downloaded"));
				//Task.Run(async () =>
				//{
					if (await model.Context.HaveUpdate())
						model.Context.ApplyUpdate();
				//});
			};

			model.Context.LogMessage += (o, args) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add(args.Message));
			//ShedulerParser.LogMessage += (o, args) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add(args.Message));
			model.Context.DownloadUpdate();
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
			StopsListView.Visibility = Visibility.Collapsed;
	    }
    }
}
