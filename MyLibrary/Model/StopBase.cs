using Newtonsoft.Json;

namespace MinskTrans.DesctopClient.Model
{
	[JsonObject(MemberSerialization.OptIn)]
	public class StopBase
	{
		//private string name;

		[JsonProperty]
		public int ID { get; set; }
		[JsonProperty]
		public string City { get; set; }
		[JsonProperty]
		public string Area { get; set; }
		[JsonProperty]
		public string Streat { get; set; }
		[JsonProperty]
		public string Name
		{
			get; set;
			//get { return name; }
			//set
			//{
			//	name = value;
			//	SearchName = value.ToLower().Trim();
			//}
		}
		[JsonProperty]
		public string SearchName { get; set; }
		[JsonProperty]
		public string Info { get; set; }
		[JsonProperty]
		public double Lng { get; set; }
		[JsonProperty]
		public double Lat { get; set; }
		[JsonProperty]
		public string StopsStr { get; set; }
		[JsonProperty]
		public string StopNum { get; set; }
		
	}
}
