﻿using IMSContract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentManagementSystem.Model
{
    public class IncidentContext : DbContext
    {
        public IncidentContext()
        {
            var adapter = (IObjectContextAdapter)this;
            var objectContext = adapter.ObjectContext;
            objectContext.CommandTimeout = 300;
        }

        public DbSet<IncidentReport> IncidentReports { get; set; }
        public DbSet<ElementStateReport> ElementStateReports { get; set; }
        public DbSet<Crew> Crews { get; set; }
    }

    public class IncidentCloudContext : DbContext
    {
        public IncidentCloudContext() : base("OMS")
        {

        }

        public DbSet<IncidentReport> IncidentReports { get; set; }
        public DbSet<ElementStateReport> ElementStateReports { get; set; }
        public DbSet<Crew> Crews { get; set; }
    }
}
