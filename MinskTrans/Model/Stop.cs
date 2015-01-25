using System;
using System.Collections.Generic;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient
{
#if !WINDOWS_PHONE_APP && !WINDOWS_APP
	[Serializable]
#endif
	public class Stop : BaseModel
	{
		private string name;

		private Stop()
		{ }
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
			//int indexStart = 0;
			char sym = ';';

			Inicialize(str, sym.ToString());

			//int indexEnd = str.IndexOf(sym);
			ID = int.Parse(GetStr());

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			City = GetStr();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Area = GetStr();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Streat = GetStr();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Name = GetStr();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Info = GetStr();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Lng = double.Parse(GetStr()) / 100000;

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Lat = double.Parse(GetStr()) / 100000;

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			StopsStr = GetStr();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			StopNum = GetStr();

			Stops = new List<Stop>();
		}

		public int ID { get; set; }
		public string City { get; set; }
		public string Area { get; set; }
		public string Streat { get; set; }

		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				SearchName = value.ToLower().Trim();
			}
		}

		public string SearchName { get; set; }
		public string Info { get; set; }
		public double Lng { get; set; }
		public double Lat { get; set; }
		public string StopsStr { get; set; }
		public List<Stop> Stops { get; set; }
		public string StopNum { get; set; }

		#region Overrides of BaseModel

		protected override string GetStr()
		{
			indexEnd = getIntStr.IndexOf(Sym, indexStart);

			string temp;
			if (indexEnd < 0)
			{
				temp = getIntStr.Substring(indexStart);
			}
			else
				temp = getIntStr.Substring(indexStart, indexEnd - indexStart);
			indexStart = indexEnd + 1;
			return temp;
		}

		#endregion
	}
}