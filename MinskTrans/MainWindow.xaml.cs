using System;
using System.Linq;
using System.Threading;
using System.Windows;
using MinskTrans.Modelview;

namespace MinskTrans
{
	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Timer timer;
		private System.Timers.Timer timerr;
		public MainWindow()
		{
			ShedulerModelView = new MainModelView();
			InitializeComponent();
			DataContext = ShedulerModelView;
			//timer = new Timer((x)=>{});
			timerr = new System.Timers.Timer(10000);
			timerr.Elapsed += (sender, args) => ShedulerModelView.StopMovelView.RefreshTimeSchedule.Execute(null);
			timerr.Start();

		}

		private MainModelView ShedulerModelView { get; set; }

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
		}

		private int w(char x, char y)
		{
			if (x == y)
				return 0;

			return 1;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string y = text1.Text;
			string x = text2.Text;
			var a = new int[x.Length][];
			for (int i = 0; i < a.Length; i++)
				a[i] = new int[y.Length + 1];

			for (int j = 0; j < a[0].Length; j++)
				a[0][j] = 0;

			for (int i = 1; i < a.Length; i++)
				a[i][0] = a[i - 1][0] + 1;

			for (int i = 1; i < a.Length; i++)
				for (int j = 1; j < a[0].Length; j++)
					a[i][j] = Math.Min(Math.Min(a[i - 1][j] + 1, a[i][j - 1] + 1), a[i - 1][j - 1] + w(y[j - 1], x[i - 1]));
			result.Text = "";
			for (int i = 0; i < a.Length; i++)
			{
				for (int j = 0; j < a[i].Length; j++)
					result.Text += a[i][j].ToString("D2") + " ";
				result.Text += "\n";
			}
			int str = 23;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			ListView.ItemsSource = ShedulerModelView.StopMovelView.Context.Stops.Where(
				x => Comparer(x.SearchName, ShedulerModelView.StopMovelView.StopNameFilter.ToLower())).Distinct();
		}


		private bool Comparer(string x, string y)
		{
			var a = new int[x.Length][];
			for (int i = 0; i < a.Length; i++)
				a[i] = new int[y.Length + 1];

			for (int j = 0; j < a[0].Length; j++)
				a[0][j] = 0;

			for (int i = 1; i < a.Length; i++)
				a[i][0] = a[i - 1][0] + 1;

			for (int i = 1; i < a.Length; i++)
				for (int j = 1; j < a[0].Length; j++)
					a[i][j] = Math.Min(Math.Min(a[i - 1][j] + 1, a[i][j - 1] + 1), a[i - 1][j - 1] + w(y[j - 1], x[i - 1]));
			result.Text = "";

			for (int j = a[a.Length - 1].Length - 1; j >= 0; j--)
				if (a[a.Length - 1][j] <= 2)
					return true;

			return false;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			timerr.Stop();
			timerr.Dispose();
		}
	}
}