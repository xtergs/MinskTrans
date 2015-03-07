using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
#if !WINDOWS_PHONE_APP && !WINDOWS_AP
using System.Runtime.Serialization.Formatters.Binary;
using MinskTrans.DesctopClient.Annotations;
using GalaSoft.MvvmLight.CommandWpf;
#else
using GalaSoft.MvvmLight.Command;
using MinskTrans.Universal.Annotations;

#endif
using MinskTrans.DesctopClient.Model;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal.Model;


namespace MinskTrans.DesctopClient
{
#if !WINDOWS_PHONE_APP && !WINDOWS_APP
	[Serializable]
#endif
	public abstract class Context : INotifyPropertyChanged , IXmlSerializable, IContext
	{
		protected readonly List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>
		{
			new KeyValuePair<string, string>("stops.txt", @"http://www.minsktrans.by/city/minsk/stops.txt"),
			new KeyValuePair<string, string>("routes.txt", @"http://www.minsktrans.by/city/minsk/routes.txt"),
			new KeyValuePair<string, string>("times.txt", @"http://www.minsktrans.by/city/minsk/times.txt")
		};

		private ObservableCollection<Rout> routs;
		private ObservableCollection<Stop> stops;
		private ObservableCollection<Schedule> times;
		private ObservableCollection<GroupStop> groups;
		private ObservableCollection<RoutWithDestinations> favouriteRouts;
		private ObservableCollection<Stop> favouriteStops;
		private DateTime lastUpdateDataDateTime;

		public DateTime LastUpdateDataDateTime
		{
			get { return lastUpdateDataDateTime; }
			set
			{
				lastUpdateDataDateTime = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<RoutWithDestinations> FavouriteRouts
		{
			get
			{
				if (favouriteRouts == null)
					favouriteRouts = new ObservableCollection<RoutWithDestinations>();
				return favouriteRouts;
			}
			set
			{
				favouriteRouts = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Stop> FavouriteStops
		{
			get
			{
				if (favouriteStops == null)
					favouriteStops = new ObservableCollection<Stop>();
				return favouriteStops;
			}
			set
			{
				favouriteStops = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<GroupStop> Groups
		{
			get
			{
				if (groups == null)
					Groups = new ObservableCollection<GroupStop>();
				return groups;
			}
			set
			{
				groups = value;
				groups.CollectionChanged += (sender, args) =>
				{
					OnPropertyChanged("Groups.Count");
					var s = Groups.Count;
				};
				OnPropertyChanged();

			}
		}


		public Context()
		{
			Create();
		}

		public void Inicialize(Context cont)
		{
			Routs = cont.Routs;
			Stops = cont.Stops;
			ActualStops = cont.ActualStops;
			Times = cont.Times;
			FavouriteRouts = cont.FavouriteRouts;
			FavouriteStops = cont.FavouriteStops;
			LastUpdateDataDateTime = cont.LastUpdateDataDateTime;
			Groups = cont.Groups;
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
				actualStops = null;
				//ActualStops = new ObservableCollection<Stop>(value.AsParallel().Where(x => Routs != null && Routs.AsParallel().Any(d => d.Stops.Contains(x))));
				OnPropertyChanged();
				OnPropertyChanged("ActualStops");
			}
		}

		public ObservableCollection<Stop> ActualStops
		{
			get
			{
				if ((actualStops == null || actualStops.Count == 0) && Stops != null)
					actualStops =
						new ObservableCollection<Stop>(Stops.Where(x => Routs != null && Routs.Any(d => d.Stops.Any(s => s.ID == x.ID))));
				return actualStops;
			}

			private set
			{
				actualStops = value;
				OnPropertyChanged();
			}
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

		public bool RoutsHaveStopId(int stopId)
		{
			return Routs.AsParallel().Any(x => x.RouteStops.Contains(stopId));
		}


		public event PropertyChangedEventHandler PropertyChanged;

		public  abstract void Create(bool AutoUpdate = true);

		
		protected abstract Task<bool> FileExists(string file);

		protected abstract Task FileDelete(string file);

		protected abstract Task FileMove(string oldFile, string newFile);

		protected abstract Task<string> FileReadAllText(string file);

		public abstract Task DownloadUpdate();

		

		protected ObservableCollection<Rout> newRoutes;
		protected ObservableCollection<Stop> newStops;
		protected ObservableCollection<Schedule> newSchedule;

		public abstract Task<bool> HaveUpdate(string fileStops, string fileRouts, string fileTimes, bool checkUpdate);

		public async Task ApplyUpdate()
		{
			OnApplyUpdateStarted();
			try
			{
#if DEBUG
				Stopwatch watch1 = new Stopwatch();
				watch1.Start();
#endif

				//Parallel.ForEach(list, async keyValuePair =>
				if (await FileExists(list[0].Key + ".new") && 
					await FileExists(list[1].Key + ".new") && 
					await FileExists(list[2].Key + ".new"))
					foreach (var keyValuePair in list)
					{

						await FileDelete(keyValuePair.Key + ".old");
						await FileMove(keyValuePair.Key, keyValuePair.Key + ".old");
						await FileMove(keyValuePair.Key + ".new", keyValuePair.Key);

					}

#if DEBUG
				watch1.Stop();
#endif
				//Stops.Clear();
				//Routs.Clear();
				//Times.Clear();
				//Parallel.ForEach(newStops, stop => Stops.Add(stop));
				//Parallel.ForEach(newRoutes, rout => Routs.Add(rout));
				//Parallel.ForEach(newSchedule, time => Times.Add(time));

#if DEBUG
				watch1.Reset();
				watch1.Start();
#endif

				Stops =newStops;
				Routs = newRoutes;
				Times = newSchedule;
#if DEBUG
				watch1.Stop();
				watch1.Reset();
				watch1.Start();
#endif
				Connect(Routs, Stops, Times);

#if DEBUG
				watch1.Stop();
#endif

				LastUpdateDataDateTime = DateTime.UtcNow;

				newStops = null;
				newRoutes = null;
				newSchedule = null;
			}
			catch (Exception e)
			{
				Debug.WriteLine("Apply update: " + e.Message);
				//OnLogMessage("Apply update: " + e.Message);
				throw new Exception(e.Message, e.InnerException);
			}

			//ActualStops = ;

			AllPropertiesChanged();

			OnApplyUpdateEnded();
		}

		public abstract Task Save();

		public abstract Task Load();

		public void AllPropertiesChanged()
		{
			OnPropertyChanged("ActualStops");
			OnPropertyChanged("Stops");
			OnPropertyChanged("Routs");
			OnPropertyChanged("Times");
			OnPropertyChanged("FavouriteRouts");
			OnPropertyChanged("FavouriteStops");
			OnPropertyChanged("Groups");
		}

		
		static protected void Connect([NotNull] IEnumerable<Rout> routsl, [NotNull] IEnumerable<Stop> stopsl,
			[NotNull] IEnumerable<Schedule> timesl)
		{
			if (routsl == null) throw new ArgumentNullException("routsl");
			if (stopsl == null) throw new ArgumentNullException("stopsl");
			if (timesl == null) throw new ArgumentNullException("timesl");
#if DEBUG
			Stopwatch watch1 = new Stopwatch();
			watch1.Start();
#endif
			Parallel.ForEach(routsl, rout =>
			{
				rout.Time = timesl.FirstOrDefault(x =>
				{
					if (x == null)
						return false;
					return x.RoutId == rout.RoutId;
				});
				if (rout.Time != null)
					rout.Time.Rout = rout;

#if DEBUG
				Stopwatch watch = new Stopwatch();
				watch.Start();
#endif
				rout.Stops = rout.RouteStops.Join(stopsl, i => i, stop => stop.ID, (i, stop) =>
				{
					if (stop.Routs == null)
						stop.Routs = new List<Rout>(10);
					stop.Routs.Add(rout);
					return stop;
				}).ToList();


#if DEBUG
				watch.Stop();
				watch.Restart();
#endif
			});


#if DEBUG
			watch1.Stop();
			watch1.Restart();
#endif

		}

		//async public void Update()
		//{
		//	await DownloadUpdate();
		//	if (await HaveUpdate())
		//		ApplyUpdate();
		//}

		async public virtual Task UpdateAsync()
		{
			//TODO
			//throw new NotImplementedException();

			OnUpdateStarted();
			await DownloadUpdate();
			if (await HaveUpdate(list[0].Key + ".new", list[1].Key + ".new", list[2].Key + ".new", true))
				await ApplyUpdate();
			OnUpdateEnded();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Event
		public delegate void EmptyDelegate(object sender, EventArgs args);

		public event EmptyDelegate DataBaseDownloadStarted;
		public event EmptyDelegate DataBaseDownloadEnded;
		public event EmptyDelegate ApplyUpdateStarted;
		public event EmptyDelegate ApplyUpdateEnded;
		public event LogDelegate LogMessage;
		public event EmptyDelegate ErrorDownloading;
		public event EmptyDelegate UpdateStarted;
		public event EmptyDelegate UpdateEnded;
		public event EmptyDelegate LoadStarted;
		public event EmptyDelegate LoadEnded;
		public event ErrorLoadingDelegate ErrorLoading;

		#region Invokators
		protected virtual void OnDataBaseDownloadStarted()
		{
			var handler = DataBaseDownloadStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnDataBaseDownloadEnded()
		{
			var handler = DataBaseDownloadEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnApplyUpdateStarted()
		{
			var handler = ApplyUpdateStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnApplyUpdateEnded()
		{
			var handler = ApplyUpdateEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		protected virtual void OnLogMessage(string message)
		{
			var handler = LogMessage;
			if (handler != null) handler(this, new LogDelegateArgs(){Message = message});
		}
		protected virtual void OnErrorDownloading()
		{
			var handler = ErrorDownloading;
			if (handler != null) handler(this, EventArgs.Empty);
		}
		#endregion

		#endregion

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

		protected internal ObservableCollection<int> FavouriteRoutsIds;
		protected internal ObservableCollection<int> FavouriteStopsIds;
		protected internal ObservableCollection<GroupStopId> GroupsStopIds;
		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
		public void ReadXml(XmlReader reader)
		{
			try
			{

			var node = XDocument.Load(reader);
			var document = (XDocument)node;

			//document.Root.Attribute("LastUpdateTime").Value;

				LastUpdateDataDateTime = (DateTime) document.Root.Attribute("LastUpdateTime");

				FavouriteRoutsIds = new ObservableCollection<int>(document.Elements("FavouritRouts").Select(x => (int) x));
				FavouriteStopsIds = new ObservableCollection<int>(document.Elements("FavouriteStops").Select(x => (int)x));

				GroupsStopIds = new ObservableCollection<GroupStopId>(document.Element("Groups").Elements("Group").Select(XGroup =>
				{
					var groupid = new GroupStopId();
					groupid.Name = (string)XGroup.Attribute("Name");
					groupid.StopID =XGroup.Elements("ID").Select(id=>(int)id).ToList();
					return groupid;
				}));
			}
			catch (Exception e)
			{

				return;
			}

			//reader.ReadStartElement("ContextDesctop");
			//LastUpdateDataDateTime = Convert.ToDateTime(reader.GetAttribute("LastUpdateTime"));
			////Routs = new ObservableCollection<Rout>();

			//reader.ReadStartElement();
			//int count = Convert.ToInt32(reader.GetAttribute("Count"));
			//FavouriteRoutsIds = new ObservableCollection<int>();
			//for (int i = 0; i < count; i ++)
			//{
			//	reader.ReadStartElement();
			//	FavouriteRoutsIds.Add(int.Parse(reader.GetAttribute("id")));
				
			//	//reader.ReadStartElement("Rout");
			//	//var rout = new Rout();
			//	//rout.ReadXml(reader);
			//	//Routs.Add(rout);
			//	//reader.ReadEndElement();
			//	if (!reader.IsEmptyElement)
			//		reader.ReadEndElement();
				
			//}

			//if (!reader.IsEmptyElement)
			//	reader.ReadEndElement();
			//else
			//{
			//	reader.ReadStartElement();
			//	if (reader.NodeType == XmlNodeType.EndElement)
			//		reader.ReadEndElement();
			//}

			////reader.ReadStartElement();
			//count = Convert.ToInt32(reader.GetAttribute("Count"));
			//FavouriteStopsIds = new ObservableCollection<int>();
			//for (int i = 0; i < count; i++)
			//{
			//	reader.ReadStartElement();
			//	FavouriteStopsIds.Add(int.Parse(reader.GetAttribute("id")));

			//	if (!reader.IsEmptyElement)
			//		reader.ReadEndElement();

			//}
			//if (!reader.IsEmptyElement)
			//	reader.ReadEndElement();
			//else
			//{
			//	reader.ReadStartElement();
			//	if (reader.NodeType == XmlNodeType.EndElement)
			//		reader.ReadEndElement();				
			//}

			////reader.ReadStartElement();
			//count = Convert.ToInt32(reader.GetAttribute("Count"));
			//GroupsStopIds = new ObservableCollection<GroupStopId>();
			//for (int i = 0; i < count; i++)
			//{
			//	var group = new GroupStopId();
			//	reader.ReadStartElement();
			//	group.Name = reader.GetAttribute("Name");
			//	int countt = int.Parse(reader.GetAttribute("Count"));
			//	group.StopID = new List<int>(countt);
			//	for (int j = 0; j < countt; j++)
			//	{
			//		reader.ReadStartElement();
			//		group.StopID.Add(int.Parse(reader.GetAttribute("id")));
			//		if (!reader.IsEmptyElement)
			//			reader.ReadEndElement();

			//	}
			//	GroupsStopIds.Add(group);

			//	if (!reader.IsEmptyElement)
			//		reader.ReadEndElement();

			//}
			//reader.ReadEndElement();
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
		public void WriteXml(XmlWriter writer)
		{
			XElement element = new XElement("Context");
			XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), element);
			//document.Declaration = new XDeclaration("1.0", "UTF", "yes");

			element.SetAttributeValue("LastUpdateTime", LastUpdateDataDateTime);
			element.SetAttributeValue("V", 1);


			XElement el = new XElement("FavouritRouts", FavouriteRouts.Select(x => new XElement("ID", x.Rout.RoutId)));

			element.Add(el);

			el = new XElement("FavouriteStops", FavouriteStops.Select(x => new XElement("ID", x.ID)));

			element.Add(el);

			el = new XElement("Groups", Groups.Select(x =>
			{
				var elem = new XElement("Group", x.Stops.Select(y => new XElement("ID", y.ID)));
				elem.SetAttributeValue("Name", x.Name);
				return elem;
			}));
			

			element.Add(el);

			document.Save(writer);

			//writer.WriteAttributeString("LastUpdateTime", LastUpdateDataDateTime.ToString());

			//writer.WriteStartElement("FavouritRouts");
			//writer.WriteAttributeString("Count", FavouriteRouts.Count.ToString());
			//foreach (var rout in FavouriteRouts)
			//{
			//	writer.WriteStartElement("Rout");
			//	writer.WriteAttributeString("id", rout.Rout.RoutId.ToString());
			//	writer.WriteEndElement();
			//}
			//writer.WriteEndElement();

			//writer.WriteStartElement("FavouriteStops");
			//writer.WriteAttributeString("Count", FavouriteStops.Count.ToString());
			//foreach (var stop in FavouriteStops)
			//{
			//	writer.WriteStartElement("Stop");
			//	writer.WriteAttributeString("id", stop.ID.ToString());
			//	writer.WriteEndElement();
			//}
			//writer.WriteEndElement();

			//writer.WriteStartElement("Groups");
			//writer.WriteAttributeString("Count", Groups.Count.ToString());
			//foreach (var group in Groups)
			//{
			//	writer.WriteStartElement("Group");
			//	writer.WriteAttributeString("Name", group.Name);
			//	writer.WriteAttributeString("Count", group.Stops.Count.ToString());

			//	foreach (var stop in group.Stops)
			//	{
			//		writer.WriteStartElement("Stop");
			//		writer.WriteAttributeString("id", stop.ID.ToString());
			//		writer.WriteEndElement();
			//	}
			//	group.WriteXml(writer);
			//	writer.WriteEndElement();
			//}
			//writer.WriteEndElement();
		}

		#endregion

		#region commands

		private bool updating = false;
		private ObservableCollection<Stop> actualStops;

		public RelayCommand UpdateDataCommand
		{
			get
			{
				return new RelayCommand(async () =>
				{
					updating = true;
					UpdateDataCommand.RaiseCanExecuteChanged();
					await UpdateAsync();
					updating = false;
					UpdateDataCommand.RaiseCanExecuteChanged();
				}, ()=>!updating);
			}
		}

		public IEnumerable<string> GetDestinations(Rout rout)
		{
			return new List<string>();
		}

		public RelayCommand<RoutWithDestinations> AddFavouriteRoutCommand
		{
			get { return new RelayCommand<RoutWithDestinations>(x => FavouriteRouts.Add(x), p => p != null && !FavouriteRouts.Contains(p)); }
		}

		public RelayCommand<Stop> AddFavouriteSopCommand
		{
			get { return new RelayCommand<Stop>(x => FavouriteStops.Add(x), p => p != null && FavouriteStops != null && !FavouriteStops.Contains(p)); }
		}
		public RelayCommand<RoutWithDestinations> RemoveFavouriteRoutCommand
		{
			get { return new RelayCommand<RoutWithDestinations>(x => FavouriteRouts.Remove(x), p => p != null && FavouriteRouts.Contains(p)); }
		}

		public RelayCommand<Stop> RemoveFavouriteSopCommand
		{
			get { return new RelayCommand<Stop>(x => FavouriteStops.Remove(x), p => p != null && FavouriteStops.Contains(p)); }
		}
		#endregion

		protected virtual void OnUpdateStarted()
		{
			var handler = UpdateStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnUpdateEnded()
		{
			var handler = UpdateEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		public bool IsFavouriteStop(Stop stop)
		{
			return FavouriteStops.Contains(stop);
		}

		public bool IsFavouriteRout(RoutWithDestinations rout)
		{
			return FavouriteRouts.Contains(rout);
		}

		public RelayCommand<Stop> AddRemoveFavouriteStop
		{
			get { return new RelayCommand<Stop>(x =>
			{
				if (IsFavouriteStop(x))
					RemoveFavouriteSopCommand.Execute(x);
				else
					AddFavouriteSopCommand.Execute(x);

			}
				);}
		}

		public RelayCommand<RoutWithDestinations> AddRemoveFavouriteRout
		{
			get
			{
				return new RelayCommand<RoutWithDestinations>(x =>
				{
					if (IsFavouriteRout(x))
						RemoveFavouriteRoutCommand.Execute(x);
					else
						AddFavouriteRoutCommand.Execute(x);

				}
					);
			}
		}

		public RelayCommand<string> CreateGroup
		{
			get
			{
				return new RelayCommand<string>(x=>Groups.Add(new GroupStop(){Name=x}), p=>!string.IsNullOrWhiteSpace(p));
			}
		}

		public RelayCommand<GroupStop> DeleteGroups
		{
			get
			{
				return new RelayCommand<GroupStop>(x =>
				{
					if (x != null)
						Groups.Remove(x);
						OnPropertyChanged("Groups");
				});
			}
		}

		public event Show ShowStop;
		public event Show ShowRoute;
		public delegate void Show(object sender, ShowArgs args);

		public RelayCommand<Stop> ShowStopMap
		{
			get { return new RelayCommand<Stop>((x) => OnShowStop(new ShowArgs() { SelectedStop = x }), (x) => x != null); }
		}

		public RelayCommand<Rout> ShowRouteMap
		{
			get { return new RelayCommand<Rout>((x) => OnShowRoute(new ShowArgs() { SelectedRoute = x }), (x) => x != null); }
		}

		protected virtual void OnShowStop(ShowArgs args)
		{
			var handler = ShowStop;
			if (handler != null) handler(this, args);
		}

		protected virtual void OnShowRoute(ShowArgs args)
		{
			var handler = ShowRoute;
			if (handler != null) handler(this, args);
		}

		protected virtual void OnLoadStarted()
		{
			var handler = LoadStarted;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnLoadEnded()
		{
			var handler = LoadEnded;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnErrorLoading(ErrorLoadingDelegateArgs args)
		{
			var handler = ErrorLoading;
			if (handler != null) handler(this, args);
		}
	}

	public delegate void ErrorLoadingDelegate(object sender, ErrorLoadingDelegateArgs args);

	public class ErrorLoadingDelegateArgs:EventArgs
	{
		public enum Errors
		{
			NoFileToDeserialize,
			NoSourceFiles
		}

		public Errors Error { get; set; }
	}

	public delegate void LogDelegate(object sender, LogDelegateArgs args);
}