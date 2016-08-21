using System.Linq;
using System.Threading.Tasks;
using MetroLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinskTrans.Context;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Utilites.Desktop;

namespace DesktopUnitTests
{
	[TestClass]
	public class ContextUnitTest
	{
		[TestMethod]
		public async Task AddFavouriteRout()
		{
            //Arrange
		    var fileHelper = new TestFileHelperDesktop();
            Context cont = new Context(fileHelper, new InternetHelperDesktop(fileHelper, LogManagerFactory.DefaultLogManager), new FilePathsSettings());
		    var rout = new Rout();
		   //ToDO cont.Routs = new [] {rout};;

            //Act
			await cont.AddFavouriteRout(rout);

            //Assert
		    Assert.AreEqual(1, cont.FavouriteRouts.Count);
		    Assert.IsTrue(cont.IsFavouriteRout(cont.Routs.First()));
		}

        [TestMethod]
        public async Task AddFavouriteStop()
        {
            //Arrange
            var fileHelper = new TestFileHelperDesktop();
            Context cont = new Context(fileHelper, new InternetHelperDesktop(fileHelper, LogManagerFactory.DefaultLogManager), new FilePathsSettings());
            var stop = new Stop();
            //ToDO cont.Stops.Add(stop);

            //Act
            await cont.AddFavouriteStop(cont.Stops.First());

            //Assert
            Assert.AreEqual(cont.FavouriteStops.Count, 1);
            Assert.IsTrue(cont.IsFavouriteStop(cont.Stops.First()));
        }
    }
}
