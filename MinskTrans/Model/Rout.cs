using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans
{
	public class Rout
	{
		int indexStart = 0;
		char sym = ';';

		int indexEnd = 0;
		private string str = "";

		public Rout(string rout, Rout routR)
			:this(rout)
		{
			if (routR == null)
				return;
			if (String.IsNullOrWhiteSpace(RouteNum))
				RouteNum = routR.RouteNum;
			if (string.IsNullOrWhiteSpace(Authority))
				Authority = routR.Authority;
			if (string.IsNullOrWhiteSpace(City))
				City = routR.City;
			if (string.IsNullOrWhiteSpace(Transport))
				Transport = routR.Transport;
			if (string.IsNullOrWhiteSpace(Operator))
				Operator = routR.Operator;
			if (string.IsNullOrWhiteSpace(ValidityPeriods))
				ValidityPeriods = routR.ValidityPeriods;
			if (string.IsNullOrWhiteSpace(SpecialDates))
				SpecialDates = routR.SpecialDates;
			if (string.IsNullOrWhiteSpace(RoutTag))
				RoutTag = routR.RoutTag;
			if (string.IsNullOrWhiteSpace(RoutType))
				RoutType = routR.RoutType;
			if (string.IsNullOrWhiteSpace(Commercial))
				Commercial = routR.Commercial;
			if (string.IsNullOrWhiteSpace(RouteName))
				RouteName = routR.RouteName;
			if (string.IsNullOrWhiteSpace(Weekdays))
				Weekdays = routR.Weekdays;

			
			if (string.IsNullOrWhiteSpace(Datestart))
				Datestart = routR.Datestart;
		}
		public Rout(string rout)
		{
			Inicialize(rout);
			RouteNum = GetNext();
			Authority = GetNext();
			City = GetNext();
			Transport = GetNext();
			Operator = GetNext();
			ValidityPeriods = GetNext();
			SpecialDates = GetNext();
			RoutTag = GetNext();
			RoutType = GetNext();
			Commercial = GetNext();
			RouteName = GetNext();
			Weekdays = GetNext();
			RoutId = int.Parse(GetNext());

			Entry = GetNext();
			var temp = GetNext().Split(',');
			RouteStops = new List<int>();
			foreach (var s in temp)
			{
				if (string.IsNullOrWhiteSpace(s))
					continue;
				RouteStops.Add(int.Parse(s));
			}
			Data = GetNext();
			Datestart = GetNext();
		}

		void Inicialize(string str)
		{
			this.str = str;
			indexStart = 0;
			indexEnd = -1;
		}

		string GetNext()
		{
			//if (!(indexStart == 0))
			//{
				indexStart = indexEnd + 1;
				indexEnd = str.IndexOf(sym, indexStart);
			//}
			if (indexEnd < 0)
				return str.Substring(indexStart);
			else
				return str.Substring(indexStart, indexEnd - indexStart);
		}

		public string RouteNum { get; set; }
		public string Authority { get; set; }
		public string City { get; set; }
		public string Transport { get; set; }
		public string Operator { get; set; }
		public string ValidityPeriods { get; set; }
		public string SpecialDates { get; set; }
		public string RoutTag { get; set; }
		public string RoutType { get; set; }
		public string Commercial { get; set; }
		public string RouteName { get; set; }
		public string Weekdays { get; set; }
		public int RoutId { get; set; }
		public Schedule Time { get; set; }
		public string Entry { get; set; }
		public List<int> RouteStops { get; set; }
		public List<Stop> Stops { get; set; } 
		public string Data { get; set; }
		public string Datestart { get; set; }

		#region Overrides of Object

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return RouteNum + " " + RouteName;
		}

		#endregion
	}
}
