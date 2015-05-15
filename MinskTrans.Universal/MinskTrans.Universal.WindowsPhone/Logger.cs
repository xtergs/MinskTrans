using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			return this;
		}

		public Logger WriteLineTime(string str)
		{
			builder.Append(DateTime.UtcNow);
			builder.Append(": ");
			builder.Append(str);
			return this;
		}

		public override string ToString()
		{
			return builder.ToString();
		}
	}
#endif
}
