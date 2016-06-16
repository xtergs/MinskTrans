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
    }
}
