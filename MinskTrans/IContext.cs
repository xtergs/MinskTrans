namespace MinskTrans.DesctopClient
{
	public interface IContext
	{
		void Create();
		void DownloadUpdate();
		bool HaveUpdate();
		void ApplyUpdate();
		void Save();
		void Load();
	}
}