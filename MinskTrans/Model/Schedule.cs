using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.Model;

namespace MinskTrans
{
	public class Schedule : BaseModel
	{
		

		public Schedule(string str)
		{
			
			char sym = ',';
			var timesDictionary = new List<Time>();

			TimesDictionary = new List<List<Time>>();

			//Route id
			var splitStr = str.Split(new[] {",,"}, StringSplitOptions.RemoveEmptyEntries);
			Inicialize(splitStr[0], ",");
			RoutId = GetInt().Value;
			int val = 0;
			int hour = 0;


			List<List<int>> list = new List<List<int>>();
			list.Add(new List<int>());
			var listValue = list[0];
			var dictionary = new Dictionary<int, List<int>>();

			while (true)// парсинг времени
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
					timesDictionary.Add(new Time() {Days = curValue, Times = list[i++], Schedule = this});
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
		
		/// <summary>
		///first - stops, second - varios days 
		/// </summary>
		public List<List<Time>> TimesDictionary { get; set; }

		public List2<Rout, int> GetListTimes(int stop,int day, int startedTime, int endTime = int.MaxValue)
		{
			var result =new List2<Rout, int>();
			var temp = TimesDictionary[stop].Where(x => x.Days.Contains(day.ToString()));
			foreach (var time in temp)
			{
				foreach (var tm in time.Times.Where(x=>x >= startedTime && x <= endTime))
				{
					if (tm >= startedTime)	
						result.Add(new KeyValuePair<Rout, int>(Rout, tm));
				}
			}
			return result;
		}
	}
}
