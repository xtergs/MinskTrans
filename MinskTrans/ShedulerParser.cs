using System;
using System.Collections.Generic;
using System.Linq;

namespace MinskTrans.DesctopClient
{
	public class ShedulerParser
	{
		public static List<Stop> ParsStops(string stops)
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

			string[] listStr = stops.Split('\n');

			Stop stopOld = null;
			for (int i = 1; i < listStr.Length; i++)
			{
				if (!String.IsNullOrWhiteSpace(listStr[i]))
				{
					resultList.Add(new Stop(listStr[i], stopOld));
					stopOld = resultList[resultList.Count - 1];
				}
			}
			foreach (Stop stop in resultList)
			{
				string[] strList = stop.StopsStr.Split(',');
				foreach (string stopId in strList)
				{
					if (string.IsNullOrWhiteSpace(stopId))
						break;
					int id = int.Parse(stopId);
					stop.Stops.Add(resultList.First(x => x.ID == id));
				}
			}

			return resultList;
		}

		public static List<Rout> ParsRout(string routs)
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

			string[] listStr = routs.Split('\n');
			Rout rout = null;
			for (int i = 1; i < listStr.Length; i++)
			{
				if (!String.IsNullOrWhiteSpace(listStr[i]))
				{
					resultList.Add(new Rout(listStr[i], rout));
					rout = resultList[resultList.Count - 1];
				}
			}

			return resultList;
		}

		public static List<Schedule> ParsTime(string times)
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

			for (int i = 1; i < listStr.Length; i++)
			{
				if (!String.IsNullOrWhiteSpace(listStr[i]))
					resultList.Add(new Schedule(listStr[i]));
			}

			return resultList;
		}
	}
}