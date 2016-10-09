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
    public sealed partial class StopsListView : UserControl
    {
        public StopsListView()
        {
            this.InitializeComponent();
            this.StopsList.SelectionChanged += (sender, args) => OnSelectionChanged(args);
            this.WebStopsList.SelectionChanged += (sender, args) => OnSelectionChanged(args);
        }

        public object SelectedItem
        {
            get { return this.StopsList.SelectedItem ?? this.WebStopsList.SelectedItem; }
            set
            {
                this.StopsList.SelectedItem = value;
                this.WebStopsList.SelectedItem = value;
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        private void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        private void ToggleFavourite(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Stops", "ToggleFavourite", "", 0);
        }

        private void ToggleCheckBoxBus(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Stops", "TransportBus", (sender as CheckBox)?.IsChecked.ToString(), 0);
        }

        private void ToggleCheckBoxTrol(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Stops", "TransportTrol", (sender as CheckBox)?.IsChecked.ToString(), 0);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Stops", "TransportTram", (sender as CheckBox)?.IsChecked.ToString(), 0);
        }

        private void ToggleCheckBoxMetro(object sender, RoutedEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Stops", "TransportMetro", (sender as CheckBox)?.IsChecked.ToString(), 0);
        }
    }
}
