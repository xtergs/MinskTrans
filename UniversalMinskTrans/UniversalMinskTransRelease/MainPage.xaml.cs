using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Email;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using MapControl;
using MapControl.Caching;
using MetroLog;
using Microsoft.Xaml.Interactivity;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.ModelView;
using MinskTrans.Utilites.Base.IO;
using MyLibrary;

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

		public MainModelView MainView
		{
			get { return MainModelView.MainModelViewGet; }
		}

		void ShowPopup(string text)
		{
			TextBlock textBlock = new TextBlock()
			{
				Text = text,
				FontSize = 20
			};
			Popup popup = new Popup()
			{
				VerticalAlignment = VerticalAlignment.Bottom,
				Child = textBlock,
				IsLightDismissEnabled = true
			};

			popup.IsOpen = true;
		}

		
		public MainPage()
		{
#if BETA
			Logger.Log("MainPage");
#endif
			this.InitializeComponent();

			//TileImageLoader.DefaultCacheExpiration = new TimeSpan(10, 0, 0, 0,0);
			TileImageLoader.Cache = new MapControl.Caching.FileDbCache();

			//model = MainModelView.Create(new UniversalContext());
			model = MainModelView.MainModelViewGet;

			//var builder = new PushPinBuilder();
			//builder.Style = (Style)mainPage.Resources["PushpinStyle1"];
			//builder.Tapped += async (sender, args) =>
			//{
			//	PopupMenu menu = new PopupMenu();
			//	var push = ((Pushpin) sender);
			//	Stop stop = (Stop) push.Tag;
				
			//	menu.Commands.Add(new UICommand("Показать расписание", command =>
			//	{
			//		model.FindModelView.StopModelView.ViewStop.Execute(stop);
			//	}));
			//	if (stop.Routs.Any(tr=> tr.Transport == TransportType.Bus))
			//		menu.Commands.Add(new UICommand(model.TransportToString(stop, TransportType.Bus)));
			//	if (stop.Routs.Any(tr => tr.Transport == TransportType.Trol))
			//		menu.Commands.Add(new UICommand(model.TransportToString(stop, TransportType.Trol)));
			//	if (stop.Routs.Any(tr => tr.Transport == TransportType.Tram))
			//		menu.Commands.Add(new UICommand(model.TransportToString(stop, TransportType.Tram)));
			//	if (stop.Routs.Any(tr => tr.Transport == TransportType.Metro))
			//		menu.Commands.Add(new UICommand(model.TransportToString(stop, TransportType.Metro)));
				
				
			//	await menu.ShowAsync(map.LocationToViewportPoint(MapPanel.GetLocation(push)));
			//};
			
			//model.MapModelView = new MapModelView(model.Context, map, model.SettingsModelView,model.Geolocation , builder);
			//MapModelView.StylePushpin = (Style) App.Current.Resources["PushpinStyle1"];
			model.ShowRoute += OnShowRoute;
			model.ShowStop += OnShowStop;

			model.FindModelView.StopModelView.ViewStopOn += (sender, args) =>
			{
				Pivot.SelectedItem = SearchPivotItem;
				VisualStateManager.GoToState(mainPage, "ShowStopDetailOnlyVisualState", true);
			};

			//model.ShowStop += (sender, args) => { Pivot.SelectedItem = MapPivotItem; };

			//TODO
			//model.FindModelView.StopModelView.StatusGPSChanged += async (sender, args) =>
			//{
			//	await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { model.FindModelView.StopModelView.Refresh(); });

			//};
			//TODO

			//VisualStateGroup.CurrentStateChanged += (sender, args) =>
			//{
			//	if (args.NewState == ShowStopDetailOnlyVisualState)
			//	{
			//		StopsListView.SelectedIndex = -1;
			//	} else if (args.NewState == RoutsListVisualState)
			//		RoutsListView.SelectedIndex = -1;
			//	else if (args.NewState == ShowRoutVisualState)
			//		ShowRoutsListView.SelectedIndex = -1;
				
			//};

			//FavouriteVisualStateGroup.CurrentStateChanged += (sender, args) =>
			//{
			//	if (args.NewState == FavouriteShowStopVisualState)
			//		FavouriteStopsListView.SelectedIndex = -1;
			//	else if (args.NewState == FavouriteShowRoutVisualState)
			//		ShowFavouriteRoutsListView.SelectedIndex = -1;
			//	else if (args.NewState == FavouriteRoutsListVisualState)
			//		FavouriteRoutsListView.SelectedIndex = -1;
			//};

			GroupsVisualStateGroup.CurrentStateChanged += (sender, args) =>
			{
				if (args.NewState == ListGroupsVisualState || args.NewState == SelectToDeleteVisualState)
					GroupsListView.SelectedIndex = -1;
			};
			
			//model.ShowStop += OnShowStop;

			

			this.NavigationCacheMode = NavigationCacheMode.Required;
			//model.Context.Save();
			//model.Context.Load();

			model.UpdateManager.ErrorDownloading += async (sender, args) =>
			{
				await ProgressBar.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressBar.Visibility = Visibility.Collapsed);
			};
			model.Context.UpdateDBStarted += async (sender, args) =>
			{
				await ProgressBar.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					ProgressBar.Visibility = Visibility.Visible;
					ProgressBar.IsIndeterminate = true;

				});
			};
			model.Context.UpdateDBEnded += async (senderr, args) =>
			{
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
				});

			};
			//model.Context.LogMessage += (o, args) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => listBox.Items.Add(args.Message));
			model.Context.LoadStarted += async (sender, args) =>
			{
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					ProgressBar.IsIndeterminate = true;
					ProgressBar.Visibility = Visibility.Visible;
				});
			};
			model.Context.LoadEnded += async (sender, args) =>
			{
				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					model.Context.Context.AllPropertiesChanged();
					ProgressBar.Visibility = Visibility.Collapsed;
				});
			};
			
			//ShowFavouriteStop.AddGroup += ShowAddGroup;
			ShowStop.AddGroup += ShowAddGroup;


			DataContext = model;


#if BETA
			Logger.Log("MainPage ended");
#endif
		}


		public void SelectVisualState()
		{
			if (Pivot.SelectedItem == SearchPivotItem)
			{
					var curState = VisualStateGroup.CurrentState;
				if (curState == null)
				{
					VisualStateManager.GoToState(mainPage, nameof(StopsCompactVisualState), true);
					return;
				}
				if (model.FindModelView.IsShowStopsView)
				{
					if (this.ActualWidth >= 800)
					{
						if (StopsListView.SelectedItem == null)
						{
							if (curState == StopsCompactVisualState)
								return;
							VisualStateManager.GoToState(mainPage, nameof(StopsCompactVisualState), true);
						}
						else
						{
							if (curState == StopsVisualState)
								return;
							VisualStateManager.GoToState(mainPage, nameof(StopsVisualState), true);
							SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
								AppViewBackButtonVisibility.Collapsed;
						}
					}
					else
					{
						SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
							AppViewBackButtonVisibility.Visible;
					   //if windows was in wide mode, go to compact

						if (StopsListView.SelectedItem != null)
						{
							if (curState == ShowStopVisualState)
								return;
							VisualStateManager.GoToState(mainPage, nameof(ShowStopVisualState), true);
						}
						else
						{
							if (curState == StopsCompactVisualState)
								return;
							VisualStateManager.GoToState(mainPage, nameof(StopsCompactVisualState), true);
						}
					}
				}
				else if (model.FindModelView.IsShowTransportsView)
				{
					
				   // var curState = VisualStateGroup.CurrentState;
					if (this.ActualWidth >= 800)
					{
						if (RoutsListView.SelectedItem == null)
						{
							if (curState == TransportListVisualState)
								return;
							VisualStateManager.GoToState(mainPage, nameof(TransportListVisualState), true);
						}
						else
						{
							if (curState == DescktopRoutsVisualState)
								return;
							VisualStateManager.GoToState(mainPage, nameof(DescktopRoutsVisualState), true);
							SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
							AppViewBackButtonVisibility.Collapsed;
						}
					}
					else
					{
						SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
						   AppViewBackButtonVisibility.Visible;
					   
						//if windows was in wide mode, go to compact

						if (ShowRoutsListView.SelectedItem != null)
							VisualStateManager.GoToState(mainPage, nameof(ShowRoutVisualState), true);
						else if (RoutsListView.SelectedItem != null)
						{
							VisualStateManager.GoToState(mainPage, nameof(RoutsListVisualState), true);
						}
						else
							VisualStateManager.GoToState(mainPage, nameof(TransportListVisualState), true);
					}
				}
			}
			else if (Pivot.SelectedItem == MapPivotItem)
			{
			   map.ResetVisualState();
			}
			//else if (Pivot.SelectedItem == GroupsPivtoItem)
			//{
			//    var curState = VisualStateGroup.CurrentState;
			//    if (this.ActualWidth >= 800)
			//    {
			//        if (curState != ShowGroupWideVisualState)
			//        {
			//            VisualStateManager.GoToState(mainPage, nameof(ShowGroupWideVisualState), true);

			//        }
			//    }
			//    else
			//    {
			//        if (curState != ShowGroupVisualState && model.GroupStopsModelView.SelectedGroup != null)
			//        {
			//            VisualStateManager.GoToState(mainPage, nameof(ShowGroupVisualState), true);
			//        }
			//        else
			//            VisualStateManager.GoToState(mainPage, nameof(ListGroupsVisualState), true);
			//    }
			//}
		}

		bool BackVisualState()
		{
			if (Pivot.SelectedItem == SearchPivotItem)
			{
				var curState = VisualStateGroup.CurrentState;
				if (curState == null)
				{
					return false;
				}
				if (model.FindModelView.IsShowStopsView)
				{
					if (this.ActualWidth >= 800)
					{
						if (StopsListView.SelectedItem != null)
						{
							StopsListView.SelectedItem = null;
							return true;
						}
					}
					else
					{
					   
						if (StopsListView.SelectedItem != null)
						{
							StopsListView.SelectedItem = null;
							return true;
						}
					   
					}
					return false;
				}
				else
				{
					// var curState = VisualStateGroup.CurrentState;
					if (this.ActualWidth >= 800)
					{
						if (StopsListView.SelectedItem != null)
						{
							StopsListView.SelectedItem = null;
							return true;
						}
					}
					else
					{

						if (ShowRoutsListView.SelectedItem != null)
						{
							ShowRoutsListView.SelectedItem = null;
							return true;
						}
						else if (RoutsListView.SelectedItem != null)
						{
							RoutsListView.SelectedItem = null;
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}


		private void ShowAddGroup(object sender, EventArgs args)
		{
			Pivot.SelectedItem = GroupsPivtoItem;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
#if BETA
			Logger.Log("Page_Loaded");
#endif

#if WINDOWS_PHONE_APP
			HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#elif WINDOWS_UWP
			//Windows.Devices.HumanInterfaceDevice.

			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
				//Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
				;
			SystemNavigationManager.GetForCurrentView().BackRequested += NavigationManagerBackRequsted;
			model.ExternalCommands.BackPressed +=
				(o, args) =>
				{
					if (Pivot.SelectedItem == SearchPivotItem)
					{
						BackVisualState();
						SelectVisualState();

					}
					else if (Pivot.SelectedItem == GroupsPivtoItem)
					{
						if (GroupsVisualStateGroup.CurrentState == ShowGroupVisualState)
							VisualStateManager.GoToState(mainPage, "ListGroupsVisualState", true);
						else if (GroupsVisualStateGroup.CurrentState == SelectToDeleteVisualState)
							VisualStateManager.GoToState(mainPage, "ListGroupsVisualState", true);
						else
						{
							return;
						}
					}
				};

			//  ToDo:


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
			model = MainModelView.MainModelViewGet;
			if (model.SettingsModelView.TypeError == Error.Critical ||
				model.SettingsModelView.TypeError == Error.Repeated)
			{
				ShowPopup("Произошла ошибка, отправьте лог разработчику");
				SendLog();
			}

			//await model.NewsManager.Load();
			MainModelView.MainModelViewGet.AllNews = null;
			
#if BETA
			await Logger.Log("Page_Loaded ended").SaveToFile();
#endif
		}

		public bool BackButton()
		{
			bool res = false;
			if (Pivot.SelectedItem == SearchPivotItem)
			{
				res = BackVisualState();
				SelectVisualState();

			}
			else
			if (Pivot.SelectedItem == GroupsPivtoItem)
			{
				res = true;
				if (GroupsVisualStateGroup.CurrentState == ShowGroupVisualState)
					VisualStateManager.GoToState(mainPage, "ListGroupsVisualState", true);
				else if (GroupsVisualStateGroup.CurrentState == SelectToDeleteVisualState)
					VisualStateManager.GoToState(mainPage, "ListGroupsVisualState", true);
				else
				{
					res = false;
				}
			}
			return res;
		}

		private void NavigationManagerBackRequsted(object sender, BackRequestedEventArgs e)
		{
			e.Handled = BackButton();
		}

#if WINDOWS_UAP
		private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
		{
			e.Handled = BackButton();

		}
#endif
	

		private void OnShowStop(object sender, ShowArgs args)
		{
			Pivot.SelectedItem = MapPivotItem;
			
			//model.MapModelView.ShowStop.Execute(args.SelectedStop);

			var temp = args.SelectedStop;
			model.MapModelView.ShowStopCommand.Execute(temp);
		}

		private void OnShowRoute(object sender, ShowArgs args)
		{
			Pivot.SelectedItem = MapPivotItem;
			var x = args.SelectedRoute;
			model.MapModelView.ShowRoutCommand.Execute(x);
			map.ShowAllPushPinss = true;
			//model.MapModelView.ShowRout.Execute(args.SelectedRoute);
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
#if WINDOWS_PHONE_APP
			HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#elif WINDOWS_UWP

			if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
				//Windows.Phone.UI.Input.HardwareButtons.BackPressed -=  ;
				;
			//TODO


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
			if (!(((Selector)sender).SelectedIndex == -1 || ((ListBox)sender).SelectionMode != SelectionMode.Single))
				VisualStateManager.GoToState(mainPage, "ShowGroupVisualState", true);
		}


		

		public event PropertyChangedEventHandler PropertyChanged;


		private void StartEmailToDeveloper(object sender, RoutedEventArgs e)
		{
			SendEmailToDeveloper("");
		}

		public async void SendEmailToDeveloper(string str, IList<string> filename, IList<IRandomAccessStreamReference> streams )
		{
			var email = new EmailMessage()
			{

				Subject = "Минский общественный транспорт",
				To =
				{
					new EmailRecipient("xtergs@gmail.com")
				},
				Body = str
			};
			for (int i = 0; i < filename.Count; i++)
			{
				EmailAttachment attachment = new EmailAttachment(filename[i], streams[i] );
				email.Attachments.Add(attachment);
			}
			await EmailManager.ShowComposeNewEmailAsync(email);
		}

		async void SendEmailToDeveloper(string str)
		{
			await EmailManager.ShowComposeNewEmailAsync(new EmailMessage()
			{ 
				
				Subject = "Минский общественный транспорт",
				To =
				{
					new EmailRecipient("xtergs@gmail.com")
				},
				Body = str
			});
		}

		private async void ShowInStore(object sender, RoutedEventArgs e)
		{
			await Launcher.LaunchUriAsync(new Uri("http://www.windowsphone.com/s?appid=0f081fb8-a7c4-4b93-b40b-d71e64dd0412"));
		}

		private async void OnOffLocationServises(object sender, RoutedEventArgs e)
		{
			await Launcher.LaunchUriAsync(new Uri("ms-settings-location://"));
		}

		private async void ShowPolicity(object sender, RoutedEventArgs e)
		{
			MessageDialog dialog = new MessageDialog(model.SettingsModelView.PrivatyPolicity);
			await dialog.ShowAsync();
		}

		private void SendLog(object sender, RoutedEventArgs e)
		{
			SendLog();
		}

		async void SendLog()
		{
			string message = "";

	   
			var fileHelper = model.FileHelper;
			var xxx = await LogManagerFactory.DefaultLogManager.GetCompressedLogs();
			await fileHelper.DeleteFile(TypeFolder.Temp, "lll.log");
			await fileHelper.WriteTextAsync(TypeFolder.Temp, "lll.log", xxx);
			xxx.Dispose();
			var refer = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(await ApplicationData.Current.TemporaryFolder.GetFileAsync("lll.log"));
			SendEmailToDeveloper("Log", new []{"log"}, new [] {refer});
		}

		

		private void PivotItem_Loaded(object sender, RoutedEventArgs e)
		{
			MainModelView.MainModelViewGet.AllNews = null;
		}

		private void PivotItem_GotFocus(object sender, RoutedEventArgs e)
		{
			MainModelView.MainModelViewGet.NewsModelView.FilteredStops = null;
		}

		private void Pivot_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{

		}

		private void mainPage_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
		{

		}

		private void mainPage_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
		{
			
		}

		

		private void mainPage_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			SelectVisualState();
		}

		private void PivotItem_GotFocus_1(object sender, RoutedEventArgs e)
		{
			MainModelView.MainModelViewGet.NewsModelView.FilteredStops = null;
		}

		private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectVisualState();
		}
	}

	public class OpenFlyoutAction : DependencyObject, IAction
	{
		public object Execute(object sender, object parameter)
		{
			FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);

			return null;
		}
	}
}
