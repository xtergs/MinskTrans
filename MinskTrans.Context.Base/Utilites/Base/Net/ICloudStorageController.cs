using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MinskTrans.Utilites.Base.IO;

namespace MinskTrans.Utilites.Base.Net
{
	public interface ICloudStorageController
	{
		Task Inicialize();
		Task<KeyValuePair<string,string>>  UploadFileAsync(TypeFolder pathToFile, string newNameFile);

		event EventHandler<EventArgs> NeedAttention;
	}
}
