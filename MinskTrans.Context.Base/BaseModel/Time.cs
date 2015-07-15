using System.Collections.Generic;
using System.Text;

namespace MinskTrans.Context.Base.BaseModel
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Time : BaseModel
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
		[JsonProperty]
		public string Days { get; set; }

		
		public Dictionary<int, string> daysToString = new Dictionary<int, string>()
		{
			{1, "Пн"},
			{2, "Вт"},
			{3, "Ср"},
			{4, "Чт"},
			{5, "Пт"},
			{6, "Сб"},
			{7, "Вс"}
		};
		
		public string DaysStr
		{
			get
			{
				var strBuilder = new StringBuilder();
				foreach (var day in Days)
				{
					strBuilder.Append(daysToString[int.Parse(day.ToString())] + " ");
				}
				return strBuilder.ToString();
			}
		}
		[JsonProperty]
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
					if (hour >= 24)
						hour -= 24;

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

		public string ToStrTable
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				foreach (var dictionary in DictionaryTime)
				{
					builder.Append(dictionary.Key + ":");
					foreach (var i in dictionary.Value)
					{
						builder.Append(i + ", ");
					}
					builder.Remove(builder.Length - 2, 2);
					builder.Append("\n");
				}
				return builder.ToString();
			}
		}

		public Schedule Schedule { get; set; }

		
	}
}