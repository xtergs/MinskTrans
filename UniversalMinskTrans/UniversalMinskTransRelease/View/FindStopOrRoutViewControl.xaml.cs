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
    public sealed partial class FindStopOrRoutViewControl : UserControl
    {
        public FindStopOrRoutViewControl()
        {
            this.InitializeComponent();
            VisualStateManager.GoToState(this, nameof(ShowListOfStopsVisualState), true);
        }

        public bool IsShowRoutList
        {
            get { return !IsShowStopList; }
            set { IsShowStopList = !value; }
        
        }

        public bool IsShowStopList
        {
            get { return this.ShowStopsGrid.Visibility == Visibility.Visible; }
            set
            {
                if (value)
                {
                    VisualStateManager.GoToState(this, nameof(ShowListOfStopsVisualState), true);
                }
                else
                {
                    VisualStateManager.GoToState(this, nameof(ShowListOrRoutesVisualState), true);
                }
                OnChangedVisibility();
            }
        }

        public event EventHandler SelectedStop;
        public event EventHandler SelectedRout;
        public event EventHandler ChangedVisibility;

        private void OnChangedVisibility()
        {
            ChangedVisibility?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedRout()
        {
            SelectedRout?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectedStop()
        {
            SelectedStop?.Invoke(this, EventArgs.Empty);
        }
    }
}
