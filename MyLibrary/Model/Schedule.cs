using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace MinskTrans.DesctopClient.Model
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Schedule : BaseModel
	{
		[JsonConstructor]
		public Schedule()
		{
			
		}

		public String InicializeString
		{
			get {StringBuilder result = new StringBuilder();
				foreach (var s in splitStr)
				{
					result.Append(s).Append("!!!");
				}
				return result.ToString();
			}
			set { splitStr = value.Split(new[] {"!!!"}, StringSplitOptions.RemoveEmptyEntries); }
		}
		[JsonProperty]
		public string[] splitStr;
		private List<List<Time>> timesDictionary;

		public Schedule(string str, bool lazyInicialize = false)
		{
			char sym = ',';

			//Route id
			splitStr = str.Split(new[] { ",," }, StringSplitOptions.RemoveEmptyEntries);
			Inicialize(splitStr[0], ",");
			RoutId = GetInt().Value;

			if (!lazyInicialize)
				InicializeTime();
		}
		
		void InicializeTime()
		{
			char sym = ',';
			var timesDictionary = new List<Time>();

			TimesDictionary = new List<List<Time>>();

			if (splitStr == null)
				return;
			//Route id
			
			int val = 0;
			int hour = 0;


			var list = new List<List<int>>();
			list.Add(new List<int>());
			List<int> listValue = list[0];
			Inicialize(splitStr[0], ",");
			var tmp = GetInt().Value;
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
					timesDictionary.Add(new Time { Days = curValue, Times = list[i++], Schedule = this });
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
		[JsonProperty]
		public int RoutId { get; set; }
		
		public Rout Rout { get; set; }

		/// <summary>
		///     first - stops, second - varios days
		/// </summary>
		/// 
		
		public List<List<Time>> TimesDictionary
		{
			get
			{
				if (timesDictionary == null)
					InicializeTime();
				return timesDictionary;
			}
			private set { timesDictionary = value; }
		}

		public List2<Rout, int> GetListTimes(int stop, int day, int startedTime, int endTime = int.MaxValue)
		{
			var result = new List2<Rout, int>();
			if (TimesDictionary.Count <= 0)
				return result;
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