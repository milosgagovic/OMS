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
        public void AddElementStateReport(ElementStateReport report)
        {
            using (var ctx = new IncidentContext())
            {
                ctx.ElementStateReports.Add(report);
                ctx.SaveChanges();
                Console.WriteLine("Upisano:\n MRID: " + report.MrID + ", Date Time: " + report.Time + ", State: " + report.State);
            }
        }

        public void AddReport(IncidentReport report)
        {
            using (var ctx = new IncidentContext())
            {
                ctx.IncidentReports.Add(report);
                ctx.SaveChanges();
            }
        }

        public List<ElementStateReport> GetAllElementStateReports()
        {
            List<ElementStateReport> retVal = new List<ElementStateReport>();
            using (var ctx = new IncidentContext())
            {
                foreach (ElementStateReport ir in ctx.ElementStateReports)
                {
                    retVal.Add(ir);
                }
            }
            return retVal;
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

        public List<ElementStateReport> GetElementStateReportsForMrID(string mrID)
        {
            List<ElementStateReport> retVal = new List<ElementStateReport>();
            using (var ctx = new IncidentContext())
            {
                ctx.ElementStateReports.Where(u => u.MrID == mrID).ToList().ForEach(x => retVal.Add(x));

            }
            return retVal;
        }

        public List<ElementStateReport> GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime)
        {
            List<ElementStateReport> retVal = new List<ElementStateReport>();
            using (var ctx = new IncidentContext())
            {
                ctx.ElementStateReports.Where(u => u.MrID == mrID && u.Time > startTime && u.Time < endTime).ToList().ForEach(x => retVal.Add(x));
            }
            return retVal;
        }

        public List<ElementStateReport> GetElementStateReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime)
        {
            List<ElementStateReport> retVal = new List<ElementStateReport>();
            using (var ctx = new IncidentContext())
            {
                ctx.ElementStateReports.Where(u => u.Time > startTime && u.Time < endTime).ToList().ForEach(x => retVal.Add(x));
            }
            return retVal;
        }

        public IncidentReport GetReport(DateTime id)
        {
            List<IncidentReport> retVal = new List<IncidentReport>();
            using (var ctx = new IncidentContext())
            {
                foreach (IncidentReport ir in ctx.IncidentReports)
                {
                    retVal.Add(ir);
                }
            }

            IncidentReport res = null;
            foreach (IncidentReport report in retVal)
            {
                if (DateTime.Compare(report.Time, id) == 0)
                {
                    res = report;
                    break;
                }
            }
            return res;
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

        public void UpdateReport(IncidentReport report)
        {
            List<IncidentReport> list = new List<IncidentReport>();
            using (var ctx = new IncidentContext())
            {
                foreach (IncidentReport ir in ctx.IncidentReports)
                {
                    list.Add(ir);
                }
                
                int i = 0;
                for(i = 0; i < list.Count; i++)
                {
                    if (DateTime.Compare(list[i].Time, report.Time) == 0)
                    {
                        i = list[i].Id;
                        break;
                    }
                }

                var res = ctx.IncidentReports.Where(r => r.Id == i).FirstOrDefault();
                res.Reason = report.Reason;
                res.RepairTime = report.RepairTime;
                res.CrewSent = report.CrewSent;
                res.IncidentState = report.IncidentState;

                ctx.SaveChanges();
            }
        }
    }
}
