using System;
using Windows.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace MinskTrans.Universal.WP.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void SaveUniversalContext()
		{
			//Arrange
			var context = new UniversalContext();
			//context.Update();

			//Act
			context.Save();

			//Assert
			
		}

		[TestMethod]
		public void DownloadDataUniversalContext()
		{
			//Arrange
			var context = new UniversalContext();
			//context.Update();

			//Act
			context.Save();

			//Assert

		}
	}
}
