using System;
using System.Net.Http;
using System.Threading.Tasks;
using MetroLog;
using MinskTrans.Utilites.Base.IO;

namespace MinskTrans.Utilites.Base.Net
{
	public abstract class  InternetHelperBase
	{
		public readonly FileHelperBase FileHelper;
	    private readonly ILogger logger;
		public InternetHelperBase(FileHelperBase fileHelper, ILogManager logger)
		{
			if (fileHelper == null)
				throw new ArgumentNullException(nameof(fileHelper));
		    if (logger == null)
		        throw new ArgumentNullException(nameof(logger));
		    this.logger = logger.GetLogger<InternetHelperBase>();
			FileHelper = fileHelper;
		}

		public abstract void UpdateNetworkInformation();
		static public bool Is_Wifi_Connected { get; protected set; }

		static public bool Is_OverDataLimit { get; protected set; }

		static public bool Is_LowOnData { get; protected set; }

		static public bool Is_Roaming { get; protected set; }

		static public bool Is_InternetAvailable { get; protected set; }

		static public bool Is_Connected { get; protected set; }

		public virtual async Task<string> Download(string uri)
		{
			try
			{
			    using (var httpClient = new HttpClient())
			    {
			        // Increase the max buffer size for the response so we don't get an exception with so many web sites
					logger.Debug($"Download: HttpClient creted, uri: {uri}");
			        httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
			        httpClient.MaxResponseContentBufferSize = 256000000;
			        httpClient.DefaultRequestHeaders.Add("user-agent",
			            "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

			        HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
			        response.EnsureSuccessStatusCode();
					logger.Debug($"Download: get response, uri: {uri}");
			        var res =  await response.Content.ReadAsStringAsync();
				    logger.Debug("Downlaod: Responsesuccessfuly readed");
				    return res;

			    }
			}
            catch (TimeoutException e)
            {
                logger.Error("InternetHelperBase Download: Timeout, can't download " + uri,e);
                throw;
            }
            catch (HttpRequestException e)
            {
                logger.Error("InternetHelperBase Download: Request, can't download " + uri, e);
                throw;
            }
            catch (Exception e)
			{
                logger.Error("InternetHelperBase Download: can't download " + uri, e);
                throw new TaskCanceledException(e.Message, e);
			}
		}

		public virtual async Task Download(string uri, string file, TypeFolder folder)
		{
		    try
		    {
		        using (var httpClient = new HttpClient())
		        {
		            // Increase the max buffer size for the response so we don't get an exception with so many web sites
					logger.Debug($"Download: httpClient created, uri: {uri}, file:{file}, TypeFolder: {folder}");
		            httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
		            httpClient.MaxResponseContentBufferSize = 256000000;
		            httpClient.DefaultRequestHeaders.Add("user-agent",
		                "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

		            HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
		            response.EnsureSuccessStatusCode();
					logger.Debug($"Download: Get response, uri:{uri}");
		            await FileHelper.WriteTextAsync(folder, file, await response.Content.ReadAsStringAsync());
					logger.Debug($"Download: uri {uri} successfuly writed to file {file}");
		        }
		    }
            catch (TimeoutException e)
            {
                logger.Error("InternetHelperBase Download: timeout can't download " + uri, e);
                throw;
            }
            catch (HttpRequestException e)
            {
                logger.Error("InternetHelperBase Download: request can't download " + uri, e);
                throw;
            }
            catch (Exception e)
            {
                logger.Error($"InternetHelperBase Download: can't download uri:{uri}, file:{file}", e);
                throw new TaskCanceledException(e.Message, e);
            }
        }

	}
}
