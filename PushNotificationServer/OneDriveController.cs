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

namespace PushNotificationServer
{
	public class OneDriveController
	{
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
			request.Credentials = CredentialCache.DefaultCredentials;
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
			browser.Navigate( new Uri(authRequestUrl));
		}

		private string lastUrl = "";

		public async Task UploadFileAsync(string pathToFile, string newNameFile)
		{
			if (token != null)
			{
				options.RefreshToken = token.Refresh_Token;
				options.AccessToken = token.Access_Token;
				var rootFolder = await client.GetFolderAsync();
				using (var fileStream = File.OpenRead(pathToFile))
				{
					await client.UploadAsync(rootFolder.Id, fileStream, newNameFile, OverwriteOption.Overwrite);
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
}
