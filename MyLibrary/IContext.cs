using MinskTrans.DesctopClient.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyLibrary
{
	[Flags]
	public enum LoadType
	{
		LoadDB = 0x00000001,
		LoadFavourite = 0x00000002,
		LoadAll = LoadDB | LoadFavourite
	}
	public interface IContext
	{
		
		void Create(bool a = true);
		
		Task<bool> HaveUpdate(IList<Rout> a, IList<Stop> b, IList<Schedule> c);
		Task ApplyUpdate(IList<Rout> a, IList<Stop> b, IList<Schedule> c);
		Task Save(bool saveAllDB = true);
		Task Load(LoadType type = LoadType.LoadAll);
	}
}