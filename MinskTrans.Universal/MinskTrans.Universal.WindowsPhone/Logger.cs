using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.VisualBasic;

namespace MinskTrans.Universal
{
#if BETA
	partial class  Logger
	{
		private static Logger log;
		private StringBuilder builder = new StringBuilder();

		public static Logger Log()
		{
			if (log == null)
				log = new Logger();
			return log;
		}

		public static Logger Log(string str)
		{
			if (log == null)
				log = new Logger();
			log.WriteLineTime(str);
			return log;
		}

		public Logger WriteLine(string str)
		{
			builder.Append(str);
			builder.Append(Environment.NewLine);
			return this;
		}

		public Logger WriteLineTime(string str)
		{
			builder.Append(DateTime.UtcNow);
			builder.Append(": ");
			builder.Append(str);
			builder.Append(Environment.NewLine);
			return this;
		}

		public async Task SaveToFile(string file = "Error.txt")
		{
			StorageFile storage = null;
			string buffer = builder.ToString();
			builder.Clear();
			try
			{
				storage = await ApplicationData.Current.LocalFolder.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
				if ((await storage.GetBasicPropertiesAsync()).Size > 1*1024*1024)
					FileIO.WriteTextAsync(storage, String.Empty);
			}
			catch (FileNotFoundException e)
			{
			}
			await FileIO.AppendTextAsync(storage, buffer);
		}

		public async Task<string> GetAllText(string file = "Error.txt")
		{
			StorageFile storage = null;
			try
			{
				storage = await ApplicationData.Current.LocalFolder.CreateFileAsync(file, CreationCollisionOption.OpenIfExists);
			}
			catch (FileNotFoundException e)
			{
			}
			await FileIO.AppendTextAsync(storage, builder.ToString());
			return await FileIO.ReadTextAsync(storage);
		}

		public override string ToString()
		{
			return builder.ToString();
		}
	}
#endif
}
