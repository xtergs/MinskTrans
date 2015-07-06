using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using MyLibrary;
//#if WINDOWS_PHONE_APP || WINDOWS_UAPWINDOWS_UAP

//#endif

namespace CommonLibrary.IO
{
	public sealed class FileHelper: FileHelperBase
	{
		
		public static readonly Dictionary<TypeFolder, IStorageFolder> Folders = new Dictionary<TypeFolder, IStorageFolder>()
		{
			{TypeFolder.Local, ApplicationData.Current.LocalFolder},
			{TypeFolder.Roaming, ApplicationData.Current.RoamingFolder},
			{TypeFolder.Temp, ApplicationData.Current.TemporaryFolder}
		};

		#region Overrides of FileHelperBase

		public override async Task<bool> FileExistAsync(TypeFolder folder, string file)
		{
			try
			{
				await Folders[folder].GetFileAsync(file);
				return true;
			}
			catch (FileNotFoundException)
			{
				return false;
			}
		}

		#endregion

		#region Overrides of FileHelperBase

		public override async Task SafeMoveAsync(TypeFolder folder, string @from, string to)
		{
			try
			{
				var file = await Folders[folder].GetFileAsync(to);
				file.RenameAsync(to + OldExt, NameCollisionOption.ReplaceExisting);
			}
			catch (FileNotFoundException)
			{

			}
			try
			{
				await(await Folders[folder].GetFileAsync(from)).RenameAsync(to, NameCollisionOption.ReplaceExisting);
			}
			catch (FileNotFoundException)
			{
				Debug.WriteLine("SafeMoveSync: moving file " + from + " not found");
			}
		}

		public override async Task<string> ReadAllTextAsync(TypeFolder folder, string file)
		{
			return await FileIO.ReadTextAsync(await Folders[folder].GetFileAsync(file));
		}

		public override async Task WriteTextAsync(TypeFolder folder, string file, string text)
		{
			await FileIO.WriteTextAsync(await Folders[folder].CreateFileAsync(file), text);
		}

		public override async Task DeleteFile(TypeFolder folder, string file)
		{
			try
			{
				await (await Folders[folder].GetFileAsync(file)).DeleteAsync(StorageDeleteOption.Default);
			}
			catch (FileNotFoundException)
			{
				//nothing to delete
			}
		}

		#endregion
	}
}
