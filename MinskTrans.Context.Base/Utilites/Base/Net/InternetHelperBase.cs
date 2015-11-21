﻿using System;
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
		public InternetHelperBase(FileHelperBase fileHelper, ILogger logger)
		{
			if (fileHelper == null)
				throw new ArgumentNullException("fileHelper");
		    if (logger == null)
		        throw new ArgumentNullException("logger");
		    this.logger = logger;
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

			        httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
			        httpClient.MaxResponseContentBufferSize = 256000000;
			        httpClient.DefaultRequestHeaders.Add("user-agent",
			            "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

			        HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
			        response.EnsureSuccessStatusCode();

			        return await response.Content.ReadAsStringAsync();
			    }
			}
            catch (TimeoutException e)
            {
                logger.Error("InternetHelperBase Download: can't download " + uri,e);
                throw;
            }
            catch (HttpRequestException e)
            {
                logger.Error("InternetHelperBase Download: can't download " + uri, e);
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

		            httpClient.Timeout = new TimeSpan(0, 0, 10, 0);
		            httpClient.MaxResponseContentBufferSize = 256000000;
		            httpClient.DefaultRequestHeaders.Add("user-agent",
		                "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

		            HttpResponseMessage response = await httpClient.GetAsync(new Uri(uri));
		            response.EnsureSuccessStatusCode();
		            await FileHelper.WriteTextAsync(folder, file, await response.Content.ReadAsStringAsync());
		        }
		    }
            catch (TimeoutException e)
            {
                logger.Error("InternetHelperBase Download: can't download " + uri, e);
                throw;
            }
            catch (HttpRequestException e)
            {
                logger.Error("InternetHelperBase Download: can't download " + uri, e);
                throw;
            }
            catch (Exception e)
            {
                logger.Error("InternetHelperBase Download: can't download " + uri, e);
                throw new TaskCanceledException(e.Message, e);
            }
        }

	}
}
