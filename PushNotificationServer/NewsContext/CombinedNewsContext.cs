using System.Threading.Tasks;
using MinskTrans.Net;
using MinskTrans.Utilites.Base.IO;

namespace PushNotificationServer.NewsContext
{
	public class CombinedNewsContext : INewsContext
	{
		private readonly INewsContext _primary;
		private readonly INewsContext _backup;
		public ListWithDate MainNews => _primary.MainNews;
		public ListWithDate HotNews => _primary.HotNews;

		public CombinedNewsContext(INewsContext primary, INewsContext backup)
		{
			_primary = primary;
			_backup = backup;
		}

		public async Task LoadDataAsync(TypeFolder folder, string file)
		{
			await _primary.LoadDataAsync(folder, file);
		}

		public async Task LoadDataAsync(ListWithDate mainnews, ListWithDate hotnews)
		{
			
		}

		public async Task Save(TypeFolder folder, string file)
		{
			await _primary.Save(folder, file);
			await _backup.LoadDataAsync(MainNews, HotNews);
			await _backup.Save(folder, file);
		}

		public async Task Clear(TypeFolder folder)
		{
			await _primary.Clear(folder);
			await _backup.Clear(folder);
		}

		public string[] supportedVersions => _primary.supportedVersions;
	}
}
