using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;
using Autofac;
using CommonLibrary;
using CommonLibrary.IO;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Universal;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;

namespace MinskTrans.BackgroundUpdateTask
{
	public sealed class UpdateBackgroundTask : IBackgroundTask
	{

		private string urlUpdateDates = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111529&authkey=%21ADs9KNHO9TDPE3Q&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D4";
		private string urlUpdateNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111532&authkey=%21AAQED1sY1RWFib8&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D8";
		private string urlUpdateHotNews = @"https://onedrive.live.com/download.aspx?cid=27EDF63E3C801B19&resid=27edf63e3c801b19%2111531&authkey=%21AIJo-8Q4661GpiI&canary=3P%2F1MinRbysxZGv9ZvRDurX7Th84GvFR4kV1zdateI8%3D2";
		private string fileNews = "datesNews.dat";
		int MaxDaysAgo { get; set; }
		int MaxMinsAgo { get; set; }

		//Dictionary<int, string> DaysLinks 

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			Debug.WriteLine("Background task started");
			BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();
			var builder = new ContainerBuilder();
			builder.RegisterType<FileHelper>().As<FileHelperBase>();
			//builder.RegisterType<SqlEFContext>().As<IContext>().SingleInstance().WithParameter("connectionString", @"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity3_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
			builder.RegisterType<UniversalContext>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>();
		    builder.RegisterType<NewsManager>().As<NewsManagerBase>();
			builder.RegisterType<InternetHelperUniversal>().As<InternetHelperBase>();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>();
			var container = builder.Build();

			var fileHelper = new FileHelper();
			//InternetHelperBase internetHelper = new InternetHelperUniversal(fileHelper);
			SettingsModelView settings = new SettingsModelView();

			var context = container.Resolve<IContext>();
			InternetHelperBase helper = container.Resolve<InternetHelperBase>();
			UpdateManagerBase updateManager = container.Resolve<UpdateManagerBase>();
			NewsManagerBase manager = container.Resolve<NewsManagerBase>();
			//UpdateManagerBase updateManager = new UpdateManagerBase(fileHelper, internetHelper, new ShedulerParser());

			settings.LastUpdatedDataInBackground = SettingsModelView.TypeOfUpdate.None;
			helper.UpdateNetworkInformation();
			if (!settings.HaveConnection())
				_deferral.Complete();
			MaxDaysAgo = 30;
			MaxMinsAgo = 20;

			await helper.Download(urlUpdateDates, fileNews, TypeFolder.Temp);
			string resultStr = await FileIO.ReadTextAsync(await ApplicationData.Current.TemporaryFolder.GetFileAsync(fileNews));
			var timeShtaps = resultStr.Split('\n');
			DateTime time = new DateTime();
			if (timeShtaps.Length > 2)
				time = DateTime.Parse(timeShtaps[2]);
			//IContext context = container.Resolve<IContext>();
			if (context.LastUpdateDataDateTime < time)
			{
				try
				{
					if (await updateManager.DownloadUpdate())
					{
						var timeTable = await updateManager.GetTimeTable();
						await context.HaveUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time);
						await context.ApplyUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time);
						context.LastUpdateDataDateTime = time;
						settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.Db;
					}
					//await context.Save(false);
				}
				catch (Exception)
				{
					Logger.Log("BackgroundUpdate, updateDb Exception");
				}
			}

			if (timeShtaps.Length < 1)
			{
				_deferral.Complete();
				return;
			}

			try
			{
				time = DateTime.Parse(timeShtaps[0]);
				//NewsManager manager = new NewsManager();
				await manager.Load();
				if (time > manager.LastUpdateMainNewsDateTimeUtc)
				{
					await helper.Download(urlUpdateNews, manager.FileNameMonths, TypeFolder.Local);
					DateTime oldTime = manager.LastUpdateMainNewsDateTimeUtc;
					//manager.LastUpdateMainNewsDateTime = time;
					settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
					DateTime nowTimeUtc = DateTime.UtcNow;
				    var newNews =
				        manager.NewNews.Where(
				            key => key.PostedUtc > oldTime && ((nowTimeUtc - key.PostedUtc).TotalDays < MaxDaysAgo)).ToList();
                    if (newNews.Count>0)
                        foreach (var source in newNews)
                        {
                            ShowNotification(source.Message);
                        }
				}
				time = DateTime.Parse(timeShtaps[1]);
				if (time > manager.LastUpdateHotNewsDateTimeUtc)
				{
					await helper.Download(urlUpdateHotNews, manager.FileNameDays, TypeFolder.Local);
					DateTime oldTime = manager.LastUpdateHotNewsDateTimeUtc;
					//manager.LastUpdateHotNewsDateTime = time;
					//await FileIO.WriteTextAsync(await ApplicationData.Current.LocalFolder.CreateFileAsync(manager.fileNameHotNews, CreationCollisionOption.ReplaceExisting),
					//	resultStr);
					DateTime nowDateTimeUtc = DateTime.UtcNow;
					settings.LastUpdatedDataInBackground |= SettingsModelView.TypeOfUpdate.News;
                    //int todayDay = nowDateTime.Day;
                    //int prevday = nowDateTime.Subtract(new TimeSpan(1, 0, 0, 0)).Day;
				    var newHotNewslist = manager.AllHotNews.Where(key =>
				    {
				        if (key.RepairedLineUtc != default(DateTime))
				        {
				            double totalminutes = (nowDateTimeUtc.ToLocalTime() - key.RepairedLineLocal).TotalMinutes;
				            if (totalminutes <= MaxMinsAgo)
				                return true;
				            return false;
				        }
				        return (key.CollectedUtc > oldTime) &&
				               ((nowDateTimeUtc - key.CollectedUtc).TotalDays < 1);
				    }).ToList();
                    if (newHotNewslist.Count>0)
                        foreach (var source in newHotNewslist)
                        {
                            ShowNotification(source.Message);
                        }
				}
			}
			catch (Exception e)
			{
				string message =
					new StringBuilder("Background task exception").AppendLine(e.ToString())
						.AppendLine(e.Message)
						.AppendLine(e.StackTrace)
						.ToString();
				Debug.WriteLine(message);

				throw;
			}

			Debug.WriteLine("Background task ended");
			_deferral.Complete();
			
		}

		void ShowNotification(string text)
		{
			var notifi = ToastNotificationManager.CreateToastNotifier();

			var xaml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
			var textNode = xaml.GetElementsByTagName("text");
			textNode.Item(0).AppendChild(xaml.CreateTextNode(text));
			//value.appendChild(toastXml.createTextNode(text));
			ToastNotification notification = new ToastNotification(xaml);
			notifi.Show(notification);
		}
	}
}


