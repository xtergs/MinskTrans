using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace MinskTrans.Universal
{
	static class InternetHelper
	{
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
	}
}
