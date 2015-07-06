using System;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Web.Http;
using MinskTrans.Universal;
using MyLibrary;

namespace CommonLibrary
{
//	public class InternetHelper : InternetHelperBase
//	{
////		public override Task<string> Download(string uri)
////		{
////			throw new NotImplementedException();
////		}

////		public override async Task Download(string uri, string file, TypeFolder folder)
////		{
////			try
////			{
////				var httpClient = new HttpClient();
////		// Increase the max buffer size for the response so we don't get an exception with so many web sites

////		httpClient.DefaultRequestHeaders.Add("user-agent",
////					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

////				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
////		response.EnsureSuccessStatusCode();

////				//string str= response.StatusCode + " " + response.ReasonPhrase + Environment.NewLine;
////				var fileGet =
////					await folder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
////				using (var writeStream = await fileGet.OpenAsync(FileAccessMode.ReadWrite))
////				{
////					using (var outputStream = writeStream.GetOutputStreamAt(0))
////					{
////						var responseBodyAsText = await response.Content.WriteToStreamAsync(outputStream);
////}
////				}
////			}
////			catch (Exception e)
////			{
////#if BETA
////				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
////#endif
////				throw new TaskCanceledException(e.Message, e);
////			}
////		}
//	}
}
