﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinskTrans.DesctopClient.Model
{
	public class RoutEx:Rout
	{
		private Context context;

		private List<Stop> stops;
		private Schedule time; 

		public RoutEx(Context context, string routS, Rout rout)
			:base(routS, rout)
		{
			this.context = context;
		}

		public RoutEx(Context context, string routS)
			:base(routS)
		{
			this.context = context;
		}

		#region Overrides of Rout

		public override List<Stop> Stops
		{
			get
			{
				if (stops == null && context.Stops != null)
				{
					stops = context.Stops.Where(x=>RouteStops.Contains(x.ID)).ToList();
				}
				return stops;
			}
			set { stops = value; }
		}

		public override Schedule Time
		{
			get
			{
				if (time == null)
					time = context.Times.FirstOrDefault(x => x.RoutId == RoutId);
				return time;
			}
			set { time = value; }
		}

		
		#endregion
	}
}