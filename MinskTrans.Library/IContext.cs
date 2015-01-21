namespace MinskTrans.Library
{
	public interface IContext
	{
		void Create();
		void DownloadUpdate();
		bool HaveUpdate();
		void Save();
		void Load();
	}
}