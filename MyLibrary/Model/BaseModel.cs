using System;


namespace MinskTrans.DesctopClient.Model
{
	public class BaseModel
	{
		protected string Sym = "";
		protected string getIntStr = "";
		protected int indexEnd = 0;
		protected int indexStart = 0;

		protected virtual void Inicialize(string str, string sym)
		{
			getIntStr = str;
			Sym = sym;
			indexStart = 0;
		}

		protected int? GetInt()
		{
			string temp = GetStr();
			if (temp == null)
				return null;
			return int.Parse(temp);
		}

		protected virtual string GetStr()
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

		
		//public event PropertyChangedEventHandler PropertyChanged;

		//[NotifyPropertyChangedInvocator]
		//protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		//{
		//	var handler = PropertyChanged;
		//	if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		//}
	}
}