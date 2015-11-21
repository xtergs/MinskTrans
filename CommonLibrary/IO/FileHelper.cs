using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using MinskTrans.Utilites.Base.IO;
using MyLibrary;
//#if WINDOWS_PHONE_APP || WINDOWS_UAPWINDOWS_UAP

//#endif

namespace CommonLibrary.IO
{
	public class FileHelper: FileHelperBase
	{
		
		public static readonly Dictionary<TypeFolder, IStorageFolder> Folders = new Dictionary<TypeFolder, IStorageFolder>()
		{
			{TypeFolder.Local, ApplicationData.Current.LocalFolder},
			{TypeFolder.Roaming, ApplicationData.Current.RoamingFolder},
			{TypeFolder.Temp, ApplicationData.Current.TemporaryFolder},
			{TypeFolder.Current, ApplicationData.Current.LocalFolder }
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
				await file.RenameAsync(to + OldExt, NameCollisionOption.ReplaceExisting);
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
			if (text == null)
				throw new ArgumentNullException(file);
			await FileIO.WriteTextAsync(await Folders[folder].CreateFileAsync(file, CreationCollisionOption.ReplaceExisting), text);
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

	    public override async Task<IList<string>> GetNamesFiles(TypeFolder folder, string subFolder)
	    {
	        return (await Folders[folder].GetFolderAsync(subFolder).GetResults().GetFilesAsync()).Select(file => file.Name).ToList();
	    }

	    public override string GetPath(TypeFolder folder)
		{
			return Folders[folder].Path;
		}

		public async override Task<Stream> OpenStream(TypeFolder folder, string file)
		{

			return await Folders[folder].OpenStreamForReadAsync(file);
		}

		#endregion
	}
}
