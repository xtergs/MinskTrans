using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinskTrans.Context.Base.BaseModel
{
	[Flags]
	public enum TransportType
		{
			None,
			Trol = 0x00000001,
			Bus = 0x00000002,
			Tram = 0x00000004,
			Metro = 0x00000008,
            All = Tram | Trol | Bus| Metro
	}
	[JsonObject(MemberSerialization.OptIn)]
	public class RoutBase
	{

		protected static Dictionary<string, TransportType> transportTypeDictionary = new Dictionary<string, TransportType>()
		{
			{"", TransportType.None},
			{"trol", TransportType.Trol},
			{"bus", TransportType.Bus},
			{"metro", TransportType.Metro},
			{"tram", TransportType.Tram}
		};

		public RoutBase()
		{ }

		public RoutBase(RoutBase routbase)
		{
			RouteNum = routbase.RouteNum;
			Authority = routbase.Authority;
			City = routbase.City;
			Transport = routbase.Transport;
			Operator = routbase.Operator;
			ValidityPeriods = routbase.ValidityPeriods;
			SpecialDates = routbase.SpecialDates;
			RoutTag = routbase.RoutTag;
			RoutType = routbase.RoutType;
			Commercial = routbase.Commercial;
			RouteName = routbase.RouteName;
			Weekdays = routbase.Weekdays;
			RoutId = routbase.RoutId;
			Entry = routbase.Entry;
			RouteStops = routbase.RouteStops;
			Data = routbase.Data;
			Datestart = routbase.Datestart;
		}

		[JsonProperty]
		public string RouteNum { get; set; }
		//[JsonProperty]
		public string Authority { get; set; }
		//[JsonProperty]
		public string City { get; set; }
		[JsonProperty]
		public TransportType Transport { get; set; }
		//[JsonProperty]
		public string Operator { get; set; }
		//[JsonProperty]
		public string ValidityPeriods { get; set; }
		[JsonProperty]
		public string SpecialDates { get; set; }
		//[JsonProperty]
		public string RoutTag { get; set; }
		[JsonProperty]
		public string RoutType { get; set; }
		//[JsonProperty]
		public string Commercial { get; set; }
		[JsonProperty]
		public string RouteName { get; set; }
		[JsonProperty]
		public string Weekdays { get; set; }
		[JsonProperty]
		public int RoutId { get; set; }
		//[JsonProperty]
		public string Entry { get; set; }
		[JsonProperty]
		public List<int> RouteStops { get; set; }
		//[JsonProperty]
		public string Data { get; set; }
		//[JsonProperty]
		public string Datestart { get; set; }
	}
}
