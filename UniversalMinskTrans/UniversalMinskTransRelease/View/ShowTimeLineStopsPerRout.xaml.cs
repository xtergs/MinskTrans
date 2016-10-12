using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace UniversalMinskTransRelease.View
{
    public sealed partial class ShowTimeLineStopsPerRout : UserControl
    {
        public ShowTimeLineStopsPerRout()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("ShowTimeLineStopsPerRout");
        }
    }
}
