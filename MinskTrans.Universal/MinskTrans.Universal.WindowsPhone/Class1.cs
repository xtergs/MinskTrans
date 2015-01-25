using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Storage;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage.Streams;

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
					stream = await storage.CreateFileAsync(file);
					
					XmlSerializer serializer = new XmlSerializer(typeof(T));
					serializer.Serialize(await stream.OpenStreamForReadAsync(), obj);
				}
				catch (Exception)
				{
				}
				finally
				{
					if (stream != null)
					{
						//stream.Close();
						//stream.Dispose();
					}
				}
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
