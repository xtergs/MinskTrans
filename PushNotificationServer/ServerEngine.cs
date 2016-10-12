using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using PushNotificationServer.Properties;
using Task = System.Threading.Tasks.Task;
using Autofac;
using CommonLibrary.Notify;
using MetroLog;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
//using Microsoft.OneDrive.Sdk.WindowsForms;
using MinskTrans.Context;
using MinskTrans.Context.Base;
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
using PropertyChanged;
using UniversalMinskTransRelease.Helpers;
//using CredentialCache = Microsoft.OneDrive.Sdk.CredentialCache;
using File = System.IO.File;
using IContainer = Autofac.IContainer;


namespace PushNotificationServer
{
	public class NotifyTimeTableChanges
	{
	    private NotificationHubClient GetHub()
	    {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(AppServerConstants.PushNotificationChanelEndPoint, AppServerConstants.PushNotificationChanelHubName);
	        return hub;
	    }
		public async void SendNotificationAsync(string text)
		{
		    var hub = GetHub();
			var toast = $@"<toast><visual><binding template=""ToastText01""><text id=""1"">{text}</text></binding></visual></toast>";
			var result = await hub.SendWindowsNativeNotificationAsync(toast);
			
		}

		public async void SendNotificationAsync(TypeOfUpdates updates)
		{
		    var hub = GetHub();
			var toast = $@"{updates}";
			var notification = new WindowsNotification(toast);
			notification.ContentType = "application/octet-stream";
			notification.Headers["X-WNS-Type"] = @"wns/raw";
			var result  = await hub.SendNotificationAsync(notification);
		}
	}

    public interface ICloudSettings
    {
        string CloudToken { get; set; }
    }

    public class CloudSeettings : ICloudSettings
    {
        public string CloudToken
        {
            get { return Settings.Default.CloudToken; }
            set { Settings.Default.CloudToken = value; }
        }
    }

    [ImplementPropertyChanged]
	public class ServerEngine:INotifyPropertyChanged
	{
		private static ServerEngine engine;
		private IBussnessLogics context;
		private ILogger log;
		private FilePathsSettings files;

		private Timer timerNewsAutoUpdate;
		readonly UpdateManagerBase updateManager;

		public bool PublishUpdates => false;

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
			NewsManager.Load().ConfigureAwait(false);
			BusnesLogic.LoadDataBase(LoadType.LoadDB).ConfigureAwait(false);
			BusnesLogic.Settings.LastUpdateDbDateTimeUtc = Settings.Default.DBUpdateTime;
			OndeDriveController.NeedAttention +=
				(sender, args) => container.Resolve<INotifyHelper>().ReportErrorAsync("Need attention OneDrive  !!!");
			OndeDriveController.Inicialize();
			this.StopChecknews += (sender, args) =>
			{
				if (args != TypeOfUpdates.None)
				{
					UploadAllToOneDrive(args);
				}
			};
		}

		public RelayCommand UploadAllToOneDriveCommand { get; private set; }

		public async Task UploadAllToOneDrive(TypeOfUpdates updates)
		{
			if (updates == TypeOfUpdates.None)
				return;
			IsFilesBlocked = true;
			try
			{
				var result = await Task.WhenAll(
					//OndeDriveController.UploadFileAsync(files.HotNewsFile.Folder, files.HotNewsFile.FileName),
					//OndeDriveController.UploadFileAsync(files.MainNewsFile.Folder, files.MainNewsFile.FileName),
					OndeDriveController.UploadFileAsync(files.LastUpdatedFile.Folder, files.LastUpdatedFile.FileName),
					OndeDriveController.UploadFileAsync(files.AllNewsFileV3.Folder, files.AllNewsFileV3.FileName),
					OndeDriveController.UploadFileAsync(files.RouteFile.Folder, files.RouteFile.FileName),
					OndeDriveController.UploadFileAsync(files.StopsFile.Folder, files.StopsFile.FileName),
					OndeDriveController.UploadFileAsync(files.TimeFile.Folder, files.TimeFile.FileName),
					OndeDriveController.UploadFileAsync(files.TimeTableAllFile.Folder, files.TimeTableAllFile.FileName));
				container.Resolve<NotifyTimeTableChanges>().SendNotificationAsync(updates);
                UploadedFiles.Clear();
			    for (int i = 0; i < result.Length; i++)
			        UploadedFiles.Add(result[i]);

			}
			finally
			{
				IsFilesBlocked = false;
			}

		}

        public readonly ObservableCollection<KeyValuePair<string,string>> UploadedFiles = new ObservableCollection<KeyValuePair<string, string>>();

		void SaveTime()
		{
			var helper = container.Resolve<FileHelperBase>();
			File.WriteAllText(Path.Combine(helper.GetPath(files.LastUpdatedFile.Folder), files.LastUpdatedFile.FileName),
				NewsManager.NewNewsDateTimeUtc.ToString(CultureInfo.InvariantCulture) + Environment.NewLine + NewsManager.HotNewsDateTimeUtc.ToString(CultureInfo.InvariantCulture) +
				Environment.NewLine + BusnesLogic.LastUpdateDbDateTimeUtc.ToString(CultureInfo.InvariantCulture));
		}

		private NewsManagerBase newsManager;
		IContainer container ;

		public NewsManagerBase NewsManager => container.Resolve<NewsManagerBase>();

		ServerEngine()
		{
			UploadAllToOneDriveCommand = new RelayCommand(() =>
			{
				UploadAllToOneDrive(TypeOfUpdates.All);
			}, () => !IsFilesBlocked);
			SetAutoUpdateTimer(NewsAutoUpdate);
			var builder = new ContainerBuilder();

			var configuration = new LoggingConfiguration();

			//configuration.AddTarget(LogLevel.Trace, new PortableFileTarget(new DesktopFileSystem()));

			//configuration.IsEnabled = true;

			//LogManagerFactory.DefaultConfiguration = configuration;

			builder.RegisterType<FileHelperDesktop>().As<FileHelperBase>().SingleInstance();
			//builder.RegisterType<SqlEFContext>().As<IContext>().WithParameter("connectionString", @"default");
			builder.RegisterType<Context>().As<IContext>().SingleInstance();
			builder.RegisterType<BaseNewsContext>().As<INewsContext>().SingleInstance();
			builder.RegisterType<UpdateManagerBase>();
			builder.RegisterType<InternetHelperDesktop>().As<InternetHelperBase>().SingleInstance();
			builder.RegisterType<OneDriveControllerOfficial>().As<ICloudStorageController>().SingleInstance();
			builder.RegisterType<NewsManagerDesktop>().As<NewsManagerBase>().SingleInstance();
			builder.RegisterType<ShedulerParser>().As<ITimeTableParser>().SingleInstance();
			builder.RegisterType<BussnessLogic>().As<IBussnessLogics>().SingleInstance();
			builder.RegisterType<FakeGeolocation>().As<IGeolocation>().SingleInstance();
			builder.RegisterType<FakeSettingsModelView>().As<ISettingsModelView>().SingleInstance();
			builder.RegisterType<ExternalCommands>().As<IExternalCommands>().SingleInstance();
			builder.RegisterInstance<ILogManager>(LogManagerFactory.DefaultLogManager).SingleInstance();
			builder.RegisterType<NotifyHelperDesctop>().As<INotifyHelper>().SingleInstance();
			builder.RegisterType<FilePathsSettings>().SingleInstance();
		    builder.RegisterType<CloudSeettings>().As<ICloudSettings>();
			builder.RegisterType<NotifyTimeTableChanges>().AsSelf().SingleInstance();
			container = builder.Build();

			context = container.Resolve<IBussnessLogics>();
			newsManager = container.Resolve<NewsManagerBase>();
			updateManager = container.Resolve<UpdateManagerBase>();
			OndeDriveController = container.Resolve<ICloudStorageController>();
			OndeDriveController.NeedAttention +=
				(sender, args) => container.Resolve<INotifyHelper>().ReportErrorAsync("Need attension for One Drive!!!");
			log = container.Resolve<ILogManager>().GetLogger<ServerEngine>();
			files = container.Resolve<FilePathsSettings>();
		}

	    public class OneDriveControllerOfficial : ICloudStorageController
	    {
            static class OneDriveErrors
            {
                public const string ItemNotFound = "itemNotFound";
            }

            public string SubFolder { get; } = "/MinskTransRelease";
	        private Options options;

	        public OneDriveControllerOfficial(ICloudSettings settings)
	        {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));
	            _settings = settings;
	            Token = settings.CloudToken;
	        }

	        public async Task Inicialize()
	        {
	            if (string.IsNullOrWhiteSpace(Token))
	            {
	                Auth();
	                return;
	            }

                AccountSession session = new AccountSession();
                session.ClientId = ClientId;
                session.RefreshToken = Token;
                var _msaAuthenticationProvider = new MsaAuthenticationProvider(ClientId, AppResponseUrl, scopes);
                client = new OneDriveClient(oneDriveConsumerBaseUrl, _msaAuthenticationProvider);
                _msaAuthenticationProvider.CurrentAccountSession = session;
                await _msaAuthenticationProvider.AuthenticateUserAsync();


	        }

            public string AppResponseUrl { get; set; } = @"https://login.live.com/oauth20_desktop.srf";
            private readonly string oneDriveConsumerBaseUrl = "https://api.onedrive.com/v1.0";
            private readonly string[] scopes = new string[] { "onedrive.readwrite", "wl.signin", "wl.offline_access" };
	        private readonly string ClientId = "0000000040158EFF";
	        private OneDriveClient client;
	        private string Token;

	        public async void Auth()
            {
                try
                {
                    var msaAuthenticationProvider = new MsaAuthenticationProvider(ClientId, AppResponseUrl,
                        scopes);
                    var authTask = msaAuthenticationProvider.AuthenticateUserAsync();
                    client = new OneDriveClient(oneDriveConsumerBaseUrl, msaAuthenticationProvider);
                    await authTask;
                    var session = (((MsaAuthenticationProvider)client.AuthenticationProvider).CurrentAccountSession);
                    Token = session.RefreshToken;
                    _settings.CloudToken = Token;
                }
                catch (Exception e)
                {

                }
            }

            public Task<KeyValuePair<string, string>> UploadFileAsync(TypeFolder pathToFile, string newNameFile)
            {
                var fileHelper = ServerEngine.Engine.container.Resolve<FileHelperBase>();
                return UploadFileAsync(Path.Combine(fileHelper.GetPath(pathToFile), newNameFile), newNameFile);
            }

            public async Task<KeyValuePair<string, string>> UploadFileAsync(string pathToFile, string newNameFile)
	        {
                using (var fileStream = File.OpenRead(pathToFile))
                {
                    var res = await client.Drive.Root.ItemWithPath(SubFolder + "/" + newNameFile).Content.Request().PutAsync<Item>(fileStream);
                    return new KeyValuePair<string, string>(newNameFile, await GetLink(newNameFile));
                }
	        }

            Regex regex = new Regex(@"(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-?%&!]*)");
	        private ICloudSettings _settings;

	        public async Task<string> GetLink(string newNameFile)
            {
                try
                {
                    return "";
                    //var link =
                    //    (client.Drive.Root.ItemWithPath(SubFolder + "/" + newNameFile).CreateLink("embed"));
                    var response = await (client.Drive.Root.ItemWithPath(SubFolder + "/" + newNameFile).CreateLink("embed")).Request().PostAsync();
                    var match = regex.Match(response.Link.WebHtml);
                    if (match.Success)
                        return match.Value.Replace("embed?", "download?");
                    return "";
                }
                catch (ServiceException e)
                {
                    if (e.Error.Code == OneDriveErrors.ItemNotFound)
                        return null;
                    throw;
                }
            }

            public event EventHandler<EventArgs> NeedAttention;
	    }


  //      public class OneDriveController : ICloudStorageController
		//{
		//	private ILogger log;
		//	public OneDriveController(ILogManager logManager)
		//	{
		//		if (logManager == null)
		//			throw new ArgumentNullException(nameof(logManager));
		//		log = logManager.GetLogger<OneDriveController>();
		//	}
		//	private Options options;

		//	private WebBrowser browser;

		//	private UserToken token;

		//	// Initialize a new Client (without an Access/Refresh tokens
		//	Client client;
		//	public async Task Inicialize()
		//	{
		//		options = new Options
		//		{
		//			ClientId = "0000000040158EFF",
		//			ClientSecret = "2QIsVS59PY9HZM--yq9W7PPeaya-q0lO",
		//			AutoRefreshTokens = true
		//		};

		//		client = new Client(options);

		//		// Get the OAuth Request Url
		//		var authRequestUrl = client.GetAuthorizationRequestUrl(new[] { Scope.Basic, Scope.Signin, Scope.SkyDrive });




		//		HttpClient clientHttp = new HttpClient();
		//		//clientHttp.GetAsync(authRequestUrl);


		//		browser = new WebBrowser();
		//		//browser.Navigate(authRequestUrl);

		//		WebRequest request = WebRequest.Create(authRequestUrl);
		//		// If required by the server, set the credentials.
		//		request.Credentials = new System.Net.CredentialCache();
		//		// Get the response.
		//		HttpWebResponse response = (HttpWebResponse)request.GetResponse();
		//		// Display the status.
		//		Console.WriteLine(response.StatusDescription);
		//		// Get the stream containing content returned by the server.
		//		Stream dataStream = response.GetResponseStream();
		//		// Open the stream using a StreamReader for easy access.
		//		StreamReader reader = new StreamReader(dataStream);
		//		// Read the content.
		//		string responseFromServer = reader.ReadToEnd();

		//		lastUrl = authRequestUrl;
		//		browser.Navigated += Navigated;
		//		browser.Navigating += (sender, args) =>
		//		{

		//		};
		//		browser.Navigate(new Uri(authRequestUrl));
		//	}

		//	public Task<KeyValuePair<string,string>> UploadFileAsync(TypeFolder pathToFile, string newNameFile)
		//	{
		//		var fileHelper = ServerEngine.Engine.container.Resolve<FileHelperBase>();
		//		return UploadFileAsync(Path.Combine( fileHelper.GetPath(pathToFile), newNameFile), newNameFile);
		//	}

		//	public event EventHandler<EventArgs> NeedAttention;

		//	private string lastUrl = "";

		//	public async Task<KeyValuePair<string,string>> UploadFileAsync(string pathToFile, string newNameFile)
		//	{
		//		if (token != null)
		//		{
		//			try
		//			{
		//				options.RefreshToken = token.Refresh_Token;
		//				options.AccessToken = token.Access_Token;
		//				var rootFolder = await client.GetFolderAsync();
		//				using (var fileStream = File.OpenRead(pathToFile))
		//				{
		//					var file = await client.UploadAsync(rootFolder.Id, fileStream, newNameFile, OverwriteOption.Overwrite);
		//				    var loaded = (await client.GetFileAsync(file.Id));
		//				}
		//			}
		//			catch (FileNotFoundException e)
		//			{
		//				log.Error("UploadFileAsync: File no foudn",e);
		//				throw;
		//			}
		//		}
		//	    return new KeyValuePair<string, string>(newNameFile, "");
		//	}

		//	async void Navigated(Object sender, NavigationEventArgs AddingNewEventArgs)
		//	{
		//		browser.Navigated -= Navigated;
		//		var resultString = browser.Source;

		//		var authCode = new string(resultString.Query.Skip(6).ToArray());
		//		if (authCode.Length > 50)
		//			return;
		//		//var authCode = @"M0921412b-b519-23a5-8c5b-c2cd95f2565c";
		//		// Exchange the Authorization Code with Access/Refresh tokens
		//		token = await client.GetAccessTokenAsync(authCode);

		//		// Get user profile
		//		var userProfile = await client.GetMeAsync();
		//		//Console.WriteLine("Name: " + userProfile.Name);
		//		//Console.WriteLine("Preferred Email: " + userProfile.Emails.Preferred);

		//		//// Get user photo
		//		//var userProfilePicture = await client.GetProfilePictureAsync(PictureSize.Small);
		//		//Console.WriteLine("Avatar: " + userProfilePicture);

		//		//// Retrieve the root folder
		//		//var rootFolder = await client.GetFolderAsync();
		//		//Console.WriteLine("Root Folder: {0} (Id: {1})", rootFolder.Name, rootFolder.Id);

		//		//// Retrieve the content of the root folder
		//		//var folderContent = await client.GetContentsAsync(rootFolder.Id);
		//		//foreach (var item in folderContent)
		//		//{
		//		//	Console.WriteLine("\tItem ({0}: {1} (Id: {2})", item.Type, item.Name, item.Id);
		//		//}

		//		options.RefreshToken = token.Refresh_Token;
		//		options.AccessToken = token.Access_Token;
		//		//// Initialize a new Client, this time by providing previously requested Access/Refresh tokens
		//		//var client2 = new Client(options);

		//		//// Find a file in the root folder
		//		//var file = folderContent.FirstOrDefault(x => x.Type == File.FileType);

		//		//// Download file to a temporary local file
		//		//var tempFile = Path.GetTempFileName();
		//		//using (var fileStream = System.IO.File.OpenWrite(tempFile))
		//		//{
		//		//	var contentStream = await client2.DownloadAsync(file.Id);
		//		//	await contentStream.CopyToAsync(fileStream);
		//		//}


		//		//// Upload the file with a new name
		//		//using (var fileStream = System.IO.File.OpenRead(tempFile))
		//		//{
		//		//	await client2.UploadAsync(rootFolder.Id, fileStream, "Copy Of " + file.Name);
		//		//}
		//	}
		//}

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
		private volatile bool IsFilesBlocked = false;
		private TypeOfUpdates currentStatusOfUpdates = TypeOfUpdates.None;

		public event StartCheckNewsDelegate StartCheckNews;
		public event EventHandler<TypeOfUpdates> StopChecknews;

		private static object o = new object();
		private static object a = new object();
		private CancellationTokenSource tokenSource;

		public async Task<bool> CheckNews()
		{
			if (Updating || IsFilesBlocked)
				return false;
			Volatile.Write(ref Updating, true);
			currentStatusOfUpdates = TypeOfUpdates.None;
			Debug.WriteLine("Check news started");
			OnStartCheckNews();
			bool[] results = null;
			try
			{
				using (tokenSource = new CancellationTokenSource())
				{
					results = await Task<bool>.WhenAll(Task<bool>.Run(async () =>
					{
						try
						{
							if (await newsManager.CheckHotNewsAsync())
							{

								currentStatusOfUpdates |= TypeOfUpdates.HotNews;
							}
							if (await newsManager.CheckMainNewsAsync())
							{
								currentStatusOfUpdates |= TypeOfUpdates.MainNews;
							}
								await newsManager.SaveToFile();
							return true;
						}
						catch (HttpRequestException ex)
						{
							container.Resolve<INotifyHelper>().ReportErrorAsync("Bad request, try another time");
						}
						catch
						{
							throw;
						}
						return false;
						//OndeDriveController.UploadFile(NewsManager.FileNameDays, NewsManager.FileNameDays);
					}), Task<bool>.Run((async () =>
					{
						try
						{
							if (await context.UpdateTimeTableAsync(tokenSource.Token, false, true))
							{
								currentStatusOfUpdates |= TypeOfUpdates.TimeTable;
								return true;
							}
						}
						catch (Exception e)
						{
							log.Error("Error while updating timetable");
							container.Resolve<INotifyHelper>()
								.ReportErrorAsync(DateTime.Now + $": Error while updating timetable\n{e.Message}").ConfigureAwait(false);
							
						}
						return false;
					}), tokenSource.Token));
				}

			}
			catch (TaskCanceledException e)
			{
				currentStatusOfUpdates = TypeOfUpdates.None;
				Updating = false;
				OnStopChecknews(TypeOfUpdates.None);
				log.Error("CheckNews: TaskCancelException", e);
				return false;
				//throw;
			}
			catch (Exception e)
			{
				log.Error("CheckNews Error", e);
				throw;
			}
			Settings.Default.DBUpdateTime = context.LastUpdateDbDateTimeUtc;
			Settings.Default.Save();		
			SaveTime();
			OnStopChecknews(currentStatusOfUpdates);
			Volatile.Write(ref Updating, false);
			Debug.WriteLine("Check news ended");
			return results != null && results.Any(x => x);
		}

		public RelayCommand<string> SentPushMessageCommand
		{
			get
			{
				return
					new RelayCommand<string>(
						str =>
						{
							container.Resolve<NotifyTimeTableChanges>().SendNotificationAsync(str);
							container.Resolve<NotifyTimeTableChanges>().SendNotificationAsync(TypeOfUpdates.All);
						},
						s => !string.IsNullOrWhiteSpace(s) && s.Length > 10);
			}
		}

		public RelayCommand ResetLastUpdatetime
		{
			get { return new RelayCommand(() =>
			{
			    context.ResetState();
			    newsManager.ResetState();

			});}
		}

		public ICloudStorageController OndeDriveController { get; }

		public IBussnessLogics BusnesLogic => context;

		public FilePathsSettings Files => container.Resolve<FilePathsSettings>();

		public event PropertyChangedEventHandler PropertyChanged;

		
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnStartCheckNews()
		{
			var handler = StartCheckNews;
			handler?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnStopChecknews(TypeOfUpdates haveUpdates)
		{
			var handler = StopChecknews;
			handler?.Invoke(this, haveUpdates);
		}

		

	}

	public delegate void StartCheckNewsDelegate(object sender, EventArgs args);
}
