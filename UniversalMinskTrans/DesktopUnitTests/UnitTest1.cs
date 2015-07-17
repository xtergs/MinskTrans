using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using MinskTrans.DesctopClient;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Desktop;

namespace DesktopUnitTests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public async Task TestUpdateManager()
		{

			UpdateManagerBase updateManager = new UpdateManagerBase(new TestFileHelperDesktop(), new InternetHelperDesktop(new TestFileHelperDesktop()), new ShedulerParser());

			using (SqlEFContext context = new SqlEFContext(@"Data Source=(localdb)\ProjectsV12;Initial Catalog=Entity_Test_MinskTrans;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
			{
				context.Database.Delete();
				context.Database.CreateIfNotExists();
				await updateManager.DownloadUpdate();
				var timeTable = await updateManager.GetTimeTable();
				
				await context.ApplyUpdate(timeTable.Routs, timeTable.Stops, timeTable.Time);


				var a = context.Routs;

				Assert.IsTrue(true);

			}
		}
	}
}
