
//#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MinskTrans.Utilites.Base.IO;

namespace MinskTrans.Utilites.Desktop
{
	public class FileHelperDesktop: FileHelperBase
	{

		Dictionary<TypeFolder, string> folders;
		protected virtual Dictionary<TypeFolder, string> Folders
		{
			get
			{
				if (folders == null)
					folders = new Dictionary<TypeFolder, string>()
		{
			{TypeFolder.Local, Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name)},
			{TypeFolder.Roaming, Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) , Assembly.GetEntryAssembly().GetName().Name)},
			{TypeFolder.Temp, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), Assembly.GetEntryAssembly().GetName().Name)},
			{TypeFolder.Current, Directory.GetCurrentDirectory() }
		};
				return folders;
			}
		}
		

		public override string GetPath(TypeFolder folder)
		{
			return Folders[folder];
		}

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
				//throw;
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

		public override async Task<string> ReadAllTextAsync(TypeFolder folder, string file, string subfolder = "")
		{
			return File.ReadAllText(Path.Combine(Folders[folder], file));
		}

		#endregion

		

		#region Overrides of FileHelperBase

		public override async Task<FluentFileHelperBase> WriteTextAsync(TypeFolder folder, string file, string text)
		{
			string path = Folders[folder];
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(Folders[folder], file), text);
		    return new FluentFileHelperBase(this, folder, file);
		}

		#endregion

		#region Overrides of FileHelperBase

		public override async Task DeleteFile(TypeFolder folder, string file)
		{
			string path = Folders[folder];
            if (Directory.Exists(path))
				File.Delete(Path.Combine(path, file));
		}

	    public override Task DeleteFolder(TypeFolder folder, string folders)
	    {
	        throw new NotImplementedException();
	    }

	    public override async Task<IList<string>> GetNamesFiles(TypeFolder folder, string subFolder)
	    {
            string path = Path.Combine(Folders[folder], subFolder);
	        return Directory.GetFiles(path).ToList();
	    }

	    public override Task<IList<string>> GetNamesFolder(TypeFolder folder)
	    {
	        throw new NotImplementedException();
	    }

	    public async override Task<Stream> OpenStream(TypeFolder folder, string file)
		{
			string path = Folders[folder];
			return File.Open(Path.Combine(path, file), FileMode.Open);
		}

	    public override async Task WriteTextAsync(TypeFolder folder, string file, Stream text)
	    {
            string path = Folders[folder];
	        using (var stream = File.Create(Path.Combine(path, file)))
	        {
	            text.Position = 0;
	            await text.CopyToAsync(stream).ConfigureAwait(false);
                text.Close();
                text.Dispose();
	        }
        }

	    #endregion
	}
}
