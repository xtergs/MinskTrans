﻿using System.Collections.Generic;

namespace MinskTrans
{
	public class Time
	{
		public Time()
		{
		}

		public Time(Time time, int correnction)
		{
			Times = new List<int>();
			Days = time.Days;
			for (int i = 0; i < time.Times.Count; i++)
			{
				Times.Add(time.Times[i] + correnction);
			}
			Schedule = time.Schedule;
		}

		public string Days { get; set; }
		public List<int> Times { get; set; }

		public Dictionary<int, List<int>> DictionaryTime
		{
			get
			{
				var dic = new Dictionary<int, List<int>>();
				int val = 0;
				for (int i = 0; i < Times.Count; i++)
				{
					val = Times[i];
					int hour = val/60;
					int min = val - hour*60;

					if (dic.ContainsKey(hour))
						dic[hour].Add(min);
					else
					{
						dic.Add(hour, new List<int>());
						dic[hour].Add(min);
					}
				}
				return dic;
			}
		}

		public Schedule Schedule { get; set; }
	}
}