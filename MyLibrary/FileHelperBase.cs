using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace MyLibrary
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

		//#if WINDOWS_PHONE_APP || WINDOWS_UAP

		public abstract Task<bool> FileExistAsync(TypeFolder folder, string file);

		public abstract Task SafeMoveAsync(TypeFolder folder, string from, string to);

		public abstract Task<string> ReadAllTextAsync(TypeFolder folder, string file);
		public abstract Task WriteTextAsync(TypeFolder folder, string file, string text);
		public abstract Task DeleteFile(TypeFolder folder, string file);
	}
}
