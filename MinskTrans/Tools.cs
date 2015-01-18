namespace MinskTrans
{
	internal class Tools
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
	}
}