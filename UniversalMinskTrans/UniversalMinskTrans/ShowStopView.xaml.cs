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
using MinskTrans.DesctopClient;
using Windows.Phone.UI.Input;
using Windows.UI.Core;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MinskTrans.Universal
{
	public sealed partial class ShowStopView : UserControl
	{
		public ShowStopView()
		{
			this.InitializeComponent();
			ShowStatusBar = Visibility.Visible;

			
		}

		public Visibility ShowStatusBar
		{
			get { return statusBar.Visibility; }
			set
			{
				statusBar.Visibility = value;
				if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
					SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

			}
		}



		private FlyoutBase flyout;

		private void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			flyout = ((AppBarButton)sender).Flyout;
			flyout.ShowAt((FrameworkElement) sender);
			GroupsListView.SelectedIndex = -1;
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			flyout.Hide();
		}

		public event Context.EmptyDelegate AddGroup;

		private void OnAddGroup()
		{
			var handler = AddGroup;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		private void AddGroupButtonClick(object sender, RoutedEventArgs e)
		{
			OnAddGroup();
			flyout.Hide();
		}

		private void BackButtonClick(object sender, RoutedEventArgs e)
		{
			
            
        }
	}
}
