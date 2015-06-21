using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DesktopLibrary.IO
{
	public static class FileHelper
	{
		public static string TempExt
		{
			get { return ".temp"; }
		}

		public static string OldExt { get { return ".old"; } }
		public static string NewExt { get { return ".new"; } }
		public static void SafeMove(string from, string to)
		{
			File.Delete(to + OldExt);
			File.Move(to, to + OldExt);
			File.Move(from, to);
		}

		public static Task SafeMoveAsync(string from, string to)
		{
			return Task.Run(()=>SafeMove(from, to));
		}
	}
}
