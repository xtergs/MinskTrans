using System.Collections.Generic;
using MinskTrans.Context.Base.BaseModel;

namespace MinskTrans.AutoRouting.AutoRouting
{
	public enum ConnectionType
	{
		Human, Transport
	}
	public struct EdgeGraph
	{
		public EdgeGraph(NodeGraph stop, double distance, ConnectionType connec)
		{
			Stop = stop;
			Distance = distance;
			Connection = connec;
			Time = 0;
			Routs = new List<Rout>();
		}
		public NodeGraph Stop { get; set; }
		public double Distance { get; set; }
		public double Time { get; set; }
		public ConnectionType Connection { get; set; }
		public List<Rout> Routs { get; set; } 

	}
	public class NodeGraph
	{
		private IList<EdgeGraph> connectedStops;
		public Stop Stop { get; set; }
		public bool Black { get; set; }

		public double G = 0;
		public double H = 0;
		public NodeGraph Parent = null;

		public double F => G + H;

		public IList<EdgeGraph> ConnectedStops
		{
			get
			{
				if (connectedStops == null)
					connectedStops = new List<EdgeGraph>();
				return connectedStops;
			}
			set { connectedStops = value; }
		}

		#region Overrides of Object

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return Stop.ID + " " +
			       Stop.Name;
		}

		#endregion
	}

	public class ConnectionInfo
	{
		public ConnectionInfo(Stop st, double dist, ConnectionType type)
		{
			Stop = st;
			Distance = dist;
			Type = type;
		}
		public Stop Stop { get; set; }
		public double Distance { get; set; }
		public ConnectionType Type { get; set; }
	}

	class PathChunk
	{
		public List<Rout> Routs { get; set; }
		public List<Stop> Stops { get; set; }
		public Stop Start { get; set; }
		public Stop End { get; set; }
	}
}
