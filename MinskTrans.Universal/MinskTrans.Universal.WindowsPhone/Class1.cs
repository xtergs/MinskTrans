using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage.Streams;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace MinskTrans.Universal
{
	public static class IsolatedStorageOperations
	{
		public static async Task Save<T>(this T obj, string file)
		{
			await Task.Run(async () =>
			{
				var storage = ApplicationData.Current.LocalFolder;
				StorageFile stream = null;

				try
				{
					stream = await storage.CreateFileAsync(file+".temp", CreationCollisionOption.ReplaceExisting);

					var serializer = new XmlSerializer(typeof(T));
					serializer.Serialize(await stream.OpenStreamForWriteAsync(), obj);
				}
				catch (Exception e)
				{
					throw new Exception(e.Message, e.InnerException);
				}
				//finally
				//{
				//	if (stream != null)
				//	{
				//		//stream.Close();
				//		//stream.Dispose();
				//	}
				//}
#if DEBUG
				//var newFile = await ApplicationData.Current.LocalFolder.GetFileAsync(file+".temp");
				var str = await FileIO.ReadTextAsync(stream);
#endif
				stream.MoveAsync(ApplicationData.Current.LocalFolder, file, NameCollisionOption.ReplaceExisting);
			});
		}

		public static async Task<T> Load<T>(string file)
		{

			var storage = ApplicationData.Current.LocalFolder;
			T obj = Activator.CreateInstance<T>();

			//if (storage.FileExists(file))
			{
				//IsolatedStorageFileStream stream = null;
				try
				{
					var stream =await storage.OpenStreamForReadAsync(file);
					XmlSerializer serializer = new XmlSerializer(typeof(T));

					obj = (T)serializer.Deserialize(stream);
				}
				catch (Exception e)
				{
				}
				finally
				{
					
					//if (stream != null)
					//{
					//	stream.Close();
					//	stream.Dispose();
					//}
				}
				return obj;
			}
			//await obj.Save(file);
			return obj;
		}
	}
}
