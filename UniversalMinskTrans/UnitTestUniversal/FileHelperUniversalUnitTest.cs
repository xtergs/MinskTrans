using System;
using MyLibrary;

using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using CommonLibrary.IO;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace DesktopUnitTests
{
	[TestClass]
	public class FileHelperDesktopUnitTest
	{
		string filePath;
		TypeFolder type = TypeFolder.Current;
		[TestCleanup]
		public async Task Cleanup()
		{
			try
			{
				foreach (var file in await FileHelper.Folders[type].GetFilesAsync())
				{
					await file.DeleteAsync();
				}
				
                
			}
			catch (FileNotFoundException )
			{

			}
		}
		//[TestMethod]
		//public async Task TestReadAllText()
		//{
		//	//Arrange
		//	FileHelperBase fileHelper = new FileHelper();
		//	string fileName = @"test.dat";
		//	TypeFolder type = TypeFolder.Current;
		//	filePath = fileHelper.GetPath(type);
		//	string message = @"djfks_1233_fjdf ФФавао овлао лы 32 !!!! :Ж";
		//	Directory.CreateDirectory(filePath);
		//	File.WriteAllText(Path.Combine(filePath, fileName), message);

				//	//Action
				//	var getMessage = await fileHelper.ReadAllTextAsync(type, fileName );

				//	//Assert
				//	Assert.IsTrue(message.CompareTo(getMessage) == 0);
				//}

		[TestMethod]
		public async Task TestWriteAllText()
		{
			//Arrange
			FileHelper fileHelper = new FileHelper();
			string fileName = @"test.dat";
			
			var fileFolder = FileHelper.Folders[type];
			string message = @"djfks_1233_fjdf ФФавао овлао лы 32 !!!! :Ж";
			//File.WriteAllText(Path.Combine(filePath, fileName), message);

			//Action
			await fileHelper.WriteTextAsync(type, fileName, message);

			//Assert
			var getMessage = await FileIO.ReadTextAsync(await fileFolder.GetFileAsync(fileName));
			Assert.IsTrue(message.CompareTo(getMessage) == 0);
		}

		[TestMethod]
		public async Task TestWriteAllTextAlrExist()
		{
			//Arrange
			FileHelper fileHelper = new FileHelper();
			string fileName = @"test.dat";

			var fileFolder = FileHelper.Folders[type];
			string message = @"djfks_1233_fjdf ФФавао овлао лы 32 !!!! :Ж";
			await FileIO.WriteTextAsync(await fileFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting), "klk");
			//File.WriteAllText(Path.Combine(filePath, fileName), message);

			//Action
			await fileHelper.WriteTextAsync(type, fileName, message);

			//Assert
			var getMessage = await FileIO.ReadTextAsync(await fileFolder.GetFileAsync(fileName));
			Assert.IsTrue(message.CompareTo(getMessage) == 0);
		}

		//[TestMethod]
		//public async Task TestFileExist()
		//{
		//	//Arrange
		//	FileHelperDesktop fileHelper = new TestFileHelperDesktop();
		//	string fileName = @"test.dat";
		//	TypeFolder type = TypeFolder.Current;
		//	filePath = fileHelper.GetPath(type);
		//	string message = @"djfks_1233_fjdf ФФавао овлао лы 32 !!!! :Ж";
		//	Directory.CreateDirectory(filePath);
		//	File.WriteAllText(Path.Combine(filePath, fileName), message);

		//	//Action
		//	var result = await fileHelper.FileExistAsync(type, fileName);

		//	//Assert
		//	var getRes = File.Exists(Path.Combine(filePath, fileName));
		//	Assert.IsTrue(result == getRes);
		//}

		//[TestMethod]
		//public async Task TestFileNotExist()
		//{
		//	//Arrange
		//	FileHelperDesktop fileHelper = new TestFileHelperDesktop();
		//	string fileName = @"test.dat";
		//	TypeFolder type = TypeFolder.Current;
		//	filePath = fileHelper.GetPath(type);

		//	//Action
		//	var result = await fileHelper.FileExistAsync(type, fileName);

		//	//Assert
		//	var getRes = File.Exists(Path.Combine(filePath, fileName));
		//	Assert.IsTrue(result == getRes);
		//}

		//[TestMethod]
		//public async Task TestFileDeleteNotExist()
		//{
		//	//Arrange
		//	FileHelperDesktop fileHelper = new TestFileHelperDesktop();
		//	string fileName = @"test.dat";
		//	TypeFolder type = TypeFolder.Current;
		//	filePath = fileHelper.GetPath(type);

		//	//Action
		//	await fileHelper.DeleteFile(type, fileName);

		//	//Assert
		//	Assert.IsTrue(true);
		//}

		//[TestMethod]
		//public async Task TestFileDelete()
		//{
		//	//Arrange
		//	FileHelperDesktop fileHelper = new TestFileHelperDesktop();
		//	string fileName = @"test.dat";
		//	TypeFolder type = TypeFolder.Current;
		//	filePath = fileHelper.GetPath(type);
		//	Directory.CreateDirectory(filePath);
		//	File.Create(Path.Combine(filePath, fileName)).Close();

		//	//Action
		//	await fileHelper.DeleteFile(type, fileName);

		//	//Assert
		//	Assert.IsTrue(!File.Exists(Path.Combine(filePath, fileName)));
		//}
	}
}
