using System.Collections.Generic;
using Newtonsoft.Json;

namespace MinskTrans.DesctopClient.Model
{
	
		public enum TransportType
		{
			None,
			Trol,
			Bus,
			Tram,
			Metro
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
		[JsonProperty]
		public string RouteNum { get; set; }
		[JsonProperty]
		public string Authority { get; set; }
		[JsonProperty]
		public string City { get; set; }
		[JsonProperty]
		public TransportType Transport { get; set; }
		[JsonProperty]
		public string Operator { get; set; }
		[JsonProperty]
		public string ValidityPeriods { get; set; }
		[JsonProperty]
		public string SpecialDates { get; set; }
		[JsonProperty]
		public string RoutTag { get; set; }
		[JsonProperty]
		public string RoutType { get; set; }
		[JsonProperty]
		public string Commercial { get; set; }
		[JsonProperty]
		public string RouteName { get; set; }
		[JsonProperty]
		public string Weekdays { get; set; }
		[JsonProperty]
		public int RoutId { get; set; }
		[JsonProperty]
		public string Entry { get; set; }
		[JsonProperty]
		public List<int> RouteStops { get; set; }
		[JsonProperty]
		public string Data { get; set; }
		[JsonProperty]
		public string Datestart { get; set; }
	}
}
