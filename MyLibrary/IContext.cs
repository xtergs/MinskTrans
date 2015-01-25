using System.Threading.Tasks;

namespace MinskTrans.DesctopClient
{
	public interface IContext
	{
		void Create(bool a = true);
		void DownloadUpdate();
		Task<bool> HaveUpdate();
		void ApplyUpdate();
		void Save();
		void Load();
	}
}