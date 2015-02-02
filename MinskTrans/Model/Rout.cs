using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient
{
#if !WINDOWS_PHONE_APP && !WINDOWS_APP
	[Serializable]
#endif
	public class Rout :BaseModel, IXmlSerializable
	{
		public enum TransportType
		{
			None,
			Trol,
			Bus,
			Tram,
			Metro
		}

		static Dictionary<string, TransportType> transportTypeDictionary = new Dictionary<string, TransportType>()
		{
			{"", TransportType.None},
			{"trol", TransportType.Trol},
			{"bus", TransportType.Bus},
			{"metro", TransportType.Metro},
			{"tram", TransportType.Tram}
		}; 
		
		private string str = "";
		private char sym = ';';

		public Rout()
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
		}

		public string RouteNum { get; set; }
		public string Authority { get; set; }
		public string City { get; set; }
		public TransportType Transport { get; set; }
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
			get { return Stops.Last(); }
		}

		public Stop StartStop
		{
			get { return Stops.FirstOrDefault(); }
		}

		#region Implementation of IXmlSerializable

		/// <summary>
		/// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
		/// </returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
		public void ReadXml(XmlReader reader)
		{
			RouteNum = reader.GetAttribute("Num");
			Authority = reader.GetAttribute("Aut");
			City = reader.GetAttribute("C");
			TransportType transportType;
			TransportType.TryParse(reader.GetAttribute("Tr"), true, out transportType);
			Transport = transportType;
			Operator = reader.GetAttribute("Op");
			ValidityPeriods = reader.GetAttribute("Valid");
			SpecialDates = reader.GetAttribute("Special");
			RoutTag = reader.GetAttribute("Tag");
			RoutType = reader.GetAttribute("Type");
			Commercial = reader.GetAttribute("Com");
			RouteName = reader.GetAttribute("Name");
			Weekdays = reader.GetAttribute("Week");
			RoutId = Convert.ToInt32(reader.GetAttribute("Id"));
			Datestart = reader.GetAttribute("Start");
			reader.ReadStartElement("Rout");
			int count = Convert.ToInt32(reader.GetAttribute("Count"));
			RouteStops = new List<int>(count);
				reader.ReadStartElement();
			for (int i = 0; i < count; i ++) 
			{
				reader.ReadStartElement();
				RouteStops.Add(reader.ReadContentAsInt());
				if (!reader.IsEmptyElement)
					reader.ReadEndElement();
				else
				{
					
				}


			}
			if (count > 0)
				reader.ReadEndElement();
			else
			{

			}
			//reader.ReadEndElement();
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString("Num", RouteNum);
			writer.WriteAttributeString("Aut", Authority);
			writer.WriteAttributeString("C", City);
			writer.WriteAttributeString("Tr", Transport.ToString());
			writer.WriteAttributeString("Op", Operator);
			writer.WriteAttributeString("Valid", ValidityPeriods);
			writer.WriteAttributeString("Special", SpecialDates);
			writer.WriteAttributeString("Tag", RoutTag);
			writer.WriteAttributeString("Type", RoutType);
			writer.WriteAttributeString("Com", Commercial);
			writer.WriteAttributeString("Name", RouteName);
			writer.WriteAttributeString("Week", Weekdays);
			writer.WriteAttributeString("Id", RoutId.ToString());
			writer.WriteAttributeString("Start", Datestart);
			writer.WriteStartElement("StList");
			writer.WriteAttributeString("Count", RouteStops.Count.ToString());
			foreach (var routeStop in RouteStops)
			{
				writer.WriteStartElement("I");
				writer.WriteValue(routeStop);
				writer.WriteEndElement();

			}
			writer.WriteEndElement();


			
		}

		#endregion
	}
}