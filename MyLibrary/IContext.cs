using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient
{
	[Flags]
	public enum LoadType
	{
		LoadDB = 0x00000001,
		LoadFavourite = 0x00000001,
		LoadAll = LoadDB | LoadFavourite
	}
	public interface IContext
	{
		
		void Create(bool a = true);
		Task<bool> DownloadUpdate();
		Task<bool> HaveUpdate(string a, string b, string c, bool t);
		Task ApplyUpdate();
		Task Save(bool saveAllDB = true);
		Task Load(LoadType type = LoadType.LoadAll);
	}
}