using System.Threading.Tasks;

namespace MinskTrans.DesctopClient
{
	public interface IContext
	{
		void Create(bool a = true);
		Task DownloadUpdate();
		Task<bool> HaveUpdate(string a, string b, string c);
		Task ApplyUpdate();
		void Save();
		Task Load();
	}
}