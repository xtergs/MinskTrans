using System;
using MyLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinskTrans.DesctopClient.Utilites.IO;
using System.IO;
using System.Threading.Tasks;

namespace DesktopUnitTests
{
	[TestClass]
	public class FileHelperDesktopUnitTest
	{
		[TestMethod]
		public async Task TestsMethod1()
		{
			//Arrange
			FileHelperDesktop fileHelper = new FileHelperDesktop();
			string fileName = @"test.dat";
			TypeFolder type = TypeFolder.Current;
			string filePath = fileHelper.GetPath(type);
			string message = @"djfks_1233_fjdf ФФавао овлао лы 32 !!!! :Ж";
			File.WriteAllText(Path.Combine(filePath, fileName), message);

			//Action
			var getMessage = await fileHelper.FileExistAsync(type, fileName );

			//Assert
			Assert.IsTrue(message.CompareTo(getMessage) == 0);
		}
	}
}
