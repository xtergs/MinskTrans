using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MinskTrans.DesctopClient.Model;
using Newtonsoft.Json;

namespace MinskTrans.DesctopClient
{
#if !WINDOWS_PHONE_APP && !WINDOWS_APP
	[Serializable]
#endif
	[JsonObject(MemberSerialization.OptIn)]
	public class Time : BaseModel, IXmlSerializable
	{
		public Time()
		{
		}

		public Time(Time time, int correnction)
		{
			Times = new List<int>();
			Days = time.Days;
			for (int i = 0; i < time.Times.Count; i++)
			{
				Times.Add(time.Times[i] + correnction);
			}
			Schedule = time.Schedule;
		}
		[JsonProperty]
		public string Days { get; set; }

		
		public Dictionary<int, string> daysToString = new Dictionary<int, string>()
		{
			{1, "Пн"},
			{2, "Вт"},
			{3, "Ср"},
			{4, "Чт"},
			{5, "Пт"},
			{6, "Сб"},
			{7, "Вс"}
		};
		
		public string DaysStr
		{
			get
			{
				var strBuilder = new StringBuilder();
				foreach (var day in Days)
				{
					strBuilder.Append(daysToString[int.Parse(day.ToString())] + " ");
				}
				return strBuilder.ToString();
			}
		}
		[JsonProperty]
		public List<int> Times { get; set; }

		public Dictionary<int, List<int>> DictionaryTime
		{
			get
			{
				var dic = new Dictionary<int, List<int>>();
				int val = 0;
				for (int i = 0; i < Times.Count; i++)
				{
					val = Times[i];
					int hour = val/60;
					int min = val - hour*60;
					if (hour >= 24)
						hour -= 24;

					if (dic.ContainsKey(hour))
						dic[hour].Add(min);
					else
					{
						dic.Add(hour, new List<int>());
						dic[hour].Add(min);
					}
				}
				return dic;
			}
		}

		public string ToStrTable
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (var dictionary in DictionaryTime)
				{
					builder.Append(dictionary.Key + ":");
					foreach (var i in dictionary.Value)
					{
						builder.Append(i + ", ");
					}
					builder.Remove(builder.Length - 2, 2);
					builder.Append("\n");
				}
				return builder.ToString();
			}
		}

		public Schedule Schedule { get; set; }

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
			Days = reader.GetAttribute("Days");
			int count = Convert.ToInt32(reader.GetAttribute("Count"));
			Times = new List<int>(count);
			for (int i = 0; i < count; i ++)
			{
				reader.ReadStartElement("T");
				Times.Add(Convert.ToInt32(reader.Value));
				if (reader.NodeType == XmlNodeType.EndElement)
					reader.ReadEndElement();
			}
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString("Days", Days);
			writer.WriteAttributeString("Coutn", Times.Count.ToString());
			foreach (var time in Times)
			{
				writer.WriteStartElement("T");
				writer.WriteValue(time);
				writer.WriteEndElement();
			}
		}

		#endregion
	}
}