using System;
using System.Linq;
using Windows.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MinskTrans.Universal.Model;

namespace MinskTrans.Universal.WP.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public async void SaveUniversalContext()
		{
			//Arrange
			var context = new UniversalContext();
			await context.UpdateAsync();
			context.FavouriteRouts.Add(new RoutWithDestinations(context.Routs.First(), context));
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
