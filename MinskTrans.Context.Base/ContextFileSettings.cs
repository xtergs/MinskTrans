using System;
using System.Collections.Generic;

using MinskTrans.Utilites.Base.IO;

namespace MinskTrans.Context
{
	public enum TypeSaveData
	{
		DB,
		Favourite,
		Statisticks
	}

	public class ContextFileSettings
	{
		public Dictionary<TypeSaveData, TypeFolder> folders = new Dictionary<TypeSaveData, TypeFolder>()
		{
			{TypeSaveData.DB, TypeFolder.Local},
			{TypeSaveData.Favourite, TypeFolder.Roaming},
			{TypeSaveData.Statisticks, TypeFolder.Roaming}

		};

		private string nameFileFavourite;
		private string nameFileRouts;
		private string nameFileStops;
		private string nameFileTimes;
		private string nameFileCounter;

		public string NameFileFavourite
		{
			get
			{
				if (String.IsNullOrWhiteSpace(nameFileFavourite))
					nameFileFavourite = "data.dat";
				return nameFileFavourite;
			}
			set { nameFileFavourite = value; }
		}

		public string NameFileRouts
		{
			get
			{
				if (string.IsNullOrWhiteSpace(nameFileRouts))
					nameFileRouts = "dataRouts.dat";
				return nameFileRouts;
			}
			set { nameFileRouts = value; }
		}

		public string NameFileStops
		{
			get
			{
				if (string.IsNullOrWhiteSpace(nameFileStops))
					nameFileStops = "dataStops.dat";
				return nameFileStops;
			}
			set { nameFileStops = value; }
		}

		public string NameFileTimes
		{
			get
			{
				if (string.IsNullOrWhiteSpace(nameFileTimes))
					nameFileTimes = "dataTimes.dat";
				return nameFileTimes;
			}
			set { nameFileTimes = value; }
		}

		public string NameFileCounter
		{
			get
			{
				if (string.IsNullOrWhiteSpace(nameFileTimes))
					nameFileCounter = "counters.dat";
				return nameFileCounter;
			}
			set { nameFileCounter = value; }
		}
	}
}
