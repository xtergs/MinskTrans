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

		public abstract Task<string> ReadAllTextAsync(TypeFolder folder, string file);
		public abstract Task WriteTextAsync(TypeFolder folder, string file, string text);
		public abstract Task DeleteFile(TypeFolder folder, string file);



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
	}
}
