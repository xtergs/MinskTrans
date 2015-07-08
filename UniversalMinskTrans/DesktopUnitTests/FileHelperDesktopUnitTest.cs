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
		string filePath;
		[TestCleanup]
		public void Cleanup()
		{
			try {
				if (filePath != null && Directory.Exists(filePath))
				{
					Directory.Delete(filePath, true);
				}
			}
			catch(FieldAccessException)
			{

			}
		}
		[TestMethod]
		public async Task TestReadAllText()
		{
			//Arrange
			FileHelperDesktop fileHelper = new TestFileHelperDesktop();
			string fileName = @"test.dat";
			TypeFolder type = TypeFolder.Current;
			filePath = fileHelper.GetPath(type);
			string message = @"djfks_1233_fjdf ФФавао овлао лы 32 !!!! :Ж";
			Directory.CreateDirectory(filePath);
			File.WriteAllText(Path.Combine(filePath, fileName), message);

			//Action
			var getMessage = await fileHelper.ReadAllTextAsync(type, fileName );

			//Assert
			Assert.IsTrue(message.CompareTo(getMessage) == 0);
		}

		[TestMethod]
		public async Task TestWriteAllText()
		{
			//Arrange
			FileHelperDesktop fileHelper = new TestFileHelperDesktop();
			string fileName = @"test.dat";
			TypeFolder type = TypeFolder.Current;
			filePath = fileHelper.GetPath(type);
			string message = @"djfks_1233_fjdf ФФавао овлао лы 32 !!!! :Ж";
			//File.WriteAllText(Path.Combine(filePath, fileName), message);

			//Action
			await fileHelper.WriteTextAsync(type, fileName, message);

			//Assert
			var getMessage = File.ReadAllText(Path.Combine(filePath, fileName));
			Assert.IsTrue(message.CompareTo(getMessage) == 0);
		}

		[TestMethod]
		public async Task TestFileExist()
		{
			//Arrange
			FileHelperDesktop fileHelper = new TestFileHelperDesktop();
			string fileName = @"test.dat";
			TypeFolder type = TypeFolder.Current;
			filePath = fileHelper.GetPath(type);
			string message = @"djfks_1233_fjdf ФФавао овлао лы 32 !!!! :Ж";
			Directory.CreateDirectory(filePath);
			File.WriteAllText(Path.Combine(filePath, fileName), message);

			//Action
			var result = await fileHelper.FileExistAsync(type, fileName);

			//Assert
			var getRes = File.Exists(Path.Combine(filePath, fileName));
			Assert.IsTrue(result == getRes);
		}

		[TestMethod]
		public async Task TestFileNotExist()
		{
			//Arrange
			FileHelperDesktop fileHelper = new TestFileHelperDesktop();
			string fileName = @"test.dat";
			TypeFolder type = TypeFolder.Current;
			filePath = fileHelper.GetPath(type);

			//Action
			var result = await fileHelper.FileExistAsync(type, fileName);

			//Assert
			var getRes = File.Exists(Path.Combine(filePath, fileName));
			Assert.IsTrue(result == getRes);
		}

		[TestMethod]
		public async Task TestFileDeleteNotExist()
		{
			//Arrange
			FileHelperDesktop fileHelper = new TestFileHelperDesktop();
			string fileName = @"test.dat";
			TypeFolder type = TypeFolder.Current;
			filePath = fileHelper.GetPath(type);

			//Action
			await fileHelper.DeleteFile(type, fileName);

			//Assert
			Assert.IsTrue(true);
		}

		[TestMethod]
		public async Task TestFileDelete()
		{
			//Arrange
			FileHelperDesktop fileHelper = new TestFileHelperDesktop();
			string fileName = @"test.dat";
			TypeFolder type = TypeFolder.Current;
			filePath = fileHelper.GetPath(type);
			Directory.CreateDirectory(filePath);
			File.Create(Path.Combine(filePath, fileName)).Close();
			
			//Action
			await fileHelper.DeleteFile(type, fileName);

			//Assert
			Assert.IsTrue(!File.Exists(Path.Combine(filePath, fileName)));
		}
	}
}
