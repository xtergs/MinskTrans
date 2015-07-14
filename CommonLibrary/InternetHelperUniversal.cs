using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using MyLibrary;

namespace CommonLibrary
{
	public class InternetHelperUniversal:InternetHelperBase
	{
		public InternetHelperUniversal(FileHelperBase fileHelper)
			:base(fileHelper)
		{ }

		public override async Task<string> Download(string uri)
		{
			try
			{
				var httpClient = new HttpClient();
				// Increase the max buffer size for the response so we don't get an exception with so many web sites

				httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
				httpClient.MaxResponseContentBufferSize = 256000;
				httpClient.DefaultRequestHeaders.Add("user-agent",
					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
				response.EnsureSuccessStatusCode();

				return await response.Content.ReadAsStringAsync();
			}
			catch (Exception e)
			{
//#if BETA
//				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
//#endif
				throw new TaskCanceledException(e.Message, e);
			}
		}

		public override async Task Download(string uri, string file, TypeFolder folder)
		{
			try
			{
				var httpClient = new HttpClient();
				// Increase the max buffer size for the response so we don't get an exception with so many web sites

				httpClient.Timeout = new TimeSpan(0, 0, 10, 0,0);
				httpClient.MaxResponseContentBufferSize = 2560000;
				httpClient.DefaultRequestHeaders.Add("user-agent",
					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri), HttpCompletionOption.ResponseContentRead);
				response.EnsureSuccessStatusCode();

				await FileHelper.WriteTextAsync(folder, file, await response.Content.ReadAsStringAsync());
			}
			catch (Exception e)
			{
//#if BETA
//				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
//#endif
				Debug.WriteLine("InternetHelperUniversal exception");
				throw new TaskCanceledException(e.Message, e);
			}
		}
	}
}
