
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IMSContract
{

	[ServiceContract]
	public interface IIMSContract
	{
        //[OperationContract]
        //void AddReport(string mrID, DateTime time, string state);

        [OperationContract]
        void AddReport(IncidentReport report);

        [OperationContract]
		List<IncidentReport> GetAllReports();

        [OperationContract]
        IncidentReport GetReport(DateTime id);

        [OperationContract]
        void UpdateReport(IncidentReport report);

        [OperationContract]
		List<IncidentReport> GetReportsForMrID(string mrID);

		[OperationContract]
		List<IncidentReport> GetReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime);

		[OperationContract]
		List<IncidentReport> GetReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime);

	}

}
