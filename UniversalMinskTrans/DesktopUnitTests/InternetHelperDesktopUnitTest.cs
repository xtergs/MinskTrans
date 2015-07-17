using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using MinskTrans.Utilites.Base.Net;
using MinskTrans.Utilites.Desktop;

namespace DesktopUnitTests
{
	[TestClass]
	public class InternetHelperDesktopUnitTest
	{
		InternetHelperBase internetHelper;
		string uri;
		
		[TestInitialize]
		public void Inicialize()
		{
			internetHelper = new InternetHelperDesktop(new TestFileHelperDesktop());
			uri = @"https://docs.google.com/uc?authuser=0&id=0Bx0XRya1BPrlVlNQd0ZyOEU4cTg&export=download";
		}

		[TestMethod]
		public async Task DownloadStringTest()
		{
			//Arrange
			string fileText = File.ReadAllText(@"months.txt");

			//Action
			var result = await internetHelper.Download(uri);

			//Assert
			Assert.IsTrue(fileText.CompareTo(result) == 0);
		}
	}
}
