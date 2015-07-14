using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient
{
	public class NodeGraph
	{
		private IList<NodeGraph> connectedStops;
		public Stop Stop { get; set; }
		public bool Black { get; set; }

		public IList<NodeGraph> ConnectedStops
		{
			get
			{
				if (connectedStops == null)
					connectedStops = new List<NodeGraph>();
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
}
