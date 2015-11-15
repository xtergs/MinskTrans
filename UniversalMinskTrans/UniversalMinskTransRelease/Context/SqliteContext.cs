using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;
using MinskTrans.DesctopClient.Modelview;
using MinskTrans.Universal;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using Newtonsoft.Json;
using SQLite;

namespace UniversalMinskTransRelease.Context
{
    [Table("Routsssss")]
    class RoutSqlite
    {
        
        public int rout { get; set; }
    }
    class SqliteContext : MinskTrans.Context.Context
    {
        public SqliteContext(FileHelperBase helper, InternetHelperBase internetHelper) : base(helper, internetHelper)
        {
        }

        private SQLiteAsyncConnection connection;

        #region Overrides of Context

        public override async Task Load(LoadType type = LoadType.LoadAll)
        {
            Debug.WriteLine("SqliteContext.Load started");
            Debug.WriteLine("SqliteContext LoadSourceData started");
#if BETA
			Logger.Log().WriteLineTime("Load started");
#endif
            OnLoadStarted();

            var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "dbs.sqlite");
            SQLiteConnection connection =
                 new SQLiteConnection(dbPath);


            //await Task.Delay(new TimeSpan(0, 0, 0, 10));

            ObservableCollection<Rout> tpRouts = null;
            ObservableCollection<Stop> tpStops = null;
            ObservableCollection<Schedule> tpTimes = null;
            IList<int> tpFavouriteStops = null;
            ObservableCollection<int> tpFavouriteRouts = null;
            ObservableCollection<GroupStop> tpGroups = null;

            try
            {
                if (type.HasFlag(LoadType.LoadFavourite))
                    try
                    {
                        //await Task.Delay(new TimeSpan(0, 0, 0, 10));

                        //var routsFile = await folders[TypeSaveData.Statisticks].GetFileAsync(NameFileCounter);
                        //var routs = await FileIO.ReadTextAsync(routsFile);

                        string routs =
                            await
                                FileHelper.ReadAllTextAsync(Settings.folders[TypeSaveData.Statisticks],
                                    Settings.NameFileCounter);
                        counterViewStops = JsonConvert.DeserializeObject<Dictionary<int, uint>>(routs);
                    }

                    catch (FileNotFoundException)
                    {
                        counterViewStops = new Dictionary<int, uint>();
                    }
                //await Task.Delay(new TimeSpan(0, 0, 0, 10));
                if (type.HasFlag(LoadType.LoadDB))
                    await Task.WhenAll(
                        Task.Run(async () =>
                        {
                            try
                            {
                                try
                                {
                                    var ttpRouts = connection.Table<Rout>().ToList();
                                }
                                catch (Exception)
                                {
                                    connection.CreateTable<Rout>();
                                }
                                finally
                                {
                                    connection.Close();
                                }

                                var routs =
                                    await
                                        FileHelper.ReadAllTextAsync(Settings.folders[TypeSaveData.DB],
                                            Settings.NameFileRouts);
                                tpRouts = JsonConvert.DeserializeObject<ObservableCollection<Rout>>(routs);

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
                                    await
                                        FileHelper.ReadAllTextAsync(Settings.folders[TypeSaveData.DB],
                                            Settings.NameFileStops);
                                tpStops = JsonConvert.DeserializeObject<ObservableCollection<Stop>>(stops);
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
                                    await
                                        FileHelper.ReadAllTextAsync(Settings.folders[TypeSaveData.DB],
                                            Settings.NameFileTimes);

                                tpTimes = JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(times);
                            }
                            catch (FileNotFoundException e)
                            {
                                throw new TaskCanceledException(e.Message, e);
                            }
                        }));
                else
                {
                    tpRouts = new ObservableCollection<Rout>(Routs);
                    tpStops = new ObservableCollection<Stop>(Stops);
                    tpTimes = new ObservableCollection<Schedule>(Times);
                }
            }
            catch (TaskCanceledException e)
            {
                //CleanTp();
                OnErrorLoading(new ErrorLoadingDelegateArgs() {Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles});
#if BETA
				Logger.Log("Load taskcanceledException").WriteLineTime(e.Message).WriteLine(e.StackTrace);
				Logger.Log().SaveToFile();
#endif
                return;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Context.Load: " + e.Message);
#if BETA
				Logger.Log("Load exception").WriteLineTime(e.Message).WriteLine(e.StackTrace);
				Logger.Log().SaveToFile();
#endif
                throw;
            }
            
            if (tpRouts == null || tpStops == null)
            {
                //CleanTp();
                OnErrorLoading(new ErrorLoadingDelegateArgs() { Error = ErrorLoadingDelegateArgs.Errors.NoSourceFiles });
                return;
            }

            //await Task.Delay(new TimeSpan(0, 0, 0, 10));
            Debug.WriteLine("UniversalContext LoadSourceData ended");
            if (type.HasFlag(LoadType.LoadDB))
            {
                //await Task.Delay(new TimeSpan(0, 0, 0, 10));
                Connect(tpRouts, tpStops, tpTimes, VariantLoad);

                //lock (o)
                //{
                Routs = tpRouts;
                Stops = tpStops;
                Times = tpTimes;
                //ActualStops = Stops;
            }
            //await Task.Delay(new TimeSpan(0, 0, 0, 10));
            Debug.WriteLine("UniversalContext loadfavourite started");
            if (type.HasFlag(LoadType.LoadFavourite) &&
                await FileHelper.FileExistAsync(Settings.folders[TypeSaveData.Favourite], Settings.NameFileFavourite))
            {
                try
                {
                    var textFavourite = await FileHelper.ReadAllTextAsync(Settings.folders[TypeSaveData.Favourite], Settings.NameFileFavourite);
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
                            tpGroups = new ObservableCollection<GroupStop>(desFavourite.Groups.Select(x => new GroupStop()
                            {
                                Name = x.Name,
                                Stops = new List<Stop>(tpStops.Join(x.IDs, stop => stop.ID, i => i, (stop, i) => stop))
                            }));
                        }
                    }
                    catch (JsonReaderException)
                    {

                        ReadXml(textFavourite);
                        //using (var reader = XmlReader.Create(textFavourite, new XmlReaderSettings()))
                        //{
                        //}

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
                                Stops = new List<Stop>(tpStops.Join(x.StopID, stop => stop.ID, i => i, (stop, i) => stop))
                            }));
                            GroupsStopIds = null;
                        }
                    }
                }
                catch (FileNotFoundException e)
                {
                    Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
#if BETA
					Logger.Log().WriteLineTime("Load favourite filenotFound");
#endif
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Context.Load.LoadFavourite: " + e.Message);
#if BETA
					Logger.Log("Load favourite exception").WriteLineTime(e.Message).WriteLine(e.StackTrace);
					Logger.Log().SaveToFile();
#endif
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
                Debug.WriteLine("UniversalContext loadfavourite ended");

                //CleanTp();
                //AllPropertiesChanged();
                //await Task.Delay(new TimeSpan(0, 0, 0, 10));
                OnLoadEnded();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Load exception, set favourite " + e.ToString());
#if BETA
				Logger.Log().WriteLineTime("Load exception");
#endif
                throw;
            }
            Debug.WriteLine("UniversalContext.Load ended");
#if BETA
			Logger.Log().WriteLineTime("Load ended");
#endif
        }

        #endregion

        #region Overrides of Context

        public override async Task Save(bool saveAllDb = true)
        {
            try
            {
                var dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "dbs.sqlite");
               var connection = new SQLite.SQLiteAsyncConnection(dbPath);
               //     new SQLiteConnection(dbPath);
                
                    //connection = new SQLite.SQLiteAsyncConnection(dbPath);

                await SaveFavourite(Settings.folders[TypeSaveData.Favourite]);

                var jsonSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

                await SaveStatistics(jsonSettings, Settings.folders[TypeSaveData.Statisticks]);
                if (saveAllDb)
                    await Task.WhenAll(Task.Run(async () =>
                    {
                        string routs = JsonConvert.SerializeObject(Routs, jsonSettings);
                        await FileHelper.WriteTextAsync(Settings.folders[TypeSaveData.DB], Settings.NameFileRouts + TempExt, routs);
                        await FileHelper.SafeMoveAsync(Settings.folders[TypeSaveData.DB], Settings.NameFileRouts + TempExt, Settings.NameFileRouts);

                        try
                        {
                            var xx = await connection.InsertAllAsync(Routs);
                           
                        }
                        catch (Exception)
                        {
                            //connection.CreateTableAsync<Schedule>();
                            await connection.CreateTablesAsync<RoutBase, Schedule, StopBase>();
                            var xx = await connection.InsertAllAsync(Routs);
                        }
                        finally
                        {
                            //connection.Close();
                        }

                    }),
                        Task.Run(async () =>
                        {
                            string stopsString = JsonConvert.SerializeObject(ActualStops, jsonSettings);

                            await FileHelper.WriteTextAsync(Settings.folders[TypeSaveData.DB], Settings.NameFileStops + TempExt, stopsString);
                            await FileHelper.SafeMoveAsync(Settings.folders[TypeSaveData.DB], Settings.NameFileStops + TempExt, Settings.NameFileStops);

                        }), Task.Run(async () =>
                        {
                            string routs = JsonConvert.SerializeObject(Times, jsonSettings);

                            await FileHelper.WriteTextAsync(Settings.folders[TypeSaveData.DB], Settings.NameFileTimes + TempExt, routs);
                            await FileHelper.SafeMoveAsync(Settings.folders[TypeSaveData.DB], Settings.NameFileTimes + TempExt, Settings.NameFileTimes);

                        }));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in UniversalContext.Save");
#if BETA
				Logger.Log("Save exception").WriteLineTime(e.Message).WriteLine(e.StackTrace);
				Logger.Log().SaveToFile();
#endif
                throw;
            }
        }

        #endregion
        }
}
