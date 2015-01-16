
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;

using System.Windows;
using MinskTrans.Modelview;


namespace MinskTrans
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		MainModelView ShedulerModelView { get; set; }
		public MainWindow()
		{
			ShedulerModelView = new MainModelView();
			InitializeComponent();
			DataContext = ShedulerModelView;
			
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			
			
		}
	}
}
