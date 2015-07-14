using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinskTrans.DesctopClient;
using System.Collections.Generic;
using MinskTrans.DesctopClient.Model;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Update;
using MinskTrans.DesctopClient.Utilites.IO;
using Autofac;
using MinskTrans.DesctopClient.Net;

namespace DesktopUnitTests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public async Task TestUpdateManager()
		{

			UpdateManagerDesktop updateManager = new UpdateManagerDesktop(new TestFileHelperDesktop(), new InternetHelperDesktop(new TestFileHelperDesktop()));

			using (SqlEFContext context = new SqlEFContext(@"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
			{
				context.Database.Delete();
				context.Database.CreateIfNotExists();
				await updateManager.DownloadUpdate();
				var timeTable = await updateManager.GetTimeTable();
				//	var routs = new List<Rout>()
				//{
				//	new Rout() { RoutId=0, RouteName="dsfs", RouteStops = new List<int>() {0 } },

				//};
				//new List<Stop>()
				//	{
				//	new Stop() {ID = 0, Name="dkfjsd" }
				//	}, new List<Schedule>()
				//	{
				//	new Schedule() { RoutId=0, Rout=routs[0] }
				//	})

				await context.ApplyUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time);


				var a = context.Routs;

				Assert.IsTrue(true);

			}
		}
	}
}
