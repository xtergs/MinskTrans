
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;

using System.Windows;


namespace MinskTrans
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		RoutesModelview ShedulerModelView { get; set; }
		public MainWindow()
		{
			ShedulerModelView = new RoutesModelview();
			InitializeComponent();
			DataContext = ShedulerModelView;
			
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			System.Windows.Data.CollectionViewSource shedulerModelViewViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("shedulerModelViewViewSource")));
			// Load data by setting the CollectionViewSource.Source property:
			shedulerModelViewViewSource.Source = ShedulerModelView.Routs;
			
		}
	}
}
