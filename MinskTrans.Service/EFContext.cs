using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinskTrans.DesctopClient;

namespace MinskTrans.Service
{
	class EFContext:DbContext
	{
		public EFContext(string connectionString)
			:base(connectionString)
		{
			
		}
		public virtual DbSet<Stop> Stops { get; set; }
		public virtual DbSet<Rout> Routs { get; set; }
		public virtual DbSet<Time> Times { get; set; }
		public virtual DbSet<Schedule> Schedules { get; set; }

	}
}
