using IMSContract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentManagementSystem.Model
{
   public class IncidentContext :DbContext
    {
        public IncidentContext()
        {

        }
        public DbSet<IncidentReport> IncidentReports { get; set; }
        public DbSet<ElementStateReport> ElementStateReports { get; set; }
		public DbSet<Crew> Crews { get; set; }
	}
}
