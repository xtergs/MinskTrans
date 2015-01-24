namespace MinskTrans.DesctopClient
{
	public interface IContext
	{
		void Create(bool a = true);
		void DownloadUpdate();
		bool HaveUpdate();
		void ApplyUpdate();
		void Save();
		void Load();
	}
}