using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MinskTrans.DesctopClient.Comparer;

namespace MinskTrans.DesctopClient
{
	public class CalculateRout
	{
	readonly Context context;

		public CalculateRout(Context newContext)
		{
			context = newContext;
		}

		private List<NodeGraph> meshGraphs;
		public void CreateGraph()
		{
			meshGraphs = new List<NodeGraph>(context.ActualStops.Count);
			foreach (var actualStop in context.ActualStops)
			{
				meshGraphs.Add(new NodeGraph(){Stop = actualStop});
			}

			foreach (var meshGraph in meshGraphs)
			{

				foreach (var rout in meshGraph.Stop.Routs)
				{
					if (rout == null)
						continue;
					int index = rout.Stops.IndexOf(meshGraph.Stop);
					if (index+1 >= rout.Stops.Count)
						continue;
					if (meshGraph.ConnectedStops.Any(x=>x.Stop == rout.Stops[index + 1]))
						continue;
					meshGraph.ConnectedStops.Add(meshGraphs.First(x=>x.Stop.ID == rout.Stops[index + 1].ID));
				}
				
			}
		}

		public Stack<KeyValuePair<Rout, IEnumerable<Stop>>> resultRout;
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
			listStop.Add(start);
			Stop startStop = start;
			resultRout = new Stack<KeyValuePair<Rout, IEnumerable<Stop>>>();
			foreach (var rout in context.Routs)
			{
				//if (rout.Stops.Any(stop => stop.ID == startStop.ID))
				//{
					var intersects = listStop.Intersect(rout.Stops.SkipWhile(stop=> stop != startStop), new StopComparer()).ToList();
#if DEBUG
					var stringListStop = listStop.Select(x=>x.Name).ToList();
					var stringRoutStops = rout.Stops.Select(x => x.Name).ToList();
					var intersectsStops = intersects.Select(x => x.Name).ToList();
#endif
					if (intersects.Count > 1 )
					{
						//startStop = intersects.Last();
						resultRout.Push(new KeyValuePair<Rout, IEnumerable<Stop>>(rout, intersects));
					}
				//}
			}

			//Second method
			resultRouts = new Stack<KeyValuePair<Rout, IEnumerable<Stop>>>();
			globalResult = new List<List<KeyValuePair<Rout, IEnumerable<Stop>>>>();
			bool method2 = FindRout(listStop, 0);

			

			return result;
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
			foreach (var connectedStop in start.ConnectedStops)
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
