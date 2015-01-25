using System;

namespace MinskTrans.DesctopClient.Modelview
{
	public class ShowArgs: EventArgs
	{
		public Stop SelectedStop { get; set; }
		public Rout SelectedRoute { get; set; }
	}
}