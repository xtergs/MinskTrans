using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans
{
	public class Schedule
	{
		private string getIntStr = "";
		private int indexStart = 0;
		private int indexEnd = 0;
		private string Sym = "";

		void Inicialize(string str, string sym)
		{
			getIntStr = str;
			Sym = sym;
			indexStart = 0;
		}
		int? GetInt()
		{
			var temp = GetStr();
			if (temp == null)
				return null;
			return int.Parse(temp);
		}

		string GetStr()
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

		public Schedule(string str)
		{
			
			char sym = ',';
			var timesDictionary = new List<Time>();

			TimesDictionary = new List<List<Time>>();

			
			var splitStr = str.Split(new[] {",,"}, StringSplitOptions.RemoveEmptyEntries);
			Inicialize(splitStr[0], ",");
			RoutId = GetInt().Value;
			int val = 0;
			int hour = 0;


			List<List<int>> list = new List<List<int>>();
			list.Add(new List<int>());
			var listValue = list[0];
			var dictionary = new Dictionary<int, List<int>>();

			while (true)
			{
				int? curValue = GetInt();
				if (curValue == null)
					break;
				if (curValue < 0)
				{
					listValue = new List<int>();
					list.Add(listValue);
				}
				val += curValue.Value;
				listValue.Add(val);
			}
			
			Inicialize(splitStr[3], ",");
			int i = 0;
			while (true)
			{
				var curValue = GetStr();
				if (curValue == null)
					break;
				if (i >= list.Count)
					timesDictionary[timesDictionary.Count - 1].Days += curValue;
				else
					timesDictionary.Add(new Time() {Days = curValue, Times = list[i++]});
				if (GetStr() == null)
					break;
			}
			
			TimesDictionary.Add(timesDictionary);
			
			

			for (int j = 4; j < splitStr.Count(); j++)  //остановки
			{
				Inicialize(splitStr[j], ",");
				int? cor = GetInt();

				//list.ForEach(x=>x.ForEach(y=>y+=cor.Value));
					timesDictionary = new List<Time>();

					for (int ddd = 0; ddd < TimesDictionary[j-4].Count; ddd++)
					{
						
						timesDictionary.Add(new Time(TimesDictionary[j-4][ddd], cor.Value));
						
					}
					TimesDictionary.Add(timesDictionary);
				//timesDictionary.Add(new Time() { Days = curValue, Times = list[i++] });
			}

			//TimesDictionary.Add();



			
		}

		public int RoutId { get; set; }
		public Rout Rout { get; set; }
		// first - stops, second - varios days
		public List<List<Time>> TimesDictionary { get; set; } 
	}
}
