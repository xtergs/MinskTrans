﻿using System;
using System.Collections.Generic;
using System.Linq;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient
{
	public class Schedule : BaseModel
	{
		private Schedule()
		{ }
		public Schedule(string str)
		{
			char sym = ',';
			var timesDictionary = new List<Time>();

			TimesDictionary = new List<List<Time>>();

			//Route id
			string[] splitStr = str.Split(new[] {",,"}, StringSplitOptions.RemoveEmptyEntries);
			Inicialize(splitStr[0], ",");
			RoutId = GetInt().Value;
			int val = 0;
			int hour = 0;


			var list = new List<List<int>>();
			list.Add(new List<int>());
			List<int> listValue = list[0];
			var dictionary = new Dictionary<int, List<int>>();

			while (true) // парсинг времени
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
				string curValue = GetStr();
				if (curValue == null)
					break;
				if (i >= list.Count)
					timesDictionary[timesDictionary.Count - 1].Days += curValue;
				else
					timesDictionary.Add(new Time {Days = curValue, Times = list[i++], Schedule = this});
				if (GetStr() == null)
					break;
			}

			TimesDictionary.Add(timesDictionary);


			for (int j = 4; j < splitStr.Count(); j++) //остановки
			{
				Inicialize(splitStr[j], ",");
				int? cor = GetInt();

				//list.ForEach(x=>x.ForEach(y=>y+=cor.Value));
				timesDictionary = new List<Time>();
				int counter = -1;
				bool change = true;
				int? addCor = 0;
				for (int ddd = 0; ddd < TimesDictionary[j - 4].Count; ddd++)
				{
					var tempTime = new Time();
					tempTime.Times = new List<int>();
					tempTime.Days = TimesDictionary[j - 4][ddd].Days;
					for (i = 0; i < TimesDictionary[j - 4][ddd].Times.Count; i++)
					{
						counter--;
						if (counter == 0)
						{
							if (change)
							{
								cor = addCor;
							}
							else
							{
								cor = addCor;
							}
							counter--;
							//change = !change;
						}
						if (counter < 0)
						{
							addCor = GetInt();
							if (addCor != null)
							{
								counter = addCor.Value;
								addCor = GetInt();
								if (addCor.Value == 6)
									addCor = cor + 1;
								else if (addCor.Value == 4)
									addCor = cor - 1;
								change = !change;
							}
						}
						tempTime.Times.Add(TimesDictionary[j - 4][ddd].Times[i] + cor.Value);
					}
					tempTime.Schedule = TimesDictionary[j - 4][ddd].Schedule;

					timesDictionary.Add(tempTime);
				}
				TimesDictionary.Add(timesDictionary);
				//timesDictionary.Add(new Time() { Days = curValue, Times = list[i++] });
			}

			//TimesDictionary.Add();
		}

		public int RoutId { get; set; }
		public Rout Rout { get; set; }

		/// <summary>
		///     first - stops, second - varios days
		/// </summary>
		public List<List<Time>> TimesDictionary { get; set; }

		public List2<Rout, int> GetListTimes(int stop, int day, int startedTime, int endTime = int.MaxValue)
		{
			var result = new List2<Rout, int>();
			IEnumerable<Time> temp = TimesDictionary[stop].Where(x => x.Days.Contains(day.ToString()));
			foreach (Time time in temp)
			{
				foreach (int tm in time.Times.Where(x => x >= startedTime && x <= endTime))
				{
					if (tm >= startedTime)
						result.Add(new KeyValuePair<Rout, int>(Rout, tm));
				}
			}
			return result;
		}
	}
}