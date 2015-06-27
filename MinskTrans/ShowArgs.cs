﻿using System;
using MinskTrans.DesctopClient.Model;

namespace MinskTrans.DesctopClient
{
	public class ShowArgs: EventArgs
	{
		public Stop SelectedStop { get; set; }
		public Rout SelectedRoute { get; set; }
	}
}