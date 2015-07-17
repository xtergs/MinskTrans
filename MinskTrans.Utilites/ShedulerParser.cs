using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.DesctopClient;
using MinskTrans.Net.Base;
using MinskTrans.Utilites.Model;

namespace MinskTrans.Utilites
{
	public class ShedulerParser : ITimeTableParser
	{
		public IEnumerable<Stop> ParsStops(string stops, IContext context = null)
		{
			int indexEnd = stops.IndexOf(' ');
			var resultList = new List<Stop>();

			//while (indexEnd <= stops.Length)
			//{
			//	indexEnd++;
			//	int indexStart = indexEnd;
			//	for (int i = 0; i < 6; i++)
			//	{
			//		indexEnd = stops.IndexOf(';', indexEnd + 1);
			//	}

			//	indexEnd = stops.IndexOf(' ', indexEnd + 1);
			//	indexEnd--;
			//	if (indexEnd < 0)
			//	{
			//		resultList.Add(new Stop(stops.Substring(indexStart)));
			//		break;
			//	}
			//	else
			//		resultList.Add(new Stop(stops.Substring(indexStart, indexEnd - indexStart)));
			//}

			string[] listStr = stops.Split(new[] {'\n'});

			Stop stopOld = null;
			for (int i = 1; i < listStr.Length; i++)
			{
				//if (i % 100 == 0)
				//	OnLogMessage(i + " strings are parsed");
				if (!String.IsNullOrWhiteSpace(listStr[i]))
				{
					if (context == null)
						resultList.Add(new Stop(listStr[i], stopOld));
					else
						resultList.Add(new StopEx(context, listStr[i], stopOld));
					stopOld = resultList[resultList.Count - 1];
				}
			}
			Parallel.ForEach(resultList, stop =>
			{
				var strList = stop.StopsStr.Split(',');
				foreach (string stopId in strList)
				{
					if (string.IsNullOrWhiteSpace(stopId))
						break;
					int id = int.Parse(stopId);
					try
					{
					stop.Stops.Add(resultList.First(x => x.ID == id));

					}
					catch (InvalidOperationException)
					{
						continue;
					}
				}

			});
			return resultList;
		}

		public IEnumerable<Rout> ParsRout(string routs, IContext context = null)
		{
			int indexEnd = routs.IndexOf(' ');
			var resultList = new List<Rout>();

			//while (indexEnd <= routs.Length)
			//{
			//	indexEnd++;
			//	int indexStart = indexEnd;
			//	for (int i = 0; i < 17; i++)
			//	{
			//		indexEnd = routs.IndexOf(';', indexEnd + 1);
			//	}
			//	int temp = routs.IndexOf(';', indexEnd + 1);

			//	if (indexEnd < 0)
			//	{
			//		resultList.Add(new Rout(routs.Substring(indexStart)));
			//		break;
			//	}
			//	else
			//	{
			//		indexEnd = routs.LastIndexOf(' ', indexEnd, temp);
			//		indexEnd--;
			//		resultList.Add(new Rout(routs.Substring(indexStart, indexEnd - indexStart+1)));
			//	}
			//}

			string[] listStr = routs.Split(new []{'\n'});
			Rout rout = null;
			for (int i = 1; i < listStr.Length; i++)
			{
				//if (i%50 == 0)
				//	OnLogMessage(i+ "routs pares");
				if (!String.IsNullOrWhiteSpace(listStr[i]))
				{
					if (context == null)
						resultList.Add(new Rout(listStr[i], rout));
					else
						resultList.Add(new RoutEx(context, listStr[i], rout));
					rout = resultList[resultList.Count - 1];
				}
			}

			return resultList;
		}

		public IEnumerable<Schedule> ParsTime(string times)
		{
			//int indexEnd = times.IndexOf(' ');
			var resultList = new List<Schedule>();

			//while (indexEnd <= routs.Length)
			//{
			//	indexEnd++;
			//	int indexStart = indexEnd;
			//	for (int i = 0; i < 17; i++)
			//	{
			//		indexEnd = routs.IndexOf(';', indexEnd + 1);
			//	}
			//	int temp = routs.IndexOf(';', indexEnd + 1);

			//	if (indexEnd < 0)
			//	{
			//		resultList.Add(new Rout(routs.Substring(indexStart)));
			//		break;
			//	}
			//	else
			//	{
			//		indexEnd = routs.LastIndexOf(' ', indexEnd, temp);
			//		indexEnd--;
			//		resultList.Add(new Rout(routs.Substring(indexStart, indexEnd - indexStart+1)));
			//	}
			//}

			string[] listStr = times.Split('\n');

			try
			{
				resultList.AddRange(from t in listStr.AsParallel().Skip(1) where !String.IsNullOrWhiteSpace(t) select new Schedule(t, true));
			}
			catch (Exception e )
			{
				throw;
			}


			return resultList;
		}


	}
}