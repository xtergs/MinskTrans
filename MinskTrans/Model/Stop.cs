using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans
{
	public class Stop
	{

		public Stop(string str, Stop stop)
			: this(str)
		{
			if (stop == null)
				return;
			if (String.IsNullOrWhiteSpace(City))
				City = stop.City;
			if (String.IsNullOrWhiteSpace(Area))
				Area = stop.Area;
			if (String.IsNullOrWhiteSpace(Streat))
				Streat = stop.Streat;
			if (string.IsNullOrWhiteSpace(Name))
				Name = stop.Name;
			//if (string.IsNullOrWhiteSpace(Info))
			//	Info = stop.Info;
			if (string.IsNullOrWhiteSpace(StopsStr))
				StopsStr = stop.StopsStr;
		}
		public Stop(string str)
		{
			int indexStart = 0;
			char sym = ';';

			int indexEnd = str.IndexOf(sym);
			ID = int.Parse(str.Substring(indexStart, indexEnd));

			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			City = str.Substring(indexStart, indexEnd - indexStart);

			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			Area = str.Substring(indexStart, indexEnd - indexStart);

			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			Streat = str.Substring(indexStart, indexEnd - indexStart);

			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			Name = str.Substring(indexStart, indexEnd - indexStart);

			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			Info = str.Substring(indexStart, indexEnd - indexStart);

			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			Lng = int.Parse(str.Substring(indexStart, indexEnd - indexStart));

			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			Lat = int.Parse(str.Substring(indexStart, indexEnd - indexStart));

			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			StopsStr = str.Substring(indexStart, indexEnd - indexStart);

			indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			StopNum = str.Substring(indexStart);

			Stops = new List<Stop>();
		}
		public int ID { get; set; }
		public string City { get; set; }
		public string Area { get; set; }
		public string Streat { get; set; }
		public string Name { get; set; }
		public string Info { get; set; }
		public int Lng { get; set; }
		public int Lat { get; set; }
		public string StopsStr { get; set; }
		public List<Stop> Stops { get; set; }
		public string StopNum { get; set; }
	}
}
