using System;
using System.Threading.Tasks;
using MinskTrans.Utilites.Base.IO;

namespace MinskTrans.Utilites.Base.Net
{
	public interface ICloudStorageController
	{
		void Inicialize();
		Task UploadFileAsync(TypeFolder pathToFile, string newNameFile);

		event EventHandler<EventArgs> NeedAttention;
	}
}
