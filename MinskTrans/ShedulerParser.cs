using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient
{
	public class ShedulerParser
	{
		public static List<Stop> ParsStops(string stops)
		{
			OnLogMessage("ParsStops Started");
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
					resultList.Add(new Stop(listStr[i], stopOld));
					stopOld = resultList[resultList.Count - 1];
				}
			}
			OnLogMessage("all strings are parsed");
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
					catch (InvalidOperationException e)
					{
						continue;
					}
				}

			});
			OnLogMessage("ParsStops ended");
			return resultList;
		}

		public static List<Rout> ParsRout(string routs)
		{
			OnLogMessage("ParsRout Started");
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
					resultList.Add(new Rout(listStr[i], rout));
					rout = resultList[resultList.Count - 1];
				}
			}
			OnLogMessage("ParsRouts Ended");

			return resultList;
		}

		public static List<Schedule> ParsTime(string times)
		{
			OnLogMessage("ParsTime Started");
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
			for (int i = 1; i < listStr.Length; i++)
			{
				//if (i % 50 == 0)
				//	OnLogMessage(i + "times parsed");
				if (!String.IsNullOrWhiteSpace(listStr[i]))
					resultList.Add(new Schedule(listStr[i]));
			}

			}
			catch (Exception e )
			{
				OnLogMessage("ParsTime error " + e.Message);
				throw;
			}

			OnLogMessage("ParsTimes ended");

			return resultList;
		}

		static public event LogDelegate LogMessage;

		static protected void OnLogMessage(string args)
		{
			LogDelegate handler = LogMessage;
			if (handler != null) handler(null, new LogDelegateArgs(){Message = args});
		}
	}
}