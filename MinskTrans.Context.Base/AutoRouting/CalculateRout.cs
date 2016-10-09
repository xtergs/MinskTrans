using System.Collections.Generic;
using System.Linq;
using MinskTrans.AutoRouting.AutoRouting;
using MinskTrans.AutoRouting.Comparer;
using MinskTrans.Context.Base;
using MinskTrans.Context.Base.BaseModel;
using MinskTrans.Context.Comparer;

namespace MinskTrans.Context.AutoRouting
{
	public class CalculateParameters
	{
		public double HumanMultipl { get; set; } = 1.5;
		public int MaxChangeTransport { get; set; } = 10;
		public int MaxHumanDistanceM { get; set; } = 500;
		public int MaxHumanStops { get; set; } = 5;
		public double ChangeTransport { get; set; } = 1.2;
	}
	public class CalculateRout
	{
		readonly IContext context;
		private readonly IDistanceCalculator calculatorDistance;
		public CalculateParameters Params { get; set; }
		public CalculateRout(IContext newContext)
		{
			context = newContext;
			calculatorDistance = new EquirectangularDistance();
		}

		private List<NodeGraph> meshGraphs;
		public Dictionary<int, List<ConnectionInfo>> StopConnections;  
		public void CreateGraph()
		{
			if (Params == null)
				return;
			StopConnections = new Dictionary<int, List<ConnectionInfo>>(context.ActualStops.Count());
			meshGraphs = new List<NodeGraph>(context.ActualStops.Count());
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
					if (meshNode.ConnectedStops.Any(x=>x.Stop.Stop == rout.Stops[index + 1]))
						continue;

					var graph = new EdgeGraph(meshGraphs.First(x => x.Stop.ID == rout.Stops[index + 1].ID),
						calculatorDistance.CalculateDistance(meshNode.Stop, rout.Stops[index + 1]),
						ConnectionType.Transport);

					graph.Routs = meshNode.Stop.Routs.Intersect(graph.Stop.Stop.Routs).ToList();
					meshNode.ConnectedStops.Add(graph);
				}
				foreach (
					var distanceNode in
						meshGraphs.Except(meshNode.ConnectedStops.Select(x=> x.Stop))
							.Where(x=> x.Stop.ID != meshNode.Stop.ID)
							.Select(x=> new {Stop = x, Distance = calculatorDistance.CalculateDistance(meshNode.Stop, x.Stop) })
							.Where(d=> d.Distance <= Params.MaxHumanDistanceM )
							.OrderBy(x => x.Distance)
							.Take(Params.MaxHumanStops)
							.ToList())
				{
					meshNode.ConnectedStops.Add(new EdgeGraph(distanceNode.Stop, distanceNode.Distance, ConnectionType.Human));
				}


				StopConnections.Add(meshNode.Stop.ID, meshNode.ConnectedStops.Select(x => new ConnectionInfo(x.Stop.Stop, x.Distance, x.Connection)).ToList());
			}
		}

		

		public List<KeyValuePair<Rout, IEnumerable<Stop>>> resultRout;
		public Stack<KeyValuePair<Rout, IEnumerable<Stop>>> resultRouts;
		public List<KeyValuePair<List<Rout>, List<Stop>>> resultRouts2;
		public List<List<KeyValuePair<Rout, IEnumerable<Stop>>>> globalResult;

		List<KeyValuePair<List<Rout>, List<Stop>>> MergeIntoRoutChunks(List<KeyValuePair<Rout, IEnumerable<Stop>>> chunks)
		{
			var comparer = new RoutNameComparer();
			var result = chunks.GroupBy(pair => new {startId = pair.Value.First().ID, endId = pair.Value.Last().ID, count = pair.Value.Count()}).Select(pairs =>
				new KeyValuePair<List<Rout>, List<Stop>>(pairs.Select(v => v.Key).Distinct(comparer).ToList(), pairs.First().Value.ToList())).ToList();

			return result;
		}

		public bool FindPath(Stop start, Stop destin)
		{
			if (start == null || destin == null)
				return false;
			var startNode = meshGraphs.First(x => x.Stop.ID == start.ID);
			var endNode = meshGraphs.First(x => x.Stop.ID == destin.ID);
			ResultStopList = new Stack<NodeGraph>();
			//var result = Findpath(startNode, endNode);
			//var xxx = FindSeveralPathAStar(startNode, endNode);
			var result = FindPathAStar(startNode, endNode);
			if (result == null)
				return false;
			List<Stop> listStop = new List<Stop>(10);
			while (result != null)
			{
				listStop.Add(result.Stop);
				result = result.Parent;
			}
			listStop.Reverse();
			//var listStop = ResultStopList.Select(node=> node.Stop).ToList();
			//listStop.Insert(0,start);
			Stop startStop = start;
			resultRout = new List<KeyValuePair<Rout, IEnumerable<Stop>>>();
			foreach (var rout in context.Routs.Where(rout => rout.Stops.Any(x => listStop.Any(d => d.ID == x.ID))))
			{
				//if (rout.Stops.Any(stop => stop.ID == startStop.ID))
				//{
				//.SkipWhile(stop=> stop != startStop)
				var intersects = listStop.Intersect(rout.Stops, new StopComparer()).ToList();

					if (intersects.Count > 1 )
					{
						//startStop = intersects.Last();
						var stops = context.GetAllStop(rout, intersects.First(), intersects.Last());
						if (stops == null || stops.Count == 1)
							continue;
						resultRout.Add(new KeyValuePair<Rout, IEnumerable<Stop>>(rout, stops ));
					}
				//}
			}

			foreach (var stop in listStop)
			{
				ConnectStopsByWalk(stop, listStop.Where(x=> x.ID != stop.ID).ToList());
			}

			resultRouts2 =  MergeIntoRoutChunks(resultRout);

			//Second method
			resultRouts = new Stack<KeyValuePair<Rout, IEnumerable<Stop>>>();
			globalResult = new List<List<KeyValuePair<Rout, IEnumerable<Stop>>>>();
			//bool method2 = FindRout(listStop, 0);



			return true;
		}


		void CreatePaths(Stop start, Stop end, List<PathChunk> chunks)
		{
			List<PathChunk> path = new List<PathChunk>();

			foreach (var st in  chunks.Where(x => start.ID == x.Start.ID))
			{
				
			}
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
				notConnectedStop
					.Select(x=> new { st = x, Distance = calculatorDistance.CalculateDistance(stop, x) })
					.Where(d=> d.Distance <= Params.MaxHumanDistanceM)
					.OrderBy(x => x.Distance)
					.Take(Params.MaxHumanStops)
					.Select(x=> x.st)
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

		public List<NodeGraph> openList = new List<NodeGraph>();
		public List<NodeGraph> closedList = new List<NodeGraph>();

		private void FillNode(NodeGraph parent, NodeGraph current, NodeGraph end, ConnectionType type, List<Rout> parentRouts )
		{
			current.H = calculatorDistance.CalculateDistance(current.Stop, end.Stop)/(20*1000/60);
			var calculatedDistance = calculatorDistance.CalculateDistance(current.Stop, parent.Stop);
			if (type == ConnectionType.Human)
				current.G = parent.G + calculatedDistance/(5.0*1000/60);
			else if (type == ConnectionType.Transport)
				current.G = parent.G + context.GetAvgRoutTime(parent.Stop, current.Stop);
			current.Parent = parent;

			if (type == ConnectionType.Transport && !current.Stop.Routs.Intersect(parentRouts).Any())
				current.G += context.GetAvgRoutIntervalBetweenStops(parent.Stop, current.Stop);
			//	current.G += calculatedDistance * Params.ChangeTransport;
			//if (!current.Stop.Routs.Intersect(end.Stop.Routs).Any())
			//	current.H *= Params.ChangeTransport;
		}

		List<NodeGraph> FindSeveralPathAStar(NodeGraph start, NodeGraph end)
		{
			List<NodeGraph> graphs = new List<NodeGraph>(start.ConnectedStops.Count);
			for (int i = 0; i < start.ConnectedStops.Count; i++)
			{
				start.ConnectedStops[i].Stop.Parent = start;
				start.Parent = null;
				var res = FindPathAStar(start.ConnectedStops[i].Stop, end);
				graphs.Add(res);
			}
			return graphs;
		}

		NodeGraph FindPathAStar(NodeGraph start, NodeGraph end, int trycount = 1)
		{
			openList.Clear();
			closedList.Clear();
			closedList.Add(start);
			foreach (var node in start.ConnectedStops)
			{
				FillNode(start, node.Stop, end, node.Connection, node.Routs);
				openList.Add(node.Stop);
			}
			while (openList.Count > 0)
			{
				openList = openList.OrderBy(x => x.F).ToList();
				var p = openList.First();
				if (p.Stop.ID == end.Stop.ID)
					return p;
				openList.Remove(p);
				closedList.Add(p);
				foreach (var succsessor in p.ConnectedStops.Where(x=> !closedList.Any(d=> d.Stop.ID == x.Stop.Stop.ID)).ToList())
				{
					FillNode(p, succsessor.Stop, end, succsessor.Connection, succsessor.Routs);
					//if (succsessor.Connection == ConnectionType.Human)
					//	succsessor.Stop.H *= Params.HumanMultipl;
					
					var contains = openList.FirstOrDefault(graph => graph.Stop.ID == succsessor.Stop.Stop.ID);
					if (contains != null)
					{
						if (contains.G > succsessor.Stop.G)
						{
							FillNode(p, contains, end, succsessor.Connection, succsessor.Routs);
						}
					}
					else
						openList.Add(succsessor.Stop);
				}
			}
			return null;
		}
		

		private Stack<NodeGraph> ResultStopList;  

		bool Findpath(NodeGraph start, NodeGraph end)
		{
			start.Black = true;
			Stack<NodeGraph> stack = new Stack<NodeGraph>();
			foreach (var connectedStop in start.ConnectedStops.Where(x=> x.Stop.Black == false).OrderBy(x=> calculatorDistance.CalculateDistance(x.Stop.Stop.Lat, x.Stop.Stop.Lng, end.Stop.Lat, end.Stop.Lng)))
			{
				if (!connectedStop.Stop.Black)
					if (connectedStop.Stop != end)
					{
						connectedStop.Stop.Black = true;
						if (Findpath(connectedStop.Stop, end))
						{
							ResultStopList.Push(connectedStop.Stop);
							return true;
						}
					}
					else
					{
						ResultStopList.Push(connectedStop.Stop);
						return true;
					}
			}
			
			if (ResultStopList.Any())
				ResultStopList.Pop();
			return false;
		}
	}
}
