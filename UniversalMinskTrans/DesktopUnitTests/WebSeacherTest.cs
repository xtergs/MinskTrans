using System;
using System.Threading.Tasks;
using MetroLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinskTrans.Context.Geopositioning;
using MinskTrans.Utilites.Desktop;

namespace DesktopUnitTests
{
    [TestClass]
    public class WebSeacherTest
    {
InternetHelperDesktop internetHelper ;

        [TestInitialize]
        public void Inicialize()
        {
            internetHelper = new InternetHelperDesktop(new TestFileHelperDesktop(), LogManagerFactory.DefaultLogManager);

        }

        [TestMethod]
        public async Task CorrectQuery()
        {
            WebSeacher seacher = new WebSeacher(internetHelper);

            var position = await seacher.QueryToPosition("Рокоссовсокго");

            Assert.IsTrue(position.Length > 0);
        }

        [TestMethod]
        public async Task EmptyQuery()
        {
            WebSeacher seacher = new WebSeacher(internetHelper);

            var position = await seacher.QueryToPosition("");

            Assert.IsTrue(position.Length > 0);
        }
    }
}
