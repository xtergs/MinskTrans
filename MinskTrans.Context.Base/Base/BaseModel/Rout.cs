using System;
using System.Collections.Generic;
using System.Linq;

namespace MinskTrans.Context.Base.BaseModel
{
	public class Rout : RoutBase
	{
		
		
		private string str = "";
		private char sym = ';';

		public Rout()
		{
			
		}

		public Rout(Rout rout)
			:base(rout)
		{

		}

		public Rout(string rout, Rout routR)
			: this(rout)
		{
			if (routR == null)
				return;
			if (String.IsNullOrWhiteSpace(RouteNum))
				RouteNum = routR.RouteNum;
			if (string.IsNullOrWhiteSpace(Authority))
				Authority = routR.Authority;
			if (string.IsNullOrWhiteSpace(City))
				City = routR.City;
			if (Transport == TransportType.None)
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
			if (stops == null)
				stops = new List<Stop>();
		}

		public Rout(string rout)
		{
			Inicialize(rout);
			RouteNum = GetNext().Trim();
			Authority = GetNext().Trim();
			City = GetNext().Trim();
			Transport = transportTypeDictionary[GetNext()];
			Operator = GetNext().Trim();
			ValidityPeriods = GetNext().Trim();
			SpecialDates = GetNext().Trim();
			RoutTag = GetNext().Trim();
			RoutType = GetNext().Trim();
			Commercial = GetNext().Trim();
			RouteName = GetNext().Trim().Replace("  "," ").Replace("дс","ДС").Replace("д/с","ДС");
			Weekdays = GetNext().Trim();
			RoutId = int.Parse(GetNext());

			Entry = GetNext();
			string[] temp = GetNext().Split(',');
			RouteStops = new List<int>();
			foreach (string s in temp)
			{
				if (string.IsNullOrWhiteSpace(s))
					continue;
				RouteStops.Add(int.Parse(s));
			}
			Data = GetNext();
			Datestart = GetNext();
			if (stops == null)
				stops = new List<Stop>();
		}
		
		public virtual Schedule Time { get; set; }

		public virtual List<Stop> Stops
		{
			get
			{
				return stops;
			}
			set { stops = value; }
		}

		#region Overrides of Object

		/// <summary>
		///     Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		///     A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return RouteNum + " " + RouteName;
		}

		#endregion

		protected string Sym = "";
		protected string getIntStr = "";
		protected int indexEnd = 0;
		protected int indexStart = 0;
		private List<Stop> stops;

		protected virtual void Inicialize(string str, string sym)
		{
			getIntStr = str;
			Sym = sym;
			indexStart = 0;
		}

		protected int? GetInt()
		{
			string temp = GetStr();
			if (temp == null)
				return null;
			return int.Parse(temp);
		}

		protected virtual string GetStr()
		{
			indexEnd = getIntStr.IndexOf(Sym, indexStart);
			if (indexStart == indexEnd || indexStart < 0)
				return null;
			string temp;
			if (indexEnd < 0)
			{
				temp = getIntStr.Substring(indexStart);
				getIntStr = Sym;
				indexStart = 0;
				indexEnd = -1;
			}
			else
				temp = getIntStr.Substring(indexStart, indexEnd - indexStart);
			indexStart = indexEnd + 1;
			return temp;
		}

		private void Inicialize(string str)
		{
			this.str = str;
			indexStart = 0;
			indexEnd = -1;
		}

		private string GetNext()
		{
			//if (!(indexStart == 0))
			//{
			indexStart = indexEnd + 1;
			indexEnd = str.IndexOf(sym, indexStart);
			//}
			if (indexEnd < 0)
				return str.Substring(indexStart);
			return str.Substring(indexStart, indexEnd - indexStart);
		}

		public Stop DestinationStop
		{
			get
			{
				if (Stops != null) return Stops.LastOrDefault();
				return null;
			}
		}

		public Stop StartStop
		{
			get { return Stops.FirstOrDefault(); }
		}

		

		
	}
}