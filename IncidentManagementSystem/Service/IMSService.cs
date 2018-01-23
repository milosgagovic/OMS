using IMSContract;
using IncidentManagementSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentManagementSystem.Service
{
	public class IMSService : IIMSContract
	{
		public void AddReport(string mrID, DateTime time, string state)
		{
            using (var ctx = new IncidentContext())
            {
                IncidentReport report = new IncidentReport() { Time = time, MrID = mrID, State = state };

                ctx.IncidentReports.Add(report);
                ctx.SaveChanges();
            }
        }

		public List<IncidentReport> GetAllReports()
		{
			List<IncidentReport> retVal = new List<IncidentReport>();
            using (var ctx = new IncidentContext())
            {
                foreach (IncidentReport ir in ctx.IncidentReports)
                {
                    retVal.Add(ir);
                }
            }
            return retVal;
		}
	}
}
