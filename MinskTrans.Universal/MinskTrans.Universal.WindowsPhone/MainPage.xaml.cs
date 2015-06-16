﻿using System;
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
using Windows.UI.Notifications;
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
			//TileImageLoader.Cache = new MapControl.Caching.ImageFileCache();
			//TileImageLoader.DefaultCacheExpiration = new TimeSpan(10, 0, 0, 0,0);
			

			//model = MainModelView.Create(new UniversalContext());
			model = MainModelView.MainModelViewGet;

			var builder = new PushPinBuilder();
			builder.Style = (Style)mainPage.Resources["PushpinStyle1"];
			builder.Tapped += (sender, args) =>
			{
				PopupMenu menu = new PopupMenu();
				var push = ((Pushpin) sender);
				Stop stop = (Stop) push.Tag;
				menu.Commands.Add(new UICommand("Показать расписание", command =>
				{
					model.FindModelView.StopModelView.ViewStop.Execute(stop);
				}));
				menu.Commands.Add(new UICommand(model.Context.GetBusToString(stop) +
												model.Context.GetTrolToString(stop) + 
												model.Context.GetTramToString(stop) + 
												model.Context.GetMetroToString(stop)));
				menu.ShowAsync(push.RenderTransformOrigin);
			};
			
			model.MapModelView = new MapModelView(model.Context, map, model.SettingsModelView, builder);
			//MapModelView.StylePushpin = (Style) App.Current.Resources["PushpinStyle1"];
			model.Context.ShowRoute += OnShowRoute;
			model.Context.ShowStop += OnShowStop;

			model.FindModelView.StopModelView.ViewStopOn += (sender, args) =>
			{
				Pivot.SelectedItem = SearchPivotItem;
				VisualStateManager.GoToState(mainPage, "ShowStopVisualState", true);
			};

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
				ProgressBar.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ProgressBar.Visibility = Visibility.Collapsed);
			};
			model.Context.UpdateStarted += (sender, args) =>
			{
				ProgressBar.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					ProgressBar.Visibility = Visibility.Visible;
					ProgressBar.IsIndeterminate = true;

				});
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


#if BETA
			Logger.Log("MainPage ended");
#endif
		}


		private void ShowAddGroup(object sender, EventArgs args)
		{
			Pivot.SelectedItem = GroupsPivtoItem;
		}

		private MapPanel mappanel;



		

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		
		

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
#if BETA
			Logger.Log("Page_Loaded");
#endif

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
			model = MainModelView.MainModelViewGet;
			if (model.SettingsModelView.TypeError == SettingsModelView.Error.Critical ||
				model.SettingsModelView.TypeError == SettingsModelView.Error.Repeated)
			{
				ShowPopup("Произошла ошибка, отправьте лог разработчику");
				SendLog();
			}
#if BETA
			Logger.Log("Page_Loaded ended").SaveToFile();
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
			ShowAllPushPins.Visibility = Visibility.Visible;
			//model.MapModelView.ShowRout.Execute(args.SelectedRoute);
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
			model.MapModelView.ShowAllStops.Execute(null);
			ShowAllPushPins.Visibility = Visibility.Collapsed;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void StartEmailToDeveloper(object sender, RoutedEventArgs e)
		{
			SendEmailToDeveloper("");
		}

		void SendEmailToDeveloper(string str)
		{
			EmailManager.ShowComposeNewEmailAsync(new EmailMessage()
			{
				Subject = "Минский общественный транспорт",
				To =
				{
					new EmailRecipient("xtergs@gmail.com")
				},
				Body = str
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

		private void OnOffLocationServises(object sender, RoutedEventArgs e)
		{
			Launcher.LaunchUriAsync(new Uri("ms-settings-location://"));
		}

		private void ShowPolicity(object sender, RoutedEventArgs e)
		{
			MessageDialog dialog = new MessageDialog(model.SettingsModelView.PrivatyPolicity);
			dialog.ShowAsync();
		}

		private async void SendLog(object sender, RoutedEventArgs e)
		{
			SendLog();
		}

		async void SendLog()
		{
			SendEmailToDeveloper(await Logger.Log().GetAllText() + Environment.NewLine + model.SettingsModelView.LastUnhandeledException);
		}

		private void testNotifications(object sender, RoutedEventArgs e)
		{
			var notifi = ToastNotificationManager.CreateToastNotifier();

			var xaml = ToastNotificationManager.GetTemplateContent(Windows.UI.Notifications.ToastTemplateType.ToastText04);
			var textNode = xaml.GetElementsByTagName("text");
			textNode.Item(0).AppendChild(xaml.CreateTextNode("TEEST dsfjkd kjdafj jdka \n dfjkdjfkjdkfj fj"));
				//value.appendChild(toastXml.createTextNode(text));
			ToastNotification notification = new ToastNotification(xaml);
			notifi.Show(notification);
		}
	}
}
