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

				Console.WriteLine("Upisano:\n MRID: " + mrID + ", Date Time: " + time.ToString() + ", State: " + state);
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

		public List<IncidentReport> GetReportsForMrID(string mrID)
		{
			List<IncidentReport> retVal = new List<IncidentReport>();
			using (var ctx = new IncidentContext())
			{
				ctx.IncidentReports.Where(u => u.MrID == mrID).ToList().ForEach(x => retVal.Add(x));

			}
			return retVal;
		}

		public List<IncidentReport> GetReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime)
		{
			List<IncidentReport> retVal = new List<IncidentReport>();
			using (var ctx = new IncidentContext())
			{
				ctx.IncidentReports.Where(u =>u.MrID == mrID && u.Time > startTime && u.Time < endTime).ToList().ForEach(x => retVal.Add(x));
			}
			return retVal;
		}

		public List<IncidentReport> GetReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime)
		{
			List<IncidentReport> retVal = new List<IncidentReport>();
			using (var ctx = new IncidentContext())
			{
				ctx.IncidentReports.Where(u => u.Time > startTime && u.Time < endTime).ToList().ForEach(x => retVal.Add(x));
			}
			return retVal;
		}
	}
}
