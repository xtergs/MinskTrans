using MyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushNotificationServer.CloudStorage
{
	public interface ICloudStorageController
	{
		void Inicialize();
		Task UploadFileAsync(TypeFolder pathToFile, string newNameFile);

		event EventHandler<EventArgs> NeedAttention;
	}
}
