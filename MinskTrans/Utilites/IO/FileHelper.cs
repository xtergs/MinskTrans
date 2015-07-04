using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MyLibrary;

//#endif

namespace MinskTrans.DesctopClient.Utilites.IO
{
	public sealed class FileHelper: FileHelperBase
	{
		
//#if WINDOWS_PHONE_APP || WINDOWS_UAP
		
		

	
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

		private Dictionary<TypeFolder, string> Folders = new Dictionary<TypeFolder, string>()
		{
			{TypeFolder.Local, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)},
			{TypeFolder.Roaming, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)},
			{TypeFolder.Temp, Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)}
		};

		#region Overrides of FileHelperBase

		public override async Task<bool> FileExistAsync(TypeFolder folder, string file)
		{
			return await Task.Run(() => { return File.Exists(Path.Combine(Folders[folder], file)); });
		}

		#endregion

		#region Overrides of FileHelperBase

		public override async Task SafeMoveAsync(TypeFolder folder, string from, string to)
		{
				string file = Path.Combine(Folders[folder], to);
					string toT = Path.Combine(Folders[folder], to + OldExt);
			try
			{
				if (File.Exists(file))
				{
					File.Delete(toT);
					File.Move(file, toT);
				}
			}
			catch (FileNotFoundException)
			{
				throw;
			}
			try
			{
				File.Move(Path.Combine(Folders[folder], from), file);
			}
			catch (FileNotFoundException)
			{
				Debug.WriteLine("SafeMoveSync: moving file " + from + " not found");
				throw;
			}
		}

		public override async Task<string> ReadAllTextAsync(TypeFolder folder, string file)
		{
			return File.ReadAllText(Path.Combine(Folders[folder], file));
		}

		#endregion

		

		#region Overrides of FileHelperBase

		public override async Task WriteTextAsync(TypeFolder folder, string file, string text)
		{
			File.WriteAllText(Path.Combine(Folders[folder], file), text);
		}

		#endregion

		#region Overrides of FileHelperBase

		public override async Task DeleteFile(TypeFolder folder, string file)
		{
			File.Delete(Path.Combine(Folders[folder], file));
		}

		#endregion
	}
}
