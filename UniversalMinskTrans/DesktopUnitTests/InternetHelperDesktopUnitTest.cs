using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinskTrans.DesctopClient.Net;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			uri = @"https://gw5bfg-ch3302.files.1drv.com/y2mBmbr8FF5B0I7OkB2vG8Hc_vus7v5GrmfS1i1j_bziS3PT9pNklhm0QJH0StXkzrNomKjRRHtHY3EsV3zk1ZNFsr9WR63gp8qn6H0AgFjehxKOzEqVKUNwyyY7Vdx2sKaT3tKKJ49pD8LAmz-NLU9kg/months.dat?download&psid=1";
        }

		[TestMethod]
		public async Task DownloadStringTest()
		{
			//Arrange
			string fileText = File.ReadAllText(@"C:\Users\Artem\Dropbox\TestData\months.txt");

			//Action
			var result = await internetHelper.Download(uri);

			//Assert
			Assert.IsTrue(fileText.CompareTo(result) == 0);
		}
	}
}
