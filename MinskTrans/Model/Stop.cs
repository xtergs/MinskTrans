using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient
{
#if !WINDOWS_PHONE_APP && !WINDOWS_APP
	[Serializable]
#endif
	public class Stop : BaseModel, IXmlSerializable
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

		#region Implementation of IXmlSerializable

		/// <summary>
		/// Этот метод является зарезервированным, и его не следует использовать. При реализации интерфейса IXmlSerializable этот метод должен возвращать значение null (Nothing в Visual Basic), а если необходимо указать пользовательскую схему, то вместо использования метода следует применить <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> к классу.
		/// </summary>
		/// <returns>
		/// <see cref="T:System.Xml.Schema.XmlSchema"/>, описывающая представление XML объекта, полученного из метода <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> и включенного в метод <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/>.
		/// </returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Создает объект из представления XML.
		/// </summary>
		/// <param name="reader">Поток <see cref="T:System.Xml.XmlReader"/>, из которого выполняется десериализация объекта.</param>
		public void ReadXml(XmlReader reader)
		{
			ID = Convert.ToInt32(reader.GetAttribute("ID"));
			City = reader.GetAttribute("City");
			Area = reader.GetAttribute("Area");
			Streat = reader.GetAttribute("Streat");
			Name = reader.GetAttribute("Name");
			Info = reader.GetAttribute("Info");
			Lng = Convert.ToDouble(reader.GetAttribute("Lng"));
			Lat = Convert.ToDouble(reader.GetAttribute("Lat"));
			StopsStr = reader.GetAttribute("StopsStr");
			StopNum = reader.GetAttribute("StopNum");
			Info = reader.GetAttribute("Info");
		}

		/// <summary>
		/// Преобразует объект в представление XML.
		/// </summary>
		/// <param name="writer">Поток <see cref="T:System.Xml.XmlWriter"/>, в который выполняется сериализация объекта.</param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString("ID", ID.ToString());
			writer.WriteAttributeString("City", City);
			writer.WriteAttributeString("Area", Area);
			writer.WriteAttributeString("Streat", Streat);
			writer.WriteAttributeString("Name", Name);
			writer.WriteAttributeString("Info", Info);
			writer.WriteAttributeString("Lng", Lng.ToString());
			writer.WriteAttributeString("Lat", Lat.ToString());
			writer.WriteAttributeString("StopsStr", StopsStr);
			writer.WriteAttributeString("StopNum", StopNum);
			writer.WriteAttributeString("Info", Info);
		}

		#endregion
	}
}