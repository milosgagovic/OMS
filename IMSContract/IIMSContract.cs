
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
		[OperationContract]
		void AddReport(string mrID, DateTime time, string state);

		[OperationContract]
		List<IncidentReport> GetAllReports();

	}

}
