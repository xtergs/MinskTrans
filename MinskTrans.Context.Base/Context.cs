using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Net.Base;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using Newtonsoft.Json;

namespace MinskTrans.Context
{
    class TimeTableSettings
    {
        public int SettingsVersion { get; set; }
		public DateTime TimeTableUpdateTimeUtc { get; set; }
        public TimeTable TimeTable { get; set; }
    }
    public class Context : IContext
    {
/*
		private ContextFileSettings settings;
*/
        protected Dictionary<int, uint> counterViewStops = new Dictionary<int, uint>();
        private readonly FilePathsSettings files;


        public void IncrementCounter(Stop stop)
        {
            if (!counterViewStops.Keys.Contains(stop.ID))
            {
                counterViewStops.Add(stop.ID, 1);
            }
            else
            {
                counterViewStops[stop.ID]++;
            }
        }

        public uint GetCounter(Stop stop)
        {
            if (counterViewStops != null)
                return counterViewStops.Keys.Contains(stop.ID) ? counterViewStops[stop.ID] : 0;
            return 0;
        }


        private int variantLoad;

        public int VariantLoad
        {
            get { return variantLoad; }
            set { variantLoad = value; }
        }

        private readonly FileHelperBase fileHelper;
        protected readonly InternetHelperBase internetHelper;

        public Context(FileHelperBase helper, InternetHelperBase internetHelper, FilePathsSettings files)
        {
            fileHelper = helper;
            this.internetHelper = internetHelper;
            this.files = files;
            favouriteRouts = new ObservableCollection<int>();
            favouriteStops = new ObservableCollection<int>();
            Groups = new ObservableCollection<GroupStop>();
            ;
        }

        public void Inicialize(IContext cont)
        {
            Routs = cont.Routs;
            Stops = cont.Stops;
            Times = cont.Times;
            Groups = cont.Groups;
        }


        public virtual void Create(bool AutoUpdate = true)
        {
            favouriteRouts = new ObservableCollection<int>();
            favouriteStops = new ObservableCollection<int>();
            Groups = new ObservableCollection<GroupStop>();
        }

#pragma warning disable 1998
        public async Task ApplyUpdate(IEnumerable<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
#pragma warning restore 1998
        {
            OnApplyUpdateStarted();
            try
            {
                Stops = newStops.ToArray();
                Routs = newRoutes.ToArray();
                Times = newSchedule.ToArray();

                await Save(true);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Apply update: " + e.Message);
#if BETA
				Logger.Log("ApplyUpdate exception").WriteLine(e.Message).WriteLine(e.StackTrace).SaveToFile();
#endif
                throw;
            }
            AllPropertiesChanged();
            OnApplyUpdateEnded();
        }

#pragma warning disable 1998
        public virtual async Task<bool> HaveUpdate(IList<Rout> newRoutes, IList<Stop> newStops,
            IList<Schedule> newSchedule)
#pragma warning restore 1998
        {
            Connect(newRoutes, newStops, newSchedule, VariantLoad);
            foreach (var toRemoveStop in newStops.Where(stop => !stop.Routs.Any()).ToList())
            {
                newStops.Remove(toRemoveStop);
            }
            foreach (var toRemoveRout in newRoutes.Where(rout => !rout.Stops.Any()).ToList())
            {
                newRoutes.Remove(toRemoveRout);
            }
            return NeedUpdate(newRoutes, newStops, newSchedule);
        }

        protected virtual async Task SaveFavourite(TypeFolder storage)
        {
            var favouriteString = JsonConvert.SerializeObject(new
            {
                Routs = FavouriteRouts.Select(x => x.RoutId).ToList(),
                Stops = favouriteStops,
                Groups = Groups.Select(x => new
                {
                    Name = x.Name,
                    IDs = x.Stops.Select(stop => stop.ID)
                })
            });
            await
                (await
                    FileHelper.WriteTextAsync(files.FavouriteFile.Folder, files.FavouriteFile.TempFileName,
                        favouriteString))
                    .SafeMoveTo(files.FavouriteFile.FileName);
        }

        protected async Task SaveStatistics(JsonSerializerSettings jsonSettings, TypeFolder storage)
        {
            string counter = JsonConvert.SerializeObject(counterViewStops, jsonSettings);
            await
                (await FileHelper.WriteTextAsync(files.StatisticFile.Folder, files.StatisticFile.TempFileName, counter))
                    .SafeMoveTo(files.StatisticFile.FileName);
        }

        public virtual async Task Save(bool saveAllDb = true)
        {
            try
            {
                await SaveFavourite(files.FavouriteFile.Folder);

                var jsonSettings = new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};

                await SaveStatistics(jsonSettings, files.StatisticFile.Folder);
                if (saveAllDb)
                {
                    TimeTable timeTable = new TimeTable() {Routs = Routs, Stops = Stops, Time = Times};
                    TimeTableSettings toSave = new TimeTableSettings()
                    {
                        TimeTable = timeTable, SettingsVersion = 3, TimeTableUpdateTimeUtc = UpdateDateTimeUtc
                    };
                    var strTimeTable = JsonConvert.SerializeObject(toSave);
                    var file = files.TimeTableAllFile;
                    await
                            (await
                                FileHelper.WriteTextAsync(file.Folder, file.TempFileName, strTimeTable))
                                .SafeMoveTo(
                                    file.FileName);
                    #region oldSaving
                    //await Task.WhenAll(Task.Run(async () =>
                    //{
                    //    string routs = JsonConvert.SerializeObject(Routs, jsonSettings);
                    //    await
                    //        (await
                    //            FileHelper.WriteTextAsync(files.RouteFile.Folder, files.RouteFile.TempFileName, routs))
                    //            .SafeMoveTo(
                    //                files.RouteFile.FileName);
                    //}),
                    //    Task.Run(async () =>
                    //    {
                    //        string stopsString = JsonConvert.SerializeObject(ActualStops, jsonSettings);

                    //        await
                    //            (await
                    //                FileHelper.WriteTextAsync(files.StopsFile.Folder, files.StopsFile.TempFileName,
                    //                    stopsString))
                    //                .SafeMoveTo(files.StopsFile.FileName);
                    //    }), Task.Run(async () =>
                    //    {
                    //        string routs = JsonConvert.SerializeObject(Times, jsonSettings);

                    //        await
                    //            (await
                    //                FileHelper.WriteTextAsync(files.TimeFile.Folder, files.TimeFile.TempFileName, routs))
                    //                .SafeMoveTo(
                    //                    files.TimeFile.FileName);
                    //    }));
                    #endregion
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Exception in UniversalContext.Save");
                throw;
            }
        }

        public virtual async Task Load(LoadType type = LoadType.LoadAll)
        {
            Debug.WriteLine($"{DateTime.UtcNow} UniversalContext.Load started");
            Debug.WriteLine($"{DateTime.UtcNow}UniversalContext LoadSourceData started");

            OnLoadStarted();

            IList<int> tpFavouriteStops = null;
            ObservableCollection<int> tpFavouriteRouts = null;
            ObservableCollection<GroupStop> tpGroups = null;

            TimeTableSettings timeTable = null;
            try
            {
                if (type.HasFlag(LoadType.LoadFavourite))
                    try
                    {
                        string routs =
                            await FileHelper.ReadAllTextAsync(files.StatisticFile.Folder, files.StatisticFile.FileName);
                        counterViewStops = JsonConvert.DeserializeObject<Dictionary<int, uint>>(routs);
                    }

                    catch (FileNotFoundException)
                    {
                        counterViewStops = new Dictionary<int, uint>();
                    }
                if (type.HasFlag(LoadType.LoadDB))
                {
                    timeTable = await LoadTimeTable();
                    if (timeTable == null)
                        timeTable =await LoadTimeTableV2();
                }
            }
            catch (TaskCanceledException)
            {
                OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles});

                return;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Context.Load: " + e.Message);
                throw;
            }

            if (timeTable == null || timeTable.TimeTable.Routs == null || timeTable.TimeTable.Stops == null
            || timeTable.TimeTable.Time == null)
            {
                //CleanTp();
                OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles});
                return;
            }

            Debug.WriteLine($"{DateTime.UtcNow}UniversalContext LoadSourceData ended");
            if (type.HasFlag(LoadType.LoadDB))
            {
                Connect(timeTable.TimeTable, VariantLoad);


                Routs = timeTable.TimeTable.Routs.ToArray();
                Stops = timeTable.TimeTable.Stops.ToArray();
                Times = timeTable.TimeTable.Time.ToArray();
            }
            Debug.WriteLine($"{DateTime.UtcNow}UniversalContext loadfavourite started");
            if (type.HasFlag(LoadType.LoadFavourite) )
            {
                try
                {
                    var textFavourite =
                        await FileHelper.ReadAllTextAsync(files.FavouriteFile.Folder, files.FavouriteFile.FileName);
                    try
                    {
                        var desFavourite = JsonConvert.DeserializeAnonymousType(textFavourite, new
                        {
                            Routs = FavouriteRouts.Select(x => x.RoutId).ToList(),
                            Stops = favouriteStops,
                            Groups = Groups.Select(x => new
                            {
                                Name = x.Name,
                                IDs = x.Stops.Select(stop => stop.ID)
                            })
                        });

                        if (desFavourite.Routs != null)
                        {
                            var temp1 = desFavourite.Routs.ToList();
                            tpFavouriteRouts = new ObservableCollection<int>(temp1);
                            //desFavourite.Routs = null;
                        }

                        if (desFavourite.Stops != null)
                        {
                            tpFavouriteStops = desFavourite.Stops;
                            //FavouriteStopsIds = null;
                        }
                        if (desFavourite.Groups != null)
                        {
                            tpGroups =
                                new ObservableCollection<GroupStop>(desFavourite.Groups.Select(x => new GroupStop()
                                {
                                    Name = x.Name,
                                    Stops =
                                        new List<Stop>(timeTable.TimeTable.Stops.Join(x.IDs, stop => stop.ID, i => i, (stop, i) => stop))
                                }));
                        }
                    }
                    catch (JsonReaderException)
                    {
                        ReadXml(textFavourite);

                        if (FavouriteRoutsIds != null)
                        {
                            var temp1 = FavouriteRoutsIds.ToList();
                            tpFavouriteRouts = new ObservableCollection<int>(temp1);
                            FavouriteRoutsIds = null;
                        }

                        if (FavouriteStopsIds != null)
                        {
                            tpFavouriteStops = FavouriteStopsIds;
                            FavouriteStopsIds = null;
                        }
                        if (GroupsStopIds != null)
                        {
                            tpGroups = new ObservableCollection<GroupStop>(GroupsStopIds.Select(x => new GroupStop()
                            {
                                Name = x.Name,
                                Stops =
                                    new List<Stop>(timeTable.TimeTable.Stops.Join(x.StopID, stop => stop.ID, i => i, (stop, i) => stop))
                            }));
                            GroupsStopIds = null;
                        }
                    }
                }
                catch (FileNotFoundException e)
                {
                    Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
                    throw;
                }
            }
            else
            {
                tpFavouriteRouts = new ObservableCollection<int>();
                tpFavouriteStops = new ObservableCollection<int>();
                tpGroups = new ObservableCollection<GroupStop>();
            }

            //}
            try
            {
                favouriteRouts = tpFavouriteRouts;
                favouriteStops = tpFavouriteStops;
                Groups = tpGroups;
                Debug.WriteLine($"{DateTime.UtcNow}UniversalContext loadfavourite ended");

                OnLoadEnded();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Load exception, set favourite " + e.ToString());

                throw;
            }
            Debug.WriteLine($"{DateTime.UtcNow}UniversalContext.Load ended");

        }

        async Task<TimeTableSettings> LoadTimeTable()
        {

            try
            {
                TimeTableSettings timeTable;
                var serializer = new JsonSerializer();

                using (var sr = new StreamReader(await FileHelper.OpenStream(files.TimeTableAllFile.Folder, files.TimeTableAllFile.FileName)))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    timeTable =  serializer.Deserialize<TimeTableSettings>(jsonTextReader);
                }
                return timeTable;
                //string strTimeTable =
                //    await FileHelper.ReadAllTextAsync(files.TimeTableAllFile.Folder, files.TimeTableAllFile.FileName);
                //timeTable = JsonConvert.DeserializeObject<TimeTableSettings>(strTimeTable);
            }
            catch (FileNotFoundException e)
            {
                throw new TaskCanceledException(e.Message, e);
            }
        }

        async Task<TimeTableSettings> LoadTimeTableV2()
        {
            TimeTableSettings timeTable = new TimeTableSettings();
            IList<Rout> tpRouts = null;
            IList<Stop> tpStops = null;
            IList<Schedule> tpTimes = null;
            await Task.WhenAll(
                        Task.Run(async () =>
                        {
                            try
                            {
                                var routs =
                                    await FileHelper.ReadAllTextAsync(files.RouteFile.Folder, files.RouteFile.FileName);
                                tpRouts = JsonConvert.DeserializeObject<List<Rout>>(routs);
                            }

                            catch (FileNotFoundException e)
                            {
                                throw new TaskCanceledException(e.Message, e);
                            }
                        }), Task.Run(async () =>
                        {
                            try
                            {
                                string stops =
                                    await FileHelper.ReadAllTextAsync(files.StopsFile.Folder, files.StopsFile.FileName);
                                tpStops = JsonConvert.DeserializeObject<List<Stop>>(stops);
                            }
                            catch (FileNotFoundException e)
                            {
                                throw new TaskCanceledException(e.Message, e);
                            }
                        }), Task.Run(async () =>
                        {
                            try
                            {
                                string times =
                                    await FileHelper.ReadAllTextAsync(files.TimeFile.Folder, files.TimeFile.FileName);

                                tpTimes = JsonConvert.DeserializeObject<List<Schedule>>(times);
                            }
                            catch (FileNotFoundException e)
                            {
                                throw new TaskCanceledException(e.Message, e);
                            }
                        }));
            timeTable.TimeTable = new TimeTable() {Routs = tpRouts, Stops = tpStops, Time = tpTimes};
            return timeTable;
        }

        public virtual async Task Recover()
        {

            await FileHelper.DeleteFile(files.RouteFile.Folder, files.RouteFile.FileName);
            await FileHelper.DeleteFile(files.StopsFile.Folder, files.StopsFile.FileName);
            await FileHelper.DeleteFile(files.TimeFile.Folder, files.TimeFile.FileName);

        }


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

        protected static void Connect( TimeTable timeTable, int variantLoad)
        {
            Connect(timeTable.Routs, timeTable.Stops, timeTable.Time, variantLoad);
        }

        protected static void Connect( /*[NotNull]*/ IList<Rout> routsl, /* [NotNull]*/ IList<Stop> stopsl,
            /*[NotNull]*/ IList<Schedule> timesl, int variantLoad)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            if (routsl == null) throw new ArgumentNullException(nameof(routsl));
            if (stopsl == null) throw new ArgumentNullException(nameof(stopsl));
            if (timesl == null) throw new ArgumentNullException(nameof(timesl));

            foreach (var rout in routsl)
            {
                var rout1 = rout;

                rout1.Stops = rout1.RouteStops.Join(stopsl, i => i, stop => stop.ID, (i, stop) =>
                {
                    if (stop.Routs == null)
                        stop.Routs = new List<Rout>();
                    stop.Routs.Add(rout1);
                    return stop;
                }).ToList();
            }
            watch.Stop();
            var xx = watch.ElapsedMilliseconds;
        }


        protected bool NeedUpdate(IList<Rout> newRoutes, IList<Stop> newStops, IList<Schedule> newSchedule)
        {
            if (Stops == null || Routs == null || Times == null || !Stops.Any() || !Routs.Any() || !Times.Any())
                return true;
            if (newStops.Count != Stops.Count() || newRoutes.Count != Routs.Length || newSchedule.Count != Times.Length)
                return true;

            foreach (var newRoute in newRoutes)
            {
                if (Routs.AsParallel().All(x => x.RoutId == newRoute.RoutId && x.Datestart != newRoute.Datestart))
                    return true;
            }
            return false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
            finally
            {
            }
        }

        #region Event

        public delegate void EmptyDelegate(object sender, EventArgs args);

        //public event EmptyDelegate DataBaseDownloadStarted;
        //public event EmptyDelegate DataBaseDownloadEnded;
        public event EventHandler<EventArgs> ApplyUpdateStarted;
        public event EventHandler<EventArgs> UpdateStarted;
        public event EventHandler<EventArgs> UpdateEnded;
        public event EventHandler<EventArgs> LoadStarted;
        public event EventHandler<EventArgs> LoadEnded;
        public event ErrorLoadingDelegate ErrorLoading;
        public event EventHandler<EventArgs> ApplyUpdateEnded;

        #region Invokators

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

        #endregion

        #endregion

        #region Legacy code, Using xml for store settings

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
        public void ReadXml(string reader)
        {
            try
            {
                var node = XDocument.Parse(reader);
                var document = (XElement) node.Root;

                //document.Root.Attribute("LastUpdateTime").Value;

                //LastUpdateDataDateTime = (DateTime) document.Attribute("LastUpdateTime");

                var temp1 = document.Elements("FavouriteStops").Elements("ID");
                FavouriteStopsIds = new ObservableCollection<int>(temp1.Select(x => (int) (x)).ToList());

                var temp = document.Elements("FavouritRouts").Elements("ID").ToList();
                if (temp.Count() > 0)
                    FavouriteRoutsIds = new ObservableCollection<int>(temp.Select(x => (int) x));

                var xGroups = document.Element("Groups");
                if (xGroups != null)
                    GroupsStopIds = new ObservableCollection<GroupStopId>(xGroups.Elements("Group").Select(XGroup =>
                    {
                        var groupid = new GroupStopId();
                        groupid.Name = (string) XGroup.Attribute("Name");
                        groupid.StopID = XGroup.Elements("ID").Select(id => (int) id).ToList();
                        return groupid;
                    }));
                else
                    GroupsStopIds = new ObservableCollection<GroupStopId>();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Context.ReadXml: " + e.Message);
                return;
            }
        }


        public void ReadXml(XmlReader reader)
        {
            try
            {
                var node = XDocument.Load(reader);
                var document = (XElement) node.Root;

                //document.Root.Attribute("LastUpdateTime").Value;

                //LastUpdateDataDateTime = (DateTime) document.Attribute("LastUpdateTime");

                var temp1 = document.Elements("FavouriteStops").Elements("ID");
                FavouriteStopsIds = new ObservableCollection<int>(temp1.Select(x => (int) (x)).ToList());

                var temp = document.Elements("FavouritRouts").Elements("ID").ToList();
                if (temp.Any())

                    FavouriteRoutsIds = new ObservableCollection<int>(temp.Select(x => { return (int) x; }));

                var xGroups = document.Element("Groups");
                if (xGroups != null)
                    GroupsStopIds = new ObservableCollection<GroupStopId>(xGroups.Elements("Group").Select(XGroup =>
                    {
                        var groupid = new GroupStopId();
                        groupid.Name = (string) XGroup.Attribute("Name");
                        groupid.StopID = XGroup.Elements("ID").Select(id => (int) id).ToList();
                        return groupid;
                    }));
                else
                    GroupsStopIds = new ObservableCollection<GroupStopId>();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Context.ReadXml: " + e.Message);
                return;
            }
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

            //element.SetAttributeValue("LastUpdateTime", LastUpdateDataDateTime);


            element.SetAttributeValue("V", 1);


            XElement el = new XElement("FavouritRouts", FavouriteRouts.Select(x => new XElement("ID", x.RoutId)));

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
        }

        #endregion

        #region commands

/*
		private bool updating = false;
*/


//		public RelayCommand UpdateDataCommand
//		{
//			get
//			{
//				return new RelayCommand(async () =>
//				{
//#if WINDOWS_PHONE_APP
//					InternetHelperBase.UpdateNetworkInformation();
//					if (!InternetHelperBase.Is_Connected)
//						return;
//#endif
//					updating = true;
//					UpdateDataCommand.RaiseCanExecuteChanged();
//					//InternetHelper.UpdateNetworkInformation();

//					await UpdateAsync();
//					updating = false;
//					UpdateDataCommand.RaiseCanExecuteChanged();
//				}, ()=>!updating);
//			}
//		}

        public IEnumerable<string> GetDestinations(Rout rout)
        {
            return new List<string>();
        }

        #endregion

        protected virtual void OnUpdateStarted()
        {
            var handler = UpdateStarted;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnUpdateEnded()
        {
            var handler = UpdateEnded;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public bool IsFavouriteStop(Stop stop)
        {
            if (stop == null)
                return false;
            return favouriteStops.Contains(stop.ID);
        }

        public bool IsFavouriteRout(Rout rout)
        {
            if (rout == null)
                return false;
            return FavouriteRouts.Contains(rout);
        }


        public getStop GetStopDelegate { get; protected set; }
        public getDirection GetStopDirectionDelegate { get; }
	    public List<Stop> GetAllStop(Rout r, Stop start, Stop end)
	    {
		    var sIndex = r.Stops.FindIndex(s => s.ID == start.ID);
		    var eIndex = r.Stops.FindIndex(s => s.ID == end.ID);
		    if (sIndex < 0 || eIndex < 0)
			    return null;
		    if (sIndex > eIndex)
			    return null;
		    var result = new List<Stop>(eIndex - sIndex + 1);
		    for (int i = sIndex; i <= eIndex; i++)
			    result.Add(r.Stops[i]);
		    return result;
		    //return r.Stops.SkipWhile(s => s.ID != start.ID).TakeWhile(s => s.ID != end.ID).ToList();
	    }

	    public int GetRoutInterval(Rout rout, DateTime currentTime, Stop stop, int dayOfWeek)
	    {
		    var totalMins = currentTime.TimeOfDay.TotalMinutes;
		    var times = rout.Time.GetScheduleForStop(rout.Stops.IndexOf(stop), dayOfWeek);
		    var index = times.Times.Select(delegate(int i, int i1) { return new {Index = i, Diff = i1 - totalMins}; })
			    .Where(x => x.Diff >= 0).OrderBy(x => x.Diff).Select(x=> x.Index).First();

		    return times.Times[index + 1] - times.Times[index];
	    }

	    private int GetStopIndex(Rout rout, Stop stop)
	    {
		    return rout.Stops.IndexOf(rout.Stops.First(x => x.ID == stop.ID));
	    }

	    public int GetRoutTime(Rout rout, DateTime currentTime, Stop stop1, Stop stop2, int dayOfWeek)
	    {
			var totalMins = currentTime.TimeOfDay.TotalMinutes;
		    var index1 = GetStopIndex(rout, stop1);
		    var time = Times.FirstOrDefault(x => x.RoutId == rout.RoutId);
		    var times1 = time?.GetScheduleForStop(index1, dayOfWeek);
		    if (times1 == null)
			    return -1;
			var index = times1.Times.Select(delegate (int i, int i1) { return new { Index = i1, Diff = i - totalMins }; })
				.Where(x => x.Diff >= 0).OrderBy(x => x.Diff).Select(x => x.Index).FirstOrDefault();
		    var times2 = time.GetScheduleForStop(GetStopIndex(rout, stop2), dayOfWeek);
		    if (times2 == null)
			    return -1;
		    return Math.Abs(times2.Times[index] - times1.Times[index]);
	    }

	    public double GetAvgRoutTime(Stop stop1, Stop stop2)
	    {
			var routes = stop1.Routs.Intersect(stop2.Routs).Where(x => x.Weekdays.Contains('1')).ToList();
		    if (!routes.Any())
			    return 0;
			DateTime timeOfDay = new DateTime(2016, 6, 15, 12, 0, 0, 0);
			return routes.Average(x => GetRoutTime(x, timeOfDay, stop1, stop2, 1));
		}

	    public double GetAvgRoutIntervalBetweenStops(Stop stop1, Stop stop2)
	    {
			var routes = stop1.Routs.Intersect(stop2.Routs).Where(x => x.Weekdays.Contains('1')).ToList();
			if (!routes.Any())
				return 0;
			DateTime timeOfDay = new DateTime(2016, 6, 15, 12, 0, 0, 0);
			return routes.Average(x => GetRoutInterval(x, timeOfDay, stop1, 1));
	    }

	    protected virtual void OnLoadStarted()
        {
            var handler = LoadStarted;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnLoadEnded()
        {
            var handler = LoadEnded;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnErrorLoading(ErrorLoadingDelegateArgs args)
        {
            var handler = ErrorLoading;
            handler?.Invoke(this, args);
        }

        public async Task AddFavouriteRout(Rout rout)
        {
            if (!IsFavouriteRout(rout))
            {
                favouriteRouts.Add(rout.RoutId);
                await SaveFavourite(TypeFolder.Roaming);
            }
        }

        public async Task AddFavouriteStop(Stop stop)
        {
            if (IsFavouriteStop(stop))
                return;
            favouriteStops.Add(stop.ID);
            await SaveFavourite(TypeFolder.Roaming);
        }

        public async Task RemoveFavouriteRout(Rout rout)
        {
            if (!IsFavouriteRout(rout))
                return;
            favouriteRouts.Remove(rout.RoutId);
            await SaveFavourite(TypeFolder.Roaming);
        }

        public async Task RemoveFavouriteStop(Stop stop)
        {
            if (!IsFavouriteStop(stop))
                return;
            favouriteStops.Remove(stop.ID);
            await SaveFavourite(TypeFolder.Roaming);
        }

        public async Task AddGroup(GroupStop group)
        {
            Groups.Add(group);
            await SaveFavourite(TypeFolder.Roaming);
        }

        public async Task RemoveGroup(GroupStop group)
        {
            Groups.Remove(group);
            await SaveFavourite(TypeFolder.Roaming);
        }


        public FileHelperBase FileHelper => fileHelper;

        public Rout[] Routs { get; protected set; }
        public Schedule[] Times { get; protected set; }

        public IEnumerable<Stop> ActualStops => Stops;

        protected IList<int> favouriteRouts { get; set; }

        public IList<Rout> FavouriteRouts
        {
            get
            {
                if (Routs == null)
                    return new List<Rout>();
                return Routs.Where(x => favouriteRouts.Contains(x.RoutId)).ToList();
            }
        }

        public IList<GroupStop> Groups { get; protected set; }
	    public DateTime UpdateDateTimeUtc { get; set; }
	    protected IList<int> favouriteStops { get; set; }

        public IList<Stop> FavouriteStops
        {
            get
            {
                if (ActualStops == null)
                    return new List<Stop>();
                return ActualStops.Where(st => favouriteStops.Contains(st.ID)).ToList();
            }
        }

        public Stop[] Stops { get; protected set; }

        //public ContextFileSettings Settings
        //{
        //	get
        //	{
        //		if (settings == null)
        //			settings = new ContextFileSettings();
        //		return settings;
        //	}
        //	set
        //	{
        //		if (value == null)
        //			throw new ArgumentNullException(nameof(value));
        //		settings = value;
        //	}
        //}

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public IEnumerable<KeyValuePair<Rout, TimeSpan>> GetStopTimeLine(Stop stp, int day, int startingTime,
            TransportType selectedTransportType = TransportType.All, int endTime = int.MaxValue)
        {
            IEnumerable<KeyValuePair<Rout, TimeSpan>> stopTimeLine = new List<KeyValuePair<Rout, TimeSpan>>();


            foreach (Rout rout in stp.Routs.Where(x => selectedTransportType.HasFlag(x.Transport)))
            {
                Schedule sched = Times.FirstOrDefault(r => r.RoutId == rout.RoutId);
                //Schedule sched = rout.Time;
                IEnumerable<KeyValuePair<Rout, TimeSpan>> temp =
                    sched.GetListTimes(rout.Stops.IndexOf(stp), day, startingTime, endTime)
                        .Select(x => new KeyValuePair<Rout, TimeSpan>(x.Key, new TimeSpan(0, 0, x.Value, 0, 0)));

                stopTimeLine = stopTimeLine.Concat(temp);
            }
            stopTimeLine = stopTimeLine.OrderBy(x => x.Value);
            return stopTimeLine;
        }
    }
}