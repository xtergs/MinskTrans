using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MinskTrans.Utilites.Base.IO
{
	public enum TypeFolder
	{
		Local,
		Roaming,
		Temp,
		Current
	}

    public class FluentFileHelperBase
    {
        public FluentFileHelperBase(FileHelperBase fileHelper, TypeFolder folder, string fileName)
        {
            this.fileHelper = fileHelper;
            this.folder = folder;
            this.fileName = fileName;
        }

        private FileHelperBase fileHelper;
        private TypeFolder folder;
        private string fileName;

        public async Task<FluentFileHelperBase> SafeMoveTo(string newFileName)
        {
            await fileHelper.SafeMoveAsync(folder, fileName, newFileName);
            fileName = newFileName;
            return this;
        }

    }

    public abstract class FileHelperBase
	{
		public static string TempExt
		{
			get { return ".temp"; }
		}

		public static string OldExt { get { return ".old"; } }
		public static string NewExt { get { return ".new"; } }

		public abstract string GetPath(TypeFolder folder);

		//#if WINDOWS_PHONE_APP || WINDOWS_UAP

		public abstract Task<bool> FileExistAsync(TypeFolder folder, string file);

		public abstract Task SafeMoveAsync(TypeFolder folder, string from, string to);

		public abstract Task<string> ReadAllTextAsync(TypeFolder folder, string file, string subfolder = "");
		public abstract Task<FluentFileHelperBase> WriteTextAsync(TypeFolder folder, string file, string text);
		public abstract Task DeleteFile(TypeFolder folder, string file);
        public abstract Task DeleteFolder(TypeFolder folder, string folders);

	    public abstract Task<IList<string>> GetNamesFiles(TypeFolder folder, string subFolder);
        public abstract Task<IList<string>> GetNamesFolder(TypeFolder folder);

		public abstract Task<Stream> OpenStream(TypeFolder folder, string file);

		public async Task DeleteFiels(TypeFolder folder, IEnumerable<string> filesList)
		{
			List<Task> taskList = filesList.Select(file => DeleteFile(folder, file)).ToList();
			await Task.WhenAll(taskList);
		}

		public async Task SafeMoveFilesAsync(TypeFolder folder, IEnumerable<string> from, IEnumerable<string> to)
		{
			List<Task> taskList = from.Select((file, index) => SafeMoveAsync(folder, file, to.ElementAt(index))).ToList();
			await Task.WhenAll(taskList);
		}

        public abstract Task WriteTextAsync(TypeFolder folder, string file, Stream text);

        public async Task ClearFolder(TypeFolder folder)
        {
            List<Task> tasks = new List<Task>();
            var list =await GetNamesFolder(folder);
            foreach (var folders in list)
            {
                tasks.Add(
                    DeleteFolder(folder, folders));
            }
            list = await GetNamesFiles(folder, "");
            foreach (var files in list)
            {
                tasks.Add(DeleteFile(folder, files));
            }
                await Task.WhenAll(tasks);

        }
	}
}
