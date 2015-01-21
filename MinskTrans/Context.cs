using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MinskTrans.DesctopClient.Annotations;

namespace MinskTrans.DesctopClient
{

	public class Context : INotifyPropertyChanged , IXmlSerializable
	{
		private readonly List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>
		{
			new KeyValuePair<string, string>("stops.txt", @"http://www.minsktrans.by/city/minsk/stops.txt"),
			new KeyValuePair<string, string>("routes.txt", @"http://www.minsktrans.by/city/minsk/routes.txt"),
			new KeyValuePair<string, string>("times.txt", @"http://www.minsktrans.by/city/minsk/times.txt")
		};

		private ObservableCollection<Rout> routs;
		private ObservableCollection<Stop> stops;
		private ObservableCollection<Schedule> times;
		private DateTime lastUpdateDataDateTime;

		public DateTime LastUpdateDataDateTime
		{
			get { return lastUpdateDataDateTime; }
			set
			{
				if (value.Equals(lastUpdateDataDateTime)) return;
				lastUpdateDataDateTime = value;
				OnPropertyChanged();
			}
		}


		public Context()
		{
			Create();
		}


		public ObservableCollection<Schedule> Times
		{
			get { return times; }
			set
			{
				times = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Stop> Stops
		{
			get { return stops; }
			set
			{
				if (Equals(value, stops)) return;
				stops = value;
				OnPropertyChanged();
				OnPropertyChanged("ActualStops");
			}
		}

		public ObservableCollection<Stop> ActualStops
		{
			get { return new ObservableCollection<Stop>(Stops.Where(x => Routs.Any(d => d.Stops.Contains(x)))); }
		}

		public ObservableCollection<Rout> Routs
		{
			get { return routs; }
			set
			{
				if (Equals(value, routs))
					return;
				routs = value;
				OnPropertyChanged();
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public void Create()
		{
			if (!File.Exists(list[0].Key) || !File.Exists(list[1].Key) || !File.Exists(list[2].Key))
				DownloadUpdate();

			Stops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(File.ReadAllText(list[0].Key)));
			Routs = new ObservableCollection<Rout>(ShedulerParser.ParsRout(File.ReadAllText(list[1].Key)));
			Times = new ObservableCollection<Schedule>(ShedulerParser.ParsTime(File.ReadAllText(list[2].Key)));

			foreach (Rout rout in Routs)
			{
				rout.Time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
				if (rout.Time != null)
					rout.Time.Rout = rout;

				rout.Stops = new List<Stop>();
				foreach (int st in rout.RouteStops)
				{
					rout.Stops.Add(Stops.First(x => x.ID == st));
				}
			}
		}

		public void DownloadUpdate()
		{
			using (var client = new WebClient())
			{
				client.DownloadFile(list[0].Value, list[0].Key + ".new");
				client.DownloadFile(list[1].Value, list[1].Key + ".new");
				client.DownloadFile(list[2].Value, list[2].Key + ".new");
			}

			foreach (var keyValuePair in list)
			{
				File.Move(keyValuePair.Key + ".new", keyValuePair.Key);
			}
			
		}

		public bool HaveUpdate()
		{
			return true;
		}

		public void Save()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Context));
			StreamWriter streamWriter = new StreamWriter("data.xml");
			try
			{
				serializer.Serialize(streamWriter, this);
			}
			finally
			{
				streamWriter.Close();
			}
		}

		public void Load()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Context));
			StreamReader streamWriter = new StreamReader("data.xml");
			try
			{
				var obj = serializer.Deserialize(streamWriter);
			}
			finally
			{
				streamWriter.Close();
			}
		}

		async public void UpdateAsync()
		{
			await Task.Run(() =>
			{
				DownloadUpdate();
				Stops = new ObservableCollection<Stop>(ShedulerParser.ParsStops(File.ReadAllText(list[0].Key)));
				Routs = new ObservableCollection<Rout>(ShedulerParser.ParsRout(File.ReadAllText(list[1].Key)));
				Times = new ObservableCollection<Schedule>(ShedulerParser.ParsTime(File.ReadAllText(list[2].Key)));

				foreach (Rout rout in Routs)
				{
					rout.Time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
					if (rout.Time != null)
						rout.Time.Rout = rout;

					rout.Stops = new List<Stop>();
					foreach (int st in rout.RouteStops)
					{
						rout.Stops.Add(Stops.First(x => x.ID == st));
					}
				}

				LastUpdateDataDateTime = DateTime.Now;
			});
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
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
			reader.MoveToAttribute("LastUpdateTime");
			LastUpdateDataDateTime = reader.ReadContentAsDateTime();
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString("LastUpdateTime", LastUpdateDataDateTime.ToString());
			writer.WriteStartAttribute("Routs");
			foreach (var rout in Routs)
			{
				rout.WriteXml(writer);
			}
			writer.WriteEndAttribute();
		}

		#endregion
	}
}