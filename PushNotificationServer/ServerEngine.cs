using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.CommandWpf;
using PushNotificationServer.Properties;
using Task = System.Threading.Tasks.Task;
using Autofac;
using CommonLibrary.Notify;
using MetroLog;
using Microsoft.OneDrive.Sdk.WindowsForms;
using MinskTrans.Context;
using MinskTrans.Context.Base;
using MinskTrans.Context.Desktop;
using MinskTrans.Context.Fakes;
using MinskTrans.Context.UniversalModelView;
using MinskTrans.Net;
using MinskTrans.Net.Base;
using MinskTrans.Utilites;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Base.Net;
using MinskTrans.Utilites.Desktop;
using MyLibrary;
using OneDriveRestAPI;
using OneDriveRestAPI.Model;
using CredentialCache = Microsoft.OneDrive.Sdk.CredentialCache;
using File = System.IO.File;
using IContainer = Autofac.IContainer;


namespace PushNotificationServer
{
	public class ServerEngine:INotifyPropertyChanged
	{
		private static ServerEngine engine;
		private IBussnessLogics context;
		private string fileNameLastNews = "LastNews.txt";
	    private ILogger log;

		private Timer timerNewsAutoUpdate;
		readonly UpdateManagerBase updateManager;

		public static ServerEngine Engine
		{
			get
			{
				if (engine == null)
					engine = new ServerEngine();
				return engine;
			}
		}

	    async public Task InicializeAsync()
		{
			//NewsManager.LastNewsTime = Settings.Default.LastUpdatedNews;
			//NewsManager.LastHotNewstime = Settings.Default.LastUpdatedHotNews;
			await NewsManager.Load();
			await BusnesLogic.LoadDataBase(LoadType.LoadDB);
			BusnesLogic.Settings.LastUpdateDbDateTimeUtc = Settings.Default.DBUpdateTime;
			OndeDriveController.Inicialize();
			this.StopChecknews += (sender, args) => UploadAllToOneDrive();
#if DEBUG
			fileNameLastNews = "LastNewsDebug.txt";
#endif
		}

		void UploadAllToOneDrive()
		{
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			OndeDriveController.UploadFileAsync(TypeFolder.Local, NewsManager.FileNameDays);
			OndeDriveController.UploadFileAsync(TypeFolder.Local, NewsManager.FileNameMonths);
			OndeDriveController.UploadFileAsync(TypeFolder.Local, fileNameLastNews);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		void SaveTime()
		{
		    var helper = container.Resolve<FileHelperBase>();
			File.WriteAllText(Path.Combine(helper.GetPath(TypeFolder.Local), fileNameLastNews),
				NewsManager.LastUpdateMainNewsDateTimeUtc.ToString(CultureInfo.InvariantCulture) + Environment.NewLine + NewsManager.LastUpdateHotNewsDateTimeUtc.ToString(CultureInfo.InvariantCulture) +
				Environment.NewLine + BusnesLogic.LastUpdateDbDateTimeUtc.ToString(CultureInfo.InvariantCulture));
		}

		private NewsManagerBase newsManager;
        IContainer container ;

        public NewsManagerBase NewsManager { get { return newsManager; } }

		ServerEngine()
		{
			//newsManager = new NewsManager(new FileHelper());
			SetAutoUpdateTimer(NewsAutoUpdate);
			//CloudController = cloudStorageController;

			var builder = new ContainerBuilder();

			builder.RegisterType<FileHelperDesktop>().As<FileHelperBase>().SingleInstance();
          // builder.RegisterType<SqlEFContext>().As<IContext>().WithParameter("connectionString", @"default");
            builder.RegisterType<Context>().As<IContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>().SingleInstance();
			builder.RegisterType<InternetHelperDesktop>().As<InternetHelperBase>().SingleInstance();
			builder.RegisterType<OneDriveController>().As<ICloudStorageController>().SingleInstance();
			builder.RegisterType<NewsManagerDesktop>().As<NewsManagerBase>().SingleInstance();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>().SingleInstance();
		    builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
            builder.RegisterType<FakeGeolocation>().As<IGeolocation>().SingleInstance();
            builder.RegisterType<FakeSettingsModelView>().As<ISettingsModelView>().SingleInstance();
            builder.RegisterType<ExternalCommands>().As<IExternalCommands>().SingleInstance();
            builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
		    builder.RegisterType<NotifyHelperDesctop>().As<INotifyHelper>().SingleInstance();
            
            container = builder.Build();

			context = container.Resolve<IBussnessLogics>();
			newsManager = container.Resolve<NewsManagerBase>();
			updateManager = container.Resolve<UpdateManagerBase>();
			OndeDriveController = container.Resolve<ICloudStorageController>();
		    log = container.Resolve<ILogManager>().GetLogger<ServerEngine>();
#if DEBUG
            NewsManager.FileNameDays = "daysDebug.txt";
			NewsManager.FileNameMonths = "monthDebug.txt";
			fileNameLastNews = "lastNewsDebug.txt";
#endif
        }

        public class OneDriveController : ICloudStorageController
        {
            private ILogger log;
            public OneDriveController(ILogManager logManager)
            {
                if (logManager == null)
                    throw new ArgumentNullException(nameof(logManager));
                log = logManager.GetLogger<OneDriveController>();
            }
            private Options options;

            private WebBrowser browser;

            private UserToken token;

            // Initialize a new Client (without an Access/Refresh tokens
            Client client;
            public void Inicialize()
            {
                options = new Options
                {
                    ClientId = "0000000040158EFF",
                    ClientSecret = "2QIsVS59PY9HZM--yq9W7PPeaya-q0lO",
                    AutoRefreshTokens = true
                };

                client = new Client(options);

                // Get the OAuth Request Url
                var authRequestUrl = client.GetAuthorizationRequestUrl(new[] { Scope.Basic, Scope.Signin, Scope.SkyDrive });




                HttpClient clientHttp = new HttpClient();
                //clientHttp.GetAsync(authRequestUrl);


                browser = new WebBrowser();
                //browser.Navigate(authRequestUrl);

                WebRequest request = WebRequest.Create(authRequestUrl);
                // If required by the server, set the credentials.
                request.Credentials = new System.Net.CredentialCache();
                // Get the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // Display the status.
                Console.WriteLine(response.StatusDescription);
                // Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();

                lastUrl = authRequestUrl;
                browser.Navigated += Navigated;
                browser.Navigating += (sender, args) =>
                {

                };
                browser.Navigate(new Uri(authRequestUrl));
            }

            public Task UploadFileAsync(TypeFolder pathToFile, string newNameFile)
            {
                var fileHelper = ServerEngine.Engine.container.Resolve<FileHelperBase>();
                return UploadFileAsync(Path.Combine( fileHelper.GetPath(pathToFile), newNameFile), newNameFile);
            }

            public event EventHandler<EventArgs> NeedAttention;

            private string lastUrl = "";

            public async Task UploadFileAsync(string pathToFile, string newNameFile)
            {
                if (token != null)
                {
                    try
                    {
                        options.RefreshToken = token.Refresh_Token;
                        options.AccessToken = token.Access_Token;
                        var rootFolder = await client.GetFolderAsync();
                        using (var fileStream = File.OpenRead(pathToFile))
                        {
                            await client.UploadAsync(rootFolder.Id, fileStream, newNameFile, OverwriteOption.Overwrite);
                        }
                    }
                    catch (FileNotFoundException e)
                    {
                        log.Error("UploadFileAsync: File no foudn",e);
                        throw;
                    }
                }
            }

            async void Navigated(Object sender, NavigationEventArgs AddingNewEventArgs)
            {
                browser.Navigated -= Navigated;
                var resultString = browser.Source;

                var authCode = new string(resultString.Query.Skip(6).ToArray());
                if (authCode.Length > 50)
                    return;
                //var authCode = @"M0921412b-b519-23a5-8c5b-c2cd95f2565c";
                // Exchange the Authorization Code with Access/Refresh tokens
                token = await client.GetAccessTokenAsync(authCode);

                // Get user profile
                var userProfile = await client.GetMeAsync();
                //Console.WriteLine("Name: " + userProfile.Name);
                //Console.WriteLine("Preferred Email: " + userProfile.Emails.Preferred);

                //// Get user photo
                //var userProfilePicture = await client.GetProfilePictureAsync(PictureSize.Small);
                //Console.WriteLine("Avatar: " + userProfilePicture);

                //// Retrieve the root folder
                //var rootFolder = await client.GetFolderAsync();
                //Console.WriteLine("Root Folder: {0} (Id: {1})", rootFolder.Name, rootFolder.Id);

                //// Retrieve the content of the root folder
                //var folderContent = await client.GetContentsAsync(rootFolder.Id);
                //foreach (var item in folderContent)
                //{
                //	Console.WriteLine("\tItem ({0}: {1} (Id: {2})", item.Type, item.Name, item.Id);
                //}

                options.RefreshToken = token.Refresh_Token;
                options.AccessToken = token.Access_Token;
                //// Initialize a new Client, this time by providing previously requested Access/Refresh tokens
                //var client2 = new Client(options);

                //// Find a file in the root folder
                //var file = folderContent.FirstOrDefault(x => x.Type == File.FileType);

                //// Download file to a temporary local file
                //var tempFile = Path.GetTempFileName();
                //using (var fileStream = System.IO.File.OpenWrite(tempFile))
                //{
                //	var contentStream = await client2.DownloadAsync(file.Id);
                //	await contentStream.CopyToAsync(fileStream);
                //}


                //// Upload the file with a new name
                //using (var fileStream = System.IO.File.OpenRead(tempFile))
                //{
                //	await client2.UploadAsync(rootFolder.Id, fileStream, "Copy Of " + file.Name);
                //}
            }
        }

        public bool NewsAutoUpdate
		{
			get { return Settings.Default.NewsAutoUpdate; }
			set
			{
				Settings.Default.NewsAutoUpdate = value;
				Settings.Default.Save();
				SetAutoUpdateTimer(NewsAutoUpdate);
				OnPropertyChanged();
			}
		}

		public int DbUpdateMins
		{
			get { return Properties.Settings.Default.DbUpdateInterval; }
			set
			{
				if (value <= 0)
					return;
				Properties.Settings.Default.DbUpdateInterval = value;
				OnPropertyChanged();
			}
		}

		public void SetAutoUpdateTimer(bool turnOn)
		{
			if (turnOn)
			{
				if (timerNewsAutoUpdate == null)
					timerNewsAutoUpdate = new Timer(ChuckNews, null, new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 0, DbUpdateMins, 0));
				else
					timerNewsAutoUpdate.Change(new TimeSpan(0, 0, 0, 30), new TimeSpan(0,0,DbUpdateMins,0));
			}
			else
			{
			    if (timerNewsAutoUpdate == null)
			        return;
				timerNewsAutoUpdate.Dispose();
				timerNewsAutoUpdate = null;
			}
		}

		public async void ChuckNews(object obj)
		{
			await CheckNews();
		}

		private bool Updating = false;

		public event StartCheckNewsDelegate StartCheckNews;
		public event StartCheckNewsDelegate StopChecknews;

		private static object o = new object();
		private static object a = new object();

		public async Task CheckNews()
		{
			if (Updating)
				return;
			Debug.WriteLine("Check news started");
			Updating = true;
			OnStartCheckNews();
			try
			{
				await Task.WhenAll(Task.Run(async () =>
				{
				    try
				    {
				        await newsManager.CheckMainNewsAsync();
				        lock (a)
				        {
				            newsManager.SaveToFile();
				        }
				        //OndeDriveController.UploadFile(NewsManager.FileNameMonths, NewsManager.FileNameMonths);
				    }
				    catch (HttpRequestException ex)
				    {
                        log.Error("Bad request while donloading file");
				        container.Resolve<INotifyHelper>().ReportErrorAsync("Bad request, try another time");
				    }
					catch (Exception e)
					{
					    log.Error(e.Message, e);
						throw;
					}
				}), Task.Run(async () =>
				{
				    try
				    {
				        await newsManager.CheckHotNewsAsync();

				        lock (o)
				        {
				            newsManager.SaveToFileHotNews();
				        }
				    }
				    catch (HttpRequestException ex)
				    {
                        container.Resolve<INotifyHelper>().ReportErrorAsync("Bad request, try another time");
                    }
					catch
					{
						throw;
					}
					//OndeDriveController.UploadFile(NewsManager.FileNameDays, NewsManager.FileNameDays);
				}), Task.Run( (async ()=>
				{
				    try
				    {
				        await context.UpdateTimeTableAsync();
				    }
				    catch (Exception e)
				    {
                        log.Error("Error while updating timetable");
                        container.Resolve<INotifyHelper>().ReportErrorAsync(DateTime.Now + ": Error while updating timetable");
                    }
				})));

			}
			catch (TaskCanceledException e)
			{
				Updating = false;
				OnStopChecknews();
			    log.Error("CheckNews: TaskCancelException", e);
			    //throw;
			}
			catch (Exception e)
			{
                log.Error("CheckNews Error", e);
				throw;
			}
			Settings.Default.LastUpdatedNews = newsManager.LastUpdateMainNewsDateTimeUtc;
			Settings.Default.LastUpdatedHotNews = newsManager.LastUpdateMainNewsDateTimeUtc;
			Settings.Default.DBUpdateTime = context.LastUpdateDbDateTimeUtc;
			Settings.Default.Save();		
			SaveTime();
			Updating = false;
			OnStopChecknews();
			Debug.WriteLine("Check news ended");
		}

		public RelayCommand ResetLastUpdatetime
		{
			get { return new RelayCommand(() =>
			{
				NewsManager.LastUpdateMainNewsDateTimeUtc = new DateTime();
				NewsManager.LastUpdateHotNewsDateTimeUtc = new DateTime();
				Settings.Default.LastUpdatedHotNews = new DateTime();
				Settings.Default.LastUpdatedNews = new DateTime();
				Settings.Default.Save();
			});}
		}

		public ICloudStorageController OndeDriveController { get; }

	    public IBussnessLogics BusnesLogic
		{
			get { return context; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnStartCheckNews()
		{
			var handler = StartCheckNews;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		protected virtual void OnStopChecknews()
		{
			var handler = StopChecknews;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		

	}

	public delegate void StartCheckNewsDelegate(object sender, EventArgs args);
}
