using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

	}
}
