
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
		ShedulerModelView ShedulerModelView { get; set; }
		public MainWindow()
		{
			ShedulerModelView = new ShedulerModelView();
			InitializeComponent();
			DataContext = ShedulerModelView;
			WebClient client = new WebClient();

			client.DownloadFile(@"http://www.minsktrans.by/city/minsk/stops.txt", "stops.txt");
			this.textBox.Text = File.ReadAllText("stops.txt");

			client.DownloadFile(@"http://www.minsktrans.by/city/minsk/routes.txt", "routes.txt");
			this.textBox1.Text = File.ReadAllText("routes.txt");

			client.DownloadFile(@"http://www.minsktrans.by/city/minsk/times.txt", "times.txt");
			this.textBox2.Text = File.ReadAllText("times.txt");

			client.Dispose();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			System.Windows.Data.CollectionViewSource shedulerModelViewViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("shedulerModelViewViewSource")));
			// Load data by setting the CollectionViewSource.Source property:
			shedulerModelViewViewSource.Source = ShedulerModelView.Routs;
			
		}
	}
}
