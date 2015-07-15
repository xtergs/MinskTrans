using MyLibrary;
using System;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient.Net
{
	public class InternetHelperDesktop:InternetHelperBase
	{
		public InternetHelperDesktop(FileHelperBase fileHelper)
			:base(fileHelper)
		{

		}
//		public override async Task<string> Download(string uri)
//		{
//			try
//			{
//				var httpClient = new System.Net.HttpClient();
//				// Increase the max buffer size for the response so we don't get an exception with so many web sites

//				httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
//				httpClient.MaxResponseContentBufferSize = 256000;
//				httpClient.DefaultRequestHeaders.Add("user-agent",
//					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

//				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
//				response.EnsureSuccessStatusCode();

//				return await response.Content.ReadAsStringAsync();
//			}
//			catch (Exception e)
//			{
//#if BETA
//				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
//#endif
//				throw new TaskCanceledException(e.Message, e);
//			}
//		}

//		public override async Task Download(string uri, string file, TypeFolder folder)
//		{
//			try
//			{
//				var httpClient = new HttpClient();
//				// Increase the max buffer size for the response so we don't get an exception with so many web sites

//				httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
//				httpClient.MaxResponseContentBufferSize = 256000;
//				httpClient.DefaultRequestHeaders.Add("user-agent",
//					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

//				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
//				response.EnsureSuccessStatusCode();

//				await FileHelper.WriteTextAsync(folder, file, await response.Content.ReadAsStringAsync());
//			}
//			catch (Exception e)
//			{
//#if BETA
//				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
//#endif
//				throw new TaskCanceledException(e.Message, e);
//			}
//		}

		#region Overrides of InternetHelperBase

		public override void UpdateNetworkInformation()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
