using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MinskTrans.DesctopClient;
using MinskTrans.Service;


namespace MinskTrans.Server
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private System.Timers.Timer timer;
		public MainWindow()
		{
			InitializeComponent();
			timer = new System.Timers.Timer();
			timer.Interval = 1000 * 30;
			timer.Elapsed += (sender, args) =>
			{
				timer.Stop();
				ContextDesctop contextDesctop = new ContextDesctop();
				contextDesctop.DownloadUpdate();
				if (contextDesctop.HaveUpdate())
				{
					using (var efContext = new EFContext("default"))
					{
						if (efContext.Database.Exists())
							efContext.Database.Delete();
						efContext.Database.Create();
						efContext.Stops.AddRange(contextDesctop.Stops);
						efContext.Routs.AddRange(contextDesctop.Routs);
						efContext.Schedules.AddRange(contextDesctop.Times);
						efContext.SaveChanges();
					}
					MessageBox.Show("Updated");
				}
			};
			timer.Enabled = true;
			timer.Start();
		}
	}
}
