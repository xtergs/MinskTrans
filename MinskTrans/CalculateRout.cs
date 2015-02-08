using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
					int index = rout.Stops.IndexOf(meshGraph.Stop);
					if (index+1 >= rout.Stops.Count)
						continue;
					if (meshGraph.ConnectedStops.Any(x=>x.Stop == rout.Stops[index + 1]))
						continue;
					meshGraph.ConnectedStops.Add(meshGraphs.First(x=>x.Stop.ID == rout.Stops[index + 1].ID));
				}
				
			}
		}

		public void FindPath(Stop start, Stop destin)
		{
			var startNode = meshGraphs.First(x => x.Stop.ID == start.ID);
			var endNode = meshGraphs.First(x => x.Stop.ID == destin.ID);

			var result = Findpath(startNode, endNode);
		}

		bool Findpath(NodeGraph start, NodeGraph end)
		{
			start.Black = true;
			Stack<NodeGraph> stack = new Stack<NodeGraph>();
			foreach (var connectedStop in start.ConnectedStops)
			{
				if (!connectedStop.Black)
					if (connectedStop != end)
						stack.Push(connectedStop);
					else
					{
						return true;
					}
			}
			foreach (var nodeGraph in stack)
			{
				if (Findpath(nodeGraph, end))
					return true;
			}
			return false;
		}
	}
}
