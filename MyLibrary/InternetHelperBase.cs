using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace MyLibrary
{
	public abstract class  InternetHelperBase
	{
		public readonly FileHelperBase FileHelper;
		public InternetHelperBase(FileHelperBase fileHelper)
		{
			if (fileHelper == null)
				throw new ArgumentNullException("fileHelper");
			FileHelper = fileHelper;
		}

		static public void UpdateNetworkInformation()
		{
#if BETA
			Logger.Log("UpdateNetworkInformation");
#endif
			// Get current Internet Connection Profile.
			ConnectionProfile internetConnectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
			Is_Connected = true;
			//air plan mode is on...
			if (internetConnectionProfile == null)
			{
				Is_Connected = false;
				return;
			}

			//if true, internet is accessible.
			Is_InternetAvailable = internetConnectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;

			// Check the connection details.
			if (internetConnectionProfile.NetworkAdapter.IanaInterfaceType != 71)// Connection is not a Wi-Fi connection. 
			{
				Is_Roaming = internetConnectionProfile.GetConnectionCost().Roaming;

				/// user is Low on Data package only send low data.
				Is_LowOnData = internetConnectionProfile.GetConnectionCost().ApproachingDataLimit;

				//User is over limit do not send data
				Is_OverDataLimit = internetConnectionProfile.GetConnectionCost().OverDataLimit;

			}
			else //Connection is a Wi-Fi connection. Data restrictions are not necessary. 
			{
				Is_Wifi_Connected = true;
			}
		}

		static public bool Is_Wifi_Connected { get; private set; }

		static public bool Is_OverDataLimit { get; private set; }

		static public bool Is_LowOnData { get; private set; }

		static public bool Is_Roaming { get; private set; }

		static public bool Is_InternetAvailable { get; private set; }

		static public bool Is_Connected { get; private set; }

		public virtual async Task<string> Download(string uri)
		{
			try
			{
				var httpClient = new HttpClient();
				// Increase the max buffer size for the response so we don't get an exception with so many web sites

				httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
				httpClient.MaxResponseContentBufferSize = 256000000;
				httpClient.DefaultRequestHeaders.Add("user-agent",
					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
				response.EnsureSuccessStatusCode();

				return await response.Content.ReadAsStringAsync();
			}
			catch (Exception e)
			{
#if BETA
				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
#endif
				throw new TaskCanceledException(e.Message, e);
			}
		}

		public virtual async Task Download(string uri, string file, TypeFolder folder)
		{
			try
			{
				var httpClient = new HttpClient();
				// Increase the max buffer size for the response so we don't get an exception with so many web sites

				httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
				httpClient.MaxResponseContentBufferSize = 256000000;
				httpClient.DefaultRequestHeaders.Add("user-agent",
					"Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

				HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
				response.EnsureSuccessStatusCode();

				await FileHelper.WriteTextAsync(folder, file, await response.Content.ReadAsStringAsync());
			}
			catch (Exception e)
			{
#if BETA
				Logger.Log().WriteLineTime("Can't download " + uri).WriteLine(e.Message).WriteLine(e.StackTrace);
#endif
				throw new TaskCanceledException(e.Message, e);
			}
		}

	}
}
