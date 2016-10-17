using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using MetroLog;
using MinskTrans.Net;
using MinskTrans.Utilites.Base.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace PushNotificationServer
{

    public class NoSqlNewsContext : INewsContext
    {
        public string[] supportedVersions => new[] { "4" };
        private ILogger log;
        private FileHelperBase fileHelerp;
        public NoSqlNewsContext(FileHelperBase fileHelper, ILogManager logManager)
        {
            this.fileHelerp = fileHelper;
            this.log = logManager.GetLogger<BaseNewsContext>();
        }
        public ListWithDate MainNews { get; private set; }
        public ListWithDate HotNews { get; private set; }

        readonly object saveLocker = new object();

        public async Task LoadDataAsync(TypeFolder folder, string file)
        {
            if (!await fileHelerp.FileExistAsync(folder, file).ConfigureAwait(false))
            {
                MainNews = new ListWithDate();
                HotNews = new ListWithDate();
                return;
            }
            try
            {
                JsonSerializer serializer = GetSerializer();
                NewsSaveStruct result = null;
                var open = await fileHelerp.OpenStream(folder, file).ConfigureAwait(false);
                lock (saveLocker)
                {
                    using (var stream = open)
                    {
                        stream.Position = 0;
                        using (GZipStream zip = new GZipStream(stream, CompressionMode.Decompress, true))
                        using (BsonReader reader = new BsonReader(zip))
                        {
                            result = serializer.Deserialize<NewsSaveStruct>(reader);
                        }
                    }
                    SetSaveStruct(result);
                }
            }
            catch (Exception e)
            {
                MainNews = new ListWithDate();
                HotNews = new ListWithDate();
                Debug.WriteLine("NewsContext: not corrent json format data");
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        private JsonSerializer GetSerializer()
        {
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer;
            }
            catch (Exception e)
            {
                throw;
            }
        }


        protected virtual void SetSaveStruct(NewsSaveStruct st)

        {
            MainNews = st?.MainNews ?? new ListWithDate();
            HotNews = st?.HotNews ?? new ListWithDate();
        }
        protected virtual NewsSaveStruct GetSaveStruct()
        {
            return new NewsSaveStruct { HotNews = HotNews, MainNews = MainNews };
        }

        public Task Save(TypeFolder folder, string file)
        {
            using (var db = new LiteDatabase(@"MyData.db"))
            {
                // Get customer collection
                var customers = db.GetCollection<ListWithDate>("MainNews");

                // Create your new customer instance
                var instance = GetSaveStruct();

                // Insert new customer document (Id will be auto-incremented)
                customers = db.GetCollection<ListWithDate>("HotNews");
                customers.Insert(instance.HotNews);
                customers.Update(instance.MainNews);
                // Update a document inside a collection



            }

            try
            {
                string str = "";


                JsonSerializer serializer = GetSerializer();
                lock (saveLocker)
                {
                    MemoryStream ms = new MemoryStream();
                    using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                    using (BsonWriter writer = new BsonWriter(zip))
                    {
                        serializer.Serialize(writer, GetSaveStruct());
                        writer.CloseOutput = false;
                        writer.Flush();
                        zip.Flush();
                    }
                    ms.Position = 0;
                    return fileHelerp.WriteTextAsync(folder, file, ms);

                }
            }
            catch (JsonSerializationException e)
            {
                log.Error($"News context: can't serialize data:{e.Message} \n {MainNews}{HotNews}");
            }
            return null;
        }

        public Task Clear(TypeFolder Folder)
        {
            HotNews = new ListWithDate();
            MainNews = new ListWithDate();
            return fileHelerp.ClearFolder(Folder);
        }
    }
}
