using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using MetroLog;
using MinskTrans.Utilites.Base.IO;
using MyLibrary;

//#if WINDOWS_PHONE_APP || WINDOWS_UAPWINDOWS_UAP

//#endif

namespace CommonLibrary.IO
{
    public class FileHelper : FileHelperBase
    {
        private ILogger log;

        public static readonly Dictionary<TypeFolder, IStorageFolder> Folders = new Dictionary
            <TypeFolder, IStorageFolder>()
        {
            {TypeFolder.Local, ApplicationData.Current.LocalFolder},
            {TypeFolder.Roaming, ApplicationData.Current.RoamingFolder},
            {TypeFolder.Temp, ApplicationData.Current.TemporaryFolder},
            {TypeFolder.Current, ApplicationData.Current.LocalFolder}
        };

        public ILogger Log
        {
            get { return log; }
            set { log = value; }
        }

        #region Overrides of FileHelperBase

        public override async Task<bool> FileExistAsync(TypeFolder folder, string file)
        {
            try
            {
                await Folders[folder].GetFileAsync(file);
                return true;
            }
            catch (FileNotFoundException)
            {
                log?.Debug($"{nameof(FileExistAsync)}: fileNotFound, file: {file}, folder:{folder}");
                return false;
            }
        }

        #endregion

        #region Overrides of FileHelperBase

        public override async Task SafeMoveAsync(TypeFolder folder, string @from, string to)
        {
            try
            {
                var file = await Folders[folder].GetFileAsync(to);
                await file.RenameAsync(to + OldExt, NameCollisionOption.ReplaceExisting);
            }
            catch (FileNotFoundException)
            {
            }
            try
            {
                await (await Folders[folder].GetFileAsync(from)).RenameAsync(to, NameCollisionOption.ReplaceExisting);
            }
            catch (FileNotFoundException)
            {
                log?.Debug("SafeMoveSync: moving file " + from + " not found");
            }
        }

        public override async Task<string> ReadAllTextAsync(TypeFolder folder, string file, string subfolder = "")
        {
            IStorageFolder storage = Folders[folder];

            if (subfolder != "")
            {
                storage = await storage.GetFolderAsync(subfolder);
            }
            IStorageFile sfile = await storage.GetFileAsync(file);
            return await FileIO.ReadTextAsync(sfile);
        }

        public override async Task<FluentFileHelperBase> WriteTextAsync(TypeFolder folder, string file, string text)
        {
            if (text == null)
                throw new ArgumentNullException(file);
            await
                FileIO.WriteTextAsync(
                    await Folders[folder].CreateFileAsync(file, CreationCollisionOption.ReplaceExisting), text);
            return new FluentFileHelperBase(this, folder, file);
        }

        public override async Task WriteTextAsync(TypeFolder folder, string file, Stream text)
        {
            if (text == null)
                throw new ArgumentNullException(file);
            var stream = await
                (await Folders[folder].CreateFileAsync(file, CreationCollisionOption.ReplaceExisting))
                    .OpenStreamForWriteAsync();
            await text.CopyToAsync(stream);
            stream.Dispose();
        }

        public override async Task DeleteFile(TypeFolder folder, string file)
        {
            try
            {
                await (await Folders[folder].GetFileAsync(file)).DeleteAsync(StorageDeleteOption.Default);
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine($"Not found file for deletion: {e.FileName}\n{e.Message}");
                //nothing to delete
            }
        }

        public override async Task DeleteFolder(TypeFolder folder, string folders)
        {
            await (await Folders[folder].GetFolderAsync(folders)).DeleteAsync(StorageDeleteOption.PermanentDelete);
        }

        public override async Task<IList<string>> GetNamesFiles(TypeFolder folder, string subFolder)
        {
            var folderr = (await Folders[folder].GetFolderAsync(subFolder));
            var files = (await folderr.GetFilesAsync());
            return files.Select(file => file.Name).ToList();
        }

        public override async Task<IList<string>> GetNamesFolder(TypeFolder folder)
        {
            var folderr = (await Folders[folder].GetFoldersAsync());
            return folderr.Select(file => file.Name).ToList();
        }

        public override string GetPath(TypeFolder folder)
        {
            return Folders[folder].Path;
        }

        public override async Task<Stream> OpenStream(TypeFolder folder, string file)
        {
            return await Folders[folder].OpenStreamForReadAsync(file);
        }

        #endregion
    }
}