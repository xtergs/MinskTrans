using System;
using System.Collections.Generic;
using System.IO;
using MinskTrans.Utilites.Base.IO;
using MinskTrans.Utilites.Desktop;

namespace DesktopUnitTests
{
	class TestFileHelperDesktop : FileHelperDesktop
	{
		protected override Dictionary<TypeFolder, string> Folders
		{
			get
			{
				if (folders == null)
					folders = new Dictionary<TypeFolder, string>()
		{
			{TypeFolder.Local, Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Test")},
			{TypeFolder.Roaming, Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) , "Test")},
			{TypeFolder.Temp, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), "Test")},
			{TypeFolder.Current, Path.Combine(Directory.GetCurrentDirectory(), "TestsEnviroment") }
		};
				return folders;
			}
		}
		private  Dictionary<TypeFolder, string> folders;

	}
}
