using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;


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
			set { statusBar.Visibility = value; }
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

		public event EventHandler<EventArgs> AddGroup;

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

	    private void TesstListview_OnItemClick(object sender, ItemClickEventArgs e)
	    {
	        throw new NotImplementedException();
	    }
	}
}
