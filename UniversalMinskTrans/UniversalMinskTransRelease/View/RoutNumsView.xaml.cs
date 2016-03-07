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
    public sealed partial class RoutNumsView : UserControl
    {
        public RoutNumsView()
        {
            this.InitializeComponent();
            this.RoutsListView.SelectionChanged += (sender, args) => OnSelectionChanged(args);
           
        }

        public object SelectedItem { get { return this.RoutsListView.SelectedItem; } set
        {
            this.RoutsListView.SelectedItem = value;
        } }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        private void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (sender != AllTogleButton && AllTogleButton != null)
                AllTogleButton.IsChecked = false;
            if (TrolTogleButton != null && sender != TrolTogleButton)
                TrolTogleButton.IsChecked = false;
            if (TramTogleButton != null && sender != TramTogleButton)
                TramTogleButton.IsChecked = false;
            if (BusTogleButton != null && sender != BusTogleButton)
                BusTogleButton.IsChecked = false;
        }
    }
}
