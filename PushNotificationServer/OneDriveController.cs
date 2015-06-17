using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using OneDriveRestAPI;
using OneDriveRestAPI.Model;
using File = OneDriveRestAPI.Model.File;

namespace PushNotificationServer
{
	public class OneDriveController
	{
		private Options options;

		private WebBrowser browser;

		// Initialize a new Client (without an Access/Refresh tokens
		Client client;
		public async Task Inicialize()
		{
			options = new Options
			{
				ClientId = "0000000040158EFF",
				ClientSecret = "2QIsVS59PY9HZM--yq9W7PPeaya-q0lO",
				AutoRefreshTokens = true
			};

			client = new Client(options);

			

			// Get the OAuth Request Url
			var authRequestUrl = client.GetAuthorizationRequestUrl(new[] { Scope.Basic, Scope.Signin, Scope.SkyDrive, Scope.OfflineAccess });


			HttpClient clientHttp = new HttpClient();
			browser = new WebBrowser();
			browser.Navigate(authRequestUrl);
			browser.Navigated += Navigated;
		}
		async void Navigated(Object sender, NavigationEventArgs AddingNewEventArgs)
		{
			browser.Navigated -= Navigated;
			var resultString = browser.Source;

			var authCode = new string(resultString.Query.Skip(6).ToArray());

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
}
