using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MinskTrans.DesctopClient;

namespace MinskTrans.Service
{
	public partial class Service1 : ServiceBase
	{
		private System.Timers.Timer timer;
		public Service1()
		{
			InitializeComponent();
			timer = new System.Timers.Timer();
			timer.Interval = 1000*30;
			timer.Elapsed += (sender, args) =>
			{
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
				}
			};
		}

		protected override void OnStart(string[] args)
		{
			timer.Enabled = true;
		}

		protected override void OnStop()
		{
			timer.Enabled = false;
			timer.Dispose();
		}
	}
}
