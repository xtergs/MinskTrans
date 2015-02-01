using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MinskTrans.Universal.Annotations;
using MinskTrans.Universal.Common;
using MinskTrans.Universal.ModelView;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MinskTrans.Universal
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class RoutView : Page, INotifyPropertyChanged
	{
		
		//private NavigationHelper navigationHelper;
		private ObservableDictionary defaultViewModel = new ObservableDictionary();

		public RoutView()
		{
			this.InitializeComponent();

			//this.navigationHelper = new NavigationHelper(this);
			//this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			//this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

#if WINDOWS_PHONE_APP
			Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
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



			
			DataContext = MainModelView.MainModelViewGet.FindModelView.RoutsModelView;
			//ListView.ItemsSource = MainModelView.MainModelViewGet.FindModelView.RoutsModelView.TimesObservableCollection;
			OnPropertyChanged("RouteNumSelectedValue");
		}

		private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
		{
#if WINDOWS_PHONE_APP
			Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
#else
				Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
					CoreDispatcher_AcceleratorKeyActivated;
				Window.Current.CoreWindow.PointerPressed -=
					this.CoreWindow_PointerPressed;
#endif
			e.Handled = true;
			Frame.GoBack();
		}

		/// <summary>
		/// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
		/// </summary>
		//public NavigationHelper NavigationHelper
		//{
		//	get { return this.navigationHelper; }
		//}

		/// <summary>
		/// Gets the view model for this <see cref="Page"/>.
		/// This can be changed to a strongly typed view model.
		/// </summary>
		public ObservableDictionary DefaultViewModel
		{
			get { return this.defaultViewModel; }
		}

		/// <summary>
		/// Populates the page with content passed during navigation.  Any saved state is also
		/// provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="sender">
		/// The source of the event; typically <see cref="NavigationHelper"/>
		/// </param>
		/// <param name="e">Event data that provides both the navigation parameter passed to
		/// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
		/// a dictionary of state preserved by this page during an earlier
		/// session.  The state will be null the first time a page is visited.</param>
		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
		}

		/// <summary>
		/// Preserves state associated with this page in case the application is suspended or the
		/// page is discarded from the navigation cache.  Values must conform to the serialization
		/// requirements of <see cref="SuspensionManager.SessionState"/>.
		/// </summary>
		/// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
		/// <param name="e">Event data that provides an empty dictionary to be populated with
		/// serializable state.</param>
		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{

		}

		#region NavigationHelper registration

		/// <summary>
		/// The methods provided in this section are simply used to allow
		/// NavigationHelper to respond to the page's navigation methods.
		/// <para>
		/// Page specific logic should be placed in event handlers for the  
		/// <see cref="NavigationHelper.LoadState"/>
		/// and <see cref="NavigationHelper.SaveState"/>.
		/// The navigation parameter is available in the LoadState method 
		/// in addition to page state preserved during an earlier session.
		/// </para>
		/// </summary>
		/// <param name="e">Provides data for navigation methods and event
		/// handlers that cannot cancel the navigation request.</param>
		//protected override void OnNavigatedTo(NavigationEventArgs e)
		//{
		//	this.navigationHelper.OnNavigatedTo(e);
		//}

		//protected override void OnNavigatedFrom(NavigationEventArgs e)
		//{
		//	this.navigationHelper.OnNavigatedFrom(e);
		//}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ShowScheduleButton(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
