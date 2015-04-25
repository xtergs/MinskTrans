using System.Threading.Tasks;

namespace MinskTrans.DesctopClient
{
	public interface IContext
	{
		void Create(bool a = true);
		Task<bool> DownloadUpdate();
		Task<bool> HaveUpdate(string a, string b, string c, bool t);
		Task ApplyUpdate();
		Task Save();
		Task Load();
	}
}