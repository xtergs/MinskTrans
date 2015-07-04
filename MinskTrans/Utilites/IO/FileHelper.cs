using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

//#if WINDOWS_PHONE_APP || WINDOWS_UAPWINDOWS_UAP
using Windows.Storage;
//#endif

namespace MinskTrans.Utilites.IO
{
	public sealed class FileHelper
	{
		public static string TempExt
		{
			get { return ".temp"; }
		}

		public static string OldExt { get { return ".old"; } }
		public static string NewExt { get { return ".new"; } }

//#if WINDOWS_PHONE_APP || WINDOWS_UAP
		public static IAsyncOperation<bool> FileExistOperationAsync(IStorageFolder folder, string file)
		{
			return FileExistAsync(folder, file).AsAsyncOperation();
		}

		internal async static Task<bool> FileExistAsync(IStorageFolder folder, string file)
		{
			try
			{
				await folder.GetFileAsync(file);
				return true;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		public static IAsyncOperation<bool> FileExistLocalOperationAsync(string file)
		{
			return FileExistLocalAsync(file).AsAsyncOperation();
		}

		internal async static Task<bool> FileExistLocalAsync(string file)
		{
			return await FileExistAsync(ApplicationData.Current.LocalFolder, file);
		}

		public static IAsyncAction SafeMoveActionAsync(IStorageFolder folder, string from, string to)
		{
			return SafeMoveAsync(folder, @from, to).AsAsyncAction();
		}

		internal static async Task SafeMoveAsync(IStorageFolder folder, string from, string to)
		{
			try
			{
				var file = await folder.GetFileAsync(to);
				file.RenameAsync(to + OldExt, NameCollisionOption.ReplaceExisting);
			}
			catch (FileNotFoundException)
			{
				
			}
			try
			{
				await (await folder.GetFileAsync(from)).RenameAsync(to, NameCollisionOption.ReplaceExisting);
			}
			catch (FileNotFoundException)
			{
				Debug.WriteLine("SafeMoveSync: moving file " + from + " not found");
			}
		}
//#else
//		public static bool FileExist(string file)
//		{
//			return File.Exists(file);
//		}

//		public static async Task<bool> FileExistAsync(string file)
//		{
//			return await Task.Run(()=> File.Exists(file));
//		}

		

//		public static void SafeMove(string from, string to)
//		{
//			File.Delete(to + OldExt);
//			File.Move(to, to + OldExt);
//			File.Move(from, to);
//		}

//		public static async Task SafeMoveAsync(string from, string to)
//		{
//			await Task.Run(()=>SafeMove(from, to));
//		}
//#endif
	}
}
