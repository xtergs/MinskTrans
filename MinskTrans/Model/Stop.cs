using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public class Stop : StopBase, IXmlSerializable
	{
		
		protected List<Rout> routs;

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

			Routs = new List<Rout>();
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
			City = GetStr().Trim();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Area = GetStr().Trim();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Streat = GetStr().Trim();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Name = GetStr().Trim();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Info = GetStr().Trim();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Lng = double.Parse(GetStr()) / 100000;

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			Lat = double.Parse(GetStr()) / 100000;

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			StopsStr = GetStr().Trim();

			//indexStart = indexEnd + 1;
			//indexEnd = str.IndexOf(sym, indexStart);
			StopNum = GetStr().Trim();

			Stops = new List<Stop>();
			Routs = new List<Rout>();
		}

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


		public List<Stop> Stops
		{
			get
			{
				if (stops == null)
					stops = new List<Stop>();
				return stops;
			}
			set { stops = value; }
		}

		public virtual List<Rout> Routs
		{
			get
			{
				if (routs == null)
					routs = new List<Rout>();
				return routs;
			}
			set { routs = value; }
		}

		public IEnumerable<IGrouping<TransportType, Rout>> GroupedTypeRouts
		{
			get { return Routs.GroupBy(x => x.Transport); }
		}

		public IEnumerable<Rout> TrolRouts
		{
			get { return Routs.Where(x => x.Transport == TransportType.Trol); }
		}
		public IEnumerable<Rout> BusRouts
		{
			get { return Routs.Where(x => x.Transport == TransportType.Bus); }
		}
		public IEnumerable<Rout> TramRouts
		{
			get { return Routs.Where(x => x.Transport == TransportType.Tram); }
		}

		public IEnumerable<Rout> MetroRouts
		{
			get { return Routs.Where(x => x.Transport == TransportType.Metro); }
		}

		public ObservableCollection<string> TrolRoutsNum
		{
			get { return new ObservableCollection<string>(TrolRouts.Select(x=>x.RouteNum).Distinct()); }
		}

		public ObservableCollection<string> BusRoutsNum
		{
			get { return new ObservableCollection<string>( BusRouts.Select(x => x.RouteNum).Distinct()); }
		}
		public ObservableCollection<string> TramRoutsNum
		{
			get { return new ObservableCollection<string>(TramRouts.Select(x => x.RouteNum).Distinct()); }
		}

		public List<string> MetroRoutsNum
		{
			get { return MetroRouts.Select(x => x.RouteNum).Distinct().ToList(); }
		} 


		public IEnumerable<Stop> Directions
		{
			get { return Routs.Select(x => x.DestinationStop); }
		}

		public IEnumerable<string> DirectionsString
		{
			get { return Directions.Select(x => x.Name).Distinct(); }
		}

		
		#region Overrides of BaseModel

		protected string GetStr()
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