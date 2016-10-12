using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace UniversalMinskTransRelease.View
{
    public sealed partial class RoutStopsTimeTableView : UserControl
    {
        public RoutStopsTimeTableView()
        {
            this.InitializeComponent();
        }

        private void ListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RoutView", "ShowStopsDetails", "", 0);
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Rout->Stop->Detail");
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Favourite", "SetFavourite", "", 0);
        }

        private void FavouriteToggle(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RoutView", "FavouriteToggle", "", 0);
        }

        private void ShowOnMapToggle(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RoutView", "ShowRoutOnMap", "", 0);
        }

        private void ShowHideRoutStopTimeTable(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RoutView", "Show/HideRout'sStopDetail", "", 0);
        }

        private void SoftBackButton_OnClick(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("RoutView", "Back", "", 0);
        }
    }
}
