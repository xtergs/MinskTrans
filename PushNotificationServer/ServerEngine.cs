using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Build.Utilities;
using MinskTrans.DesctopClient.Annotations;
using OneDriveRestAPI;
using OneDriveRestAPI.Model;
using File = OneDriveRestAPI.Model.File;
using Task = System.Threading.Tasks.Task;

namespace PushNotificationServer
{
	public class ServerEngine:INotifyPropertyChanged
	{
		private static ServerEngine engine;
		

		private Timer timerNewsAutoUpdate;

		public static ServerEngine Engine
		{
			get
			{
				if (engine == null)
					engine = new ServerEngine();
				return engine;
			}
		}

		public void Inicialize()
		{
			NewsManager.LastNewsTime = Properties.Settings.Default.LastUpdatedNews;
			NewsManager.Load();
		}

		private NewsManager newsManager;

		public NewsManager NewsManager { get { return newsManager;} }

		ServerEngine()
		{
			newsManager = new NewsManager();
			SetAutoUpdateTimer(NewsAutoUpdate);
		}

		public bool NewsAutoUpdate
		{
			get { return Properties.Settings.Default.NewsAutoUpdate; }
			set
			{
				Properties.Settings.Default.NewsAutoUpdate = value;
				Properties.Settings.Default.Save();
				SetAutoUpdateTimer(NewsAutoUpdate);
				OnPropertyChanged();
			}
		}

		public void SetAutoUpdateTimer(bool turnOn)
		{
			if (turnOn)
			{
				if (timerNewsAutoUpdate == null)
					timerNewsAutoUpdate = new Timer(ChuckNews, null, new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 1, 0, 0));
				else
					timerNewsAutoUpdate.Change(new TimeSpan(0, 0, 0, 30), new TimeSpan(0, 1, 0, 0));
			}
			else
			{
				timerNewsAutoUpdate.Dispose();
				timerNewsAutoUpdate = null;
			}
		}

		public async void ChuckNews(object obj)
		{
			CheckNews();
		}

		private bool Updating = false;

		public event StartCheckNewsDelegate StartCheckNews;
		public event StartCheckNewsDelegate StopChecknews;

		public async Task CheckNews()
		{
			if (Updating)
				return;
			Updating = true;
			OnStartCheckNews();
			try
			{
				await Task.WhenAll(Task.Run(async () =>
				{
					try
					{
						await newsManager.CheckMainNewsAsync();
						newsManager.SaveToFile();
					}
					catch (Exception e)
					{
						
						throw;
					}
				}), Task.Run(async () =>
				{
					await newsManager.CheckHotNewsAsync();
					newsManager.SaveToFileHotNews();
				}));

			}
			catch (TaskCanceledException e)
			{
				Updating = false;
				OnStopChecknews();
				throw;
			}
			Properties.Settings.Default.LastUpdatedNews = newsManager.LastNewsTime;
			Properties.Settings.Default.LastUpdatedHotNews = newsManager.LastHotNewstime;
			Properties.Settings.Default.Save();			
			Updating = false;
			OnStopChecknews();
		}

		public RelayCommand ResetLastUpdatetime
		{
			get { return new RelayCommand(() =>
			{
				NewsManager.LastHotNewstime = new DateTime();
				NewsManager.LastHotNewstime = new DateTime();
				Properties.Settings.Default.LastUpdatedHotNews = new DateTime();
				Properties.Settings.Default.LastUpdatedNews = new DateTime();
				Properties.Settings.Default.Save();
			});}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
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

		public async Task TestOndeDrive()
		{
			var options = new Options
			{
				ClientId = "0000000040158EFF",
				ClientSecret = "2QIsVS59PY9HZM--yq9W7PPeaya-q0lO",
				AutoRefreshTokens = true,
				PrettyJson = false,
				ReadRequestsPerSecond = 2,
				WriteRequestsPerSecond = 2
			};

			// Initialize a new Client (without an Access/Refresh tokens
			var client = new Client(options);

			// Get the OAuth Request Url
			var authRequestUrl = client.GetAuthorizationRequestUrl(new[] { Scope.Basic, Scope.Signin, Scope.SkyDrive, Scope.SkyDriveUpdate });

			// TODO: Navigate to authRequestUrl using the browser, and retrieve the Authorization Code from the response
			HttpClient clientHttp = new HttpClient();
			HttpContent content = new StringContent("");
			var response = await clientHttp.PostAsync(authRequestUrl, content );
			
			var authCode = @"M90d6371c-7207-5225-b4bc-2a2c4f89c82d";

			// Exchange the Authorization Code with Access/Refresh tokens
			var token = await client.GetAccessTokenAsync(authCode);

			// Get user profile
			var userProfile = await client.GetMeAsync();
			Console.WriteLine("Name: " + userProfile.Name);
			Console.WriteLine("Preferred Email: " + userProfile.Emails.Preferred);

			// Get user photo
			var userProfilePicture = await client.GetProfilePictureAsync(PictureSize.Small);
			Console.WriteLine("Avatar: " + userProfilePicture);

			// Retrieve the root folder
			var rootFolder = await client.GetFolderAsync();
			Console.WriteLine("Root Folder: {0} (Id: {1})", rootFolder.Name, rootFolder.Id);

			// Retrieve the content of the root folder
			var folderContent = await client.GetContentsAsync(rootFolder.Id);
			foreach (var item in folderContent)
			{
				Console.WriteLine("\tItem ({0}: {1} (Id: {2})", item.Type, item.Name, item.Id);
			}

			options.RefreshToken = token.Refresh_Token;
			options.AccessToken = token.Access_Token;
			// Initialize a new Client, this time by providing previously requested Access/Refresh tokens
			var client2 = new Client(options);

			// Find a file in the root folder
			var file = folderContent.FirstOrDefault(x => x.Type == File.FileType);

			// Download file to a temporary local file
			var tempFile = Path.GetTempFileName();
			using (var fileStream = System.IO.File.OpenWrite(tempFile))
			{
				var contentStream = await client2.DownloadAsync(file.Id);
				await contentStream.CopyToAsync(fileStream);
			}


			// Upload the file with a new name
			using (var fileStream = System.IO.File.OpenRead(tempFile))
			{
				await client2.UploadAsync(rootFolder.Id, fileStream, "Copy Of " + file.Name);
			}
		}
	}

	public delegate void StartCheckNewsDelegate(object sender, EventArgs args);
}
