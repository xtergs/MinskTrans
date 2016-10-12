using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MapControl;
using MetroLog;
using Microsoft.Xaml.Interactivity;
using MinskTrans.DesctopClient;
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

        private enum States
        {
            Stops,
            Routs,
            StopView,
            RoutList,
            RoutView
        }

        //private List<PushpinLocation> pushpins;
        //States state = States.Stops;

        public MainModelView MainView
        {
            get { return MainModelView.MainModelViewGet; }
        }

        private void ShowPopup(string text)
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
            this.InitializeComponent();
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                this.feedbackButton.Visibility = Visibility.Visible;
            }
#if DEBUG
            Stopwatch watch = new Stopwatch();
            watch.Start();
#endif

            model = MainModelView.MainModelViewGet;

            model.ShowRoute += OnShowRoute;
            model.ShowStop += OnShowStop;

            model.FindModelView.StopModelView.ViewStopOn += (sender, args) =>
            {
                Pivot.SelectedItem = SearchPivotItem;
                VisualStateManager.GoToState(mainPage, "ShowStopDetailOnlyVisualState", true);
            };

            GroupsVisualStateGroup.CurrentStateChanged += (sender, args) =>
            {
                if (args.NewState == ListGroupsVisualState || args.NewState == SelectToDeleteVisualState)
                    GroupsListView.SelectedIndex = -1;
            };

            this.NavigationCacheMode = NavigationCacheMode.Required;
            ShowStop.AddGroup += ShowAddGroup;
            DataContext = model;
#if DEBUG
            watch.Stop();

            Debug.WriteLine($"\nMainPage ctro: {watch.ElapsedMilliseconds}\n");
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
                    GoogleAnalytics.EasyTracker.GetTracker().SendView("ShowStops");
                    if (this.ActualWidth >= 800)
                    {
                        if (ShowStopsGrid.SelectedItem == null)
                        {
                            if (curState == StopsCompactVisualState)
                                return;
                            VisualStateManager.GoToState(mainPage, nameof(StopsCompactVisualState), true);
                        }
                        else
                        {
                            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("SearchTab", "Stops", "ShowStop", 0);
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

                        if (ShowStopsGrid.SelectedItem != null)
                        {
                            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("SearchTab", "Stops", "ShowStop", 0);
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
                    GoogleAnalytics.EasyTracker.GetTracker().SendView("ShowTransports");
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
                GoogleAnalytics.EasyTracker.GetTracker().SendView("Map");
                if (map == null)
                    FindName(nameof(map));
                map.ResetVisualState();
            }
        }

        private bool BackVisualState()
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
                        if (ShowStopsGrid.SelectedItem != null)
                        {
                            ShowStopsGrid.SelectedItem = null;
                            return true;
                        }
                    }
                    else
                    {
                        if (ShowStopsGrid.SelectedItem != null)
                        {
                            ShowStopsGrid.SelectedItem = null;
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
                        if (ShowStopsGrid.SelectedItem != null)
                        {
                            ShowStopsGrid.SelectedItem = null;
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
#if BETA
			Logger.Log("Page_Loaded");
#endif
            var changelog = model.SettingsModelView.ChangeLogOnce;
            if (!string.IsNullOrWhiteSpace(changelog))
                model.NotifyHelper.ShowMessageAsync(changelog);
#if WINDOWS_PHONE_APP
			HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#elif WINDOWS_UWP
            //Windows.Devices.HumanInterfaceDevice.

            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //	//Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            //	;
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

            model.FindModelView.StopModelView.Refresh();
            //await model.NewsManager.Load();
            //MainModelView.MainModelViewGet.AllNews = null;

        }

        public bool BackButton()
        {
            bool res = false;
            if (Pivot.SelectedItem == SearchPivotItem)
            {
                res = BackVisualState();
                SelectVisualState();
            }
            else if (Pivot.SelectedItem == GroupsPivtoItem)
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
            if (map == null)
                FindName(nameof(map));
            var temp = args.SelectedStop;
            model.MapModelView.ShowStopCommand.Execute(temp);
        }

        private void OnShowRoute(object sender, ShowArgs args)
        {
            Pivot.SelectedItem = MapPivotItem;
            if (map == null)
                FindName(nameof(map));
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

            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            //	//Windows.Phone.UI.Input.HardwareButtons.BackPressed -=  ;
            //	;
            ////TODO


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
            var flyoutBase = ((AppBarButton) sender).Flyout;
            if (flyoutBase != null)
                flyoutBase.ShowAt((FrameworkElement) sender);
            //AddGroupAppBarButton.Flyout.ShowAt(this);
        }

        private void GroupSelectedListView(object sender, SelectionChangedEventArgs e)
        {
            if (!(((Selector) sender).SelectedIndex == -1 || ((ListBox) sender).SelectionMode != SelectionMode.Single))
                VisualStateManager.GoToState(mainPage, "ShowGroupVisualState", true);
        }


        public event PropertyChangedEventHandler PropertyChanged;


        private void StartEmailToDeveloper(object sender, RoutedEventArgs e)
        {
            SendEmailToDeveloper("");
        }

        public async void SendEmailToDeveloper(string str, IList<string> filename,
            IList<IRandomAccessStreamReference> streams)
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
                EmailAttachment attachment = new EmailAttachment(filename[i], streams[i]);
                email.Attachments.Add(attachment);
            }
            await EmailManager.ShowComposeNewEmailAsync(email);
        }

        private async void SendEmailToDeveloper(string str)
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
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", "ShowInStore", "", 0);
            await
                Launcher.LaunchUriAsync(
                    new Uri("http://www.windowsphone.com/s?appid=0f081fb8-a7c4-4b93-b40b-d71e64dd0412"));
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

        private async void SendLog()
        {
            var fileHelper = model.FileHelper;
            using (var xxx = await LogManagerFactory.DefaultLogManager.GetCompressedLogs())
            {
                await fileHelper.DeleteFile(TypeFolder.Temp, "lll.log");
                await fileHelper.WriteTextAsync(TypeFolder.Temp, "lll.log", xxx);
            }
            var refer =
                Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(
                    await ApplicationData.Current.TemporaryFolder.GetFileAsync("lll.log"));
            SendEmailToDeveloper("Log", new[] {"log"}, new[] {refer});
        }


        private void PivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            //MainModelView.MainModelViewGet.AllNews = null;
        }

        private void PivotItem_GotFocus(object sender, RoutedEventArgs e)
        {
            MainModelView.MainModelViewGet.NewsModelView.NotifyChanges();
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
            //MainModelView.MainModelViewGet.NewsModelView.NotifyChanges();
        }

        Stopwatch watch = new Stopwatch();
        private object currentState = null;
        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            watch.Stop();
            if (watch.ElapsedMilliseconds > 0)
                GoogleAnalytics.EasyTracker.GetTracker()
                    .SendEvent("TabViewLengthSec", (Pivot.SelectedItem as PivotItem).Header.ToString(),
                        (((int) watch.ElapsedMilliseconds/1000)).ToString(), 0);
            watch.Restart();
            //GoogleAnalytics.EasyTracker.GetTracker().SendView((Pivot.SelectedItem as PivotItem).Header.ToString());
            SelectVisualState();

        }

        private void ShowChangelog(object sender, RoutedEventArgs e)
        {
            model.NotifyHelper.ShowMessageAsync(model.SettingsModelView.ChangeLog);
        }

        private void OpenSettingsClick(object sender, TappedRoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Settings");
        }

        private void UpdateScheduleManualClick(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", "UpdateScheduleManual", "", 0);
        }

        private void NoNotifyAboutNewsToggled(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", "NotifyAboutNews", (e.OriginalSource as ToggleSwitch)?.IsOn.ToString(), 0);
        }

        private void NoConsiderDistanceWhenSortToggle(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", "ConsiderDistanceWhenSortToggle", (e.OriginalSource as ToggleSwitch)?.IsOn.ToString(), 0);
        }

        private void NoConsiderFrequensyWhenSortToggle(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", "ConsiderFrequencyWhenSortToggle", (e.OriginalSource as ToggleSwitch)?.IsOn.ToString(), 0);
        }

        private void NotUseGPSToggle(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", "UseGps", (e.OriginalSource as ToggleSwitch)?.IsOn.ToString(), 0);
        }

        private void SelectedLookAtBackChanged(object sender, SelectionChangedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings", "LookAtBackChanged", (e.OriginalSource as ComboBox)?.SelectedValue?.ToString(), 0);
        }

        private void StopsClick(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("SearchTab", "Stops", "", 0);
        }

        private void ShowTransport(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("SearchTab", "Transports", "", 0);
        }

        private async void feedbackButton_Click(object sender, RoutedEventArgs e)
        {
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }
    }

    public class OpenFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement) sender);

            return null;
        }
    }
}