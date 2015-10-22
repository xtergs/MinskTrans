using System.Collections.Generic;
using System.Linq;
using MinskTrans.AutoRouting.Comparer;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.AutoRouting.AutoRouting
{
	public class CalculateRout
	{
	readonly IContext context;
		private readonly IDistanceCalculator calculatorDistance;
		public CalculateRout(IContext newContext)
		{
			context = newContext;
			calculatorDistance = new EquirectangularDistance();
		}

		private List<NodeGraph> meshGraphs;
		public void CreateGraph()
		{
			meshGraphs = new List<NodeGraph>(context.ActualStops.Count);
			foreach (var actualStop in context.ActualStops)
			{
				meshGraphs.Add(new NodeGraph(){Stop = actualStop});
			}

			foreach (var meshNode in meshGraphs)
			{

				foreach (var rout in meshNode.Stop.Routs)
				{
					if (rout == null)
						continue;
					int index = rout.Stops.IndexOf(meshNode.Stop);
					if (index+1 >= rout.Stops.Count)
						continue;
					if (meshNode.ConnectedStops.Any(x=>x.Stop == rout.Stops[index + 1]))
						continue;
					meshNode.ConnectedStops.Add(meshGraphs.First(x=>x.Stop.ID == rout.Stops[index + 1].ID));
				}
				foreach (
					var distanceNode in
						meshGraphs.Except(meshNode.ConnectedStops).Where(x=> x.Stop.ID != meshNode.Stop.ID).OrderBy(
							x => calculatorDistance.CalculateDistance(meshNode.Stop.Lat, meshNode.Stop.Lng, x.Stop.Lat, x.Stop.Lng))
							.Take(5)
							.ToList())
				{
					meshNode.ConnectedStops.Add(distanceNode);
				}

			}
		}

		public List<KeyValuePair<Rout, IEnumerable<Stop>>> resultRout;
		public Stack<KeyValuePair<Rout, IEnumerable<Stop>>> resultRouts;
		public List<List<KeyValuePair<Rout, IEnumerable<Stop>>>> globalResult;

		public bool FindPath(Stop start, Stop destin)
		{
			if (start == null || destin == null)
				return false;
			var startNode = meshGraphs.First(x => x.Stop.ID == start.ID);
			var endNode = meshGraphs.First(x => x.Stop.ID == destin.ID);
			ResultStopList = new Stack<NodeGraph>();
			var result = Findpath(startNode, endNode);
			var listStop = ResultStopList.Select(node=> node.Stop).ToList();
			listStop.Insert(0,start);
			Stop startStop = start;
			resultRout = new List<KeyValuePair<Rout, IEnumerable<Stop>>>();
			foreach (var rout in context.Routs.Where(rout => rout.Stops.Any(x => listStop.Any(d => d.ID == x.ID))))
			{
				//if (rout.Stops.Any(stop => stop.ID == startStop.ID))
				//{
				//.SkipWhile(stop=> stop != startStop)
				var intersects = listStop.Intersect(rout.Stops, new StopComparer()).ToList();
#if DEBUG
					var stringListStop = listStop.Select(x=>x.Name).ToList();
					var stringRoutStops = rout.Stops.Select(x => x.Name).ToList();
					var intersectsStops = intersects.Select(x => x.Name).ToList();
#endif
					if (intersects.Count > 1 )
					{
						//startStop = intersects.Last();
						resultRout.Add(new KeyValuePair<Rout, IEnumerable<Stop>>(rout, intersects));
					}
				//}
			}

			foreach (var stop in listStop)
			{
				ConnectStopsByWalk(stop, listStop.Where(x=> x.ID != stop.ID).ToList());
			}

			//Second method
			resultRouts = new Stack<KeyValuePair<Rout, IEnumerable<Stop>>>();
			globalResult = new List<List<KeyValuePair<Rout, IEnumerable<Stop>>>>();
			//bool method2 = FindRout(listStop, 0);

			

			return result;
		}

		void ConnectStopsByWalk(Stop stop, IEnumerable<Stop> listStops)
		{
			List<Stop> notConnectedStop = new List<Stop>();
			foreach (var listStop in listStops)
			{
				if (listStop.Routs.Any(x => stop.Routs.Contains(x)))
					return;
				else
				{
					notConnectedStop.Add(listStop);
				}
			}
			var tempList =
				notConnectedStop.OrderBy(x => calculatorDistance.CalculateDistance(stop.Lat, stop.Lng, x.Lat, x.Lng))
					.Take(1)
					.ToList();
			tempList.Add(stop);
					
            resultRout.Add(new KeyValuePair<Rout, IEnumerable<Stop>>(new Rout() {RouteName = "пешком"},tempList));
		}

		bool FindRout( IEnumerable<Stop> stops, int index)
		{
			if (stops == null || stops.Count() == 0)
				return false;
			if (index >= stops.Count())
				return false;
			int startIndex = index;
			Stop startStopRout = stops.ElementAt(index);
			foreach (var rout in startStopRout.Routs)
			{
				resultRouts = new Stack<KeyValuePair<Rout, IEnumerable<Stop>>>();
				index = startIndex;
				while (index < stops.Count() && rout.Stops.Any(stop => stop.ID == stops.ElementAt(index).ID))
				{
					index++;
				}
				var sequenceStops =
					stops.SkipWhile(stop => stop.ID != startStopRout.ID)
						.TakeWhile(stop => stop.ID != stops.ElementAt(index - 1).ID).Union(stops.Where(stop => stop.ID == stops.ElementAt(index - 1).ID)).ToList();
				
				if (sequenceStops.Count > 1)
					resultRouts.Push(new KeyValuePair<Rout, IEnumerable<Stop>>(rout, sequenceStops));
				if (sequenceStops.Last().ID == stops.Last().ID)
				{
					globalResult.Add(resultRouts.ToList());	
				}
				else
					if (FindRout(stops, index))
						return true;
				if (resultRouts.Count > 0)
					resultRouts.Pop();
			}
			return false;
		}

		

		private Stack<NodeGraph> ResultStopList;  

		bool Findpath(NodeGraph start, NodeGraph end)
		{
			start.Black = true;
			Stack<NodeGraph> stack = new Stack<NodeGraph>();
			foreach (var connectedStop in start.ConnectedStops.Where(x=> x.Black == false).OrderBy(x=> calculatorDistance.CalculateDistance(x.Stop.Lat, x.Stop.Lng, end.Stop.Lat, end.Stop.Lng)))
			{
				if (!connectedStop.Black)
					if (connectedStop != end)
					{
						connectedStop.Black = true;
						if (Findpath(connectedStop, end))
						{
							ResultStopList.Push(connectedStop);
							return true;
						}
					}
					else
					{
						ResultStopList.Push(connectedStop);
						return true;
					}
			}
			
			if (ResultStopList.Any())
				ResultStopList.Pop();
			return false;
		}
	}
}
