using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using OneDriveRestAPI;
using OneDriveRestAPI.Model;
using File = System.IO.File;
using PushNotificationServer.CloudStorage;
using MyLibrary;

namespace PushNotificationServer.CloudStorage.OneDrive
{
	public class OneDriveController : ICloudStorageController
	{
		readonly FileHelperBase fileHelper;
		public OneDriveController(FileHelperBase fileHelper)
		{
			this.fileHelper = fileHelper;
		}
		private Options options;

		private WebBrowser browser;

		private UserToken token;

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
			
			lastUrl = authRequestUrl;
			browser.Navigated += Navigated;
			browser.Navigating += (sender, args) =>
			{

			};
			browser.Navigate( new Uri(authRequestUrl));
		}

		private string lastUrl = "";

		public FileHelperBase FileHelper
		{
			get
			{
				return fileHelper;
			}
		}

		public async Task UploadFileAsync(TypeFolder pathToFile, string newNameFile)
		{
			if (token != null)
			{
				options.RefreshToken = token.Refresh_Token;
				options.AccessToken = token.Access_Token;
				var rootFolder = await client.GetFolderAsync();
				using (var fileStream = await FileHelper.OpenStream(pathToFile, newNameFile))
				{
					await client.UploadAsync(rootFolder.Id, fileStream, newNameFile, OverwriteOption.Overwrite);
				}
			}
		}

		public event EventHandler<EventArgs> NeedAttention;
		void OnNeedAttention()
		{
			var handler = NeedAttention;
			if (handler != null)
				handler.Invoke(this, new EventArgs());
		}

		async void Navigated(Object sender, NavigationEventArgs AddingNewEventArgs)
		{
			browser.Navigated -= Navigated;
			var resultString = browser.Source;
			
			var authCode = new string(resultString.Query.Skip(6).ToArray());
			if (authCode.Length > 50)
			{
				OnNeedAttention();
				return;
			}
			
			token = await client.GetAccessTokenAsync(authCode);

			var userProfile = await client.GetMeAsync();
			
			options.RefreshToken = token.Refresh_Token;
			options.AccessToken = token.Access_Token;
		}
	}
}
