using System;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using MapControl;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.ModelView;
using MyLibrary;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace UniversalMinskTransRelease.View
{
	public sealed partial class MapViewControll : UserControl
	{
		private MapModelView model;
		public MapViewControll()
		{
			this.InitializeComponent();
			TileImageLoader.Cache = new MapControl.Caching.FileDbCache();
			var model = MainModelView.MainModelViewGet;
			var builder = new PushPinBuilder();
			model.MapModelView = model.MapModelViewFactory(map, builder);
			this.model = model.MapModelView;
			builder.Style = (Style)this.Resources["PushpinStyle1"];
			builder.IStyle = (Style) Resources["IPushPinStyle"];
			builder.StartPositon = (Style)Resources["StartPositionPushPinStyle"];
			builder.EndPosition = (Style)Resources["EndPositionPushPinStyle"];

            builder.Tapped +=  async (sender, args) =>
			{
				PopupMenu menu = new PopupMenu();
				var push = ((Pushpin)sender);
				Stop stop = (Stop)push.Tag;

				menu.Commands.Add(new UICommand("Показать расписание", command =>
				{
					model.FindModelView.StopModelView.ViewStop.Execute(stop);
				}));
				if (stop.Routs.Any(tr => tr.Transport == TransportType.Bus))
					menu.Commands.Add(new UICommand(model.MapModelView.TransportToString(stop, TransportType.Bus)));
				if (stop.Routs.Any(tr => tr.Transport == TransportType.Trol))
					menu.Commands.Add(new UICommand(model.MapModelView.TransportToString(stop, TransportType.Trol)));
				if (stop.Routs.Any(tr => tr.Transport == TransportType.Tram))
					menu.Commands.Add(new UICommand(model.MapModelView.TransportToString(stop, TransportType.Tram)));
				if (stop.Routs.Any(tr => tr.Transport == TransportType.Metro))
					menu.Commands.Add(new UICommand(model.MapModelView.TransportToString(stop, TransportType.Metro)));

       //         if (model.MapModelView.StartStopPushpin != push)
			    //menu.Commands.Add(new UICommand("Начало", command =>
			    //{
			    //    model.MapModelView.SetStartStop.Execute(push);
			    //}));
       //         if (model.MapModelView.EndStopPushpin != push)
       //             menu.Commands.Add(new UICommand("Конец", command =>
       //         {
       //             model.MapModelView.SetEndtStop.Execute(push);
       //         }));
				await menu.ShowAsync(map.LocationToViewportPoint(MapPanel.GetLocation(push)));
			};

			CheckToggleButton();
			this.SizeChanged += OnSizeChanged;
			DataContext = model.MapModelView;
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
		{
			CheckToggleButton();
		}

		void CreateContextMenuForPushPin()
		{
			
		}

		string MinskTransRoutingAdress = @"http://www.minsktrans.by/lookout_yard/Home/PageRouteSearch#/routes/search?";
		string MisnkTransVirtualTableAdress = @"http://www.minsktrans.by/lookout_yard/Home/Index/minsk";

		string MyPositionOnMap(string baseUri)
		{
			return baseUri + "type1=location&lat1=" + map.Center.Latitude + "&lon1=" + map.Center.Longitude;
		}

		private void ShowWebViewRouting(object sender, RoutedEventArgs e)
		{
			WebViewMap.Source = new Uri(MinskTransRoutingAdress);
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Map", "ShowWebRouting", "", 0);
            //map.Visibility = Visibility.Collapsed;
            //WebViewMap.Visibility = Visibility.Visible;
            //ShowRoutingMinskTrans.Visibility = Visibility.Collapsed;
            //ShowNormalMap.Visibility = Visibility.Visible;
        }

		private void WebViewMap_PermissionRequested(WebView sender, WebViewPermissionRequestedEventArgs args)
		{
			if (args.PermissionRequest.PermissionType == WebViewPermissionType.Geolocation)
				args.PermissionRequest.Allow();
		}

		private void ShowNormalMapClick(object sender, RoutedEventArgs e)
		{
			//map.Visibility = Visibility.Visible;
			//WebViewMap.Visibility = Visibility.Collapsed;
			//ShowRoutingMinskTrans.Visibility = Visibility.Visible;
			//ShowNormalMap.Visibility = Visibility.Collapsed;
			WebViewMap.Stop();
		}

		private void ShowIClick(object sender, RoutedEventArgs e)
		{
			((MapModelView)(this.DataContext)).ShowICommand.Execute(null);
			WebViewMap.Source = new Uri(MyPositionOnMap(MinskTransRoutingAdress));
			WebViewMap.Refresh();
		}

		private void ShowVirtualTableClick(object sender, RoutedEventArgs e)
		{
			WebViewMap.Source = new Uri(MisnkTransVirtualTableAdress);
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Map", "ShowVirtualTable", "", 0);
        }

		public void ResetVisualState()
		{
			var curState = MapVisualStates.CurrentState;
			if (this.ActualWidth >= 800)
			{
				if (curState != ShowNormalMapWideVisualState)
				{
					VisualStateManager.GoToState(this, nameof(ShowNormalMapWideVisualState), true);

				}
			}
			else
			{
				if (curState != ShowNormalMapVisualState)
				{
					VisualStateManager.GoToState(this, nameof(ShowNormalMapVisualState), true);
				}
			}
		}

		public bool ShowAllPushPinss
		{
			get { return ShowAllPushPins.Visibility == Visibility.Visible; }
			set
			{
				if (value)
					ShowAllPushPins.Visibility = Visibility.Visible;
				else
					ShowAllPushPins.Visibility = Visibility.Collapsed;
			}
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
		   model.ShowAllStops.Execute(null);
			ShowAllPushPinss = false;
		}

		private void UserControl_Loaded(object senderr, RoutedEventArgs e)
		{
			DataContext = model;
			//TileImageLoader.Cache = new MapControl.Caching.FileDbCache();
		}

		private void Grid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ActualWidth >= 800)
				return;
			Grid.SetColumn((FrameworkElement) sender, 1);
			appBarToggleButton.IsChecked = false;
			CheckToggleButton();
		}

		private void CheckToggleButton()
		{
			if (appBarToggleButton.IsChecked.Value && ActualWidth <= 800)
			{
				map.Visibility = Visibility.Collapsed;
				Grid.SetColumn(grid, 0);
			}
			else
			{
				map.Visibility = Visibility.Visible;
				Grid.SetColumn(grid, 1);
			}

			if (appBarToggleButton.IsChecked.Value)
				grid.Visibility = Visibility.Visible;
			else
				grid.Visibility = Visibility.Collapsed;
		}

		private void AppBarToggleButton_OnChecked(object sender, RoutedEventArgs e)
		{
			CheckToggleButton();
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Map", "ShowStops", true.ToString(), 0);
        }

		private void AppBarToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
		{
			CheckToggleButton();
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Map", "ShowStops", false.ToString(), 0);

        }

	    private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
	    {
	        var stop = ((FrameworkElement) sender).DataContext as Stop;
	        if (stop == null)
	            return;
	        model.ShowStopCommand.Execute(stop);
	    }

	    private void MyLocationClick(object sender, RoutedEventArgs e)
	    {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Map", "MyLocaion", "", 0);
        }

	    private void ShowAdditionalMenuClick(object sender, RoutedEventArgs e)
	    {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Map", "ShowAdditionalMenu", "", 0);
        }
	}
}
