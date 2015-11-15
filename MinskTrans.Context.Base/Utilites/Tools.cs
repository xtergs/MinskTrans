namespace MyLibrary
{
	public class Tools
	{
		private string Sym = "";
		private string getIntStr = "";
		private int indexEnd;
		private int indexStart;

		private void Inicialize(string str, string sym)
		{
			getIntStr = str;
			Sym = sym;
			indexStart = 0;
		}

		private int? GetInt()
		{
			string temp = GetStr();
			if (temp == null)
				return null;
			return int.Parse(temp);
		}

		private string GetStr()
		{
			indexEnd = getIntStr.IndexOf(Sym, indexStart);
			if (indexStart == indexEnd || indexStart < 0)
				return null;
			string temp;
			if (indexEnd < 0)
			{
				temp = getIntStr.Substring(indexStart);
				getIntStr = Sym;
				indexStart = 0;
				indexEnd = -1;
			}
			else
				temp = getIntStr.Substring(indexStart, indexEnd - indexStart);
			indexStart = indexEnd + 1;
			return temp;
		}

		public static string HourToStr(int hour)
		{
			if (hour%100 >= 10 && hour%100 <= 20)
				return "часов";
			if (hour%10 == 1)
				return "час";
			if (hour % 10 >= 5 && hour% 10 <= 9)
				return "часов";
			return "часа";
		}

		public static string MinsToStr(int mins)
		{
			if (mins > 10 && mins < 20)
				return "минут";
			if (mins % 10 == 1)
				return "минуту";
			if (mins%10 >= 2 && mins%10 <= 4)
				return "минуты";
			return "минут";
		}
	}
}