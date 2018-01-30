using DMSCommon.Model;
using FTN.Common;
using IMSContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManagerContract
{
    [ServiceContract]
   public interface IOMSClient
    {
        [OperationContract]
        bool UpdateSystem(Delta d);

        [OperationContract]
        void GetNetworkWithOutParam(out List<Element> DMSElements, out List<ResourceDescription> resourceDescriptions, out int GraphDeep);

        [OperationContract]
        TMSAnswerToClient GetNetwork();

		//[OperationContract]
		//void AddReport(string mrID, DateTime time, string state);

        [OperationContract]
        void AddReport(IncidentReport report);

        [OperationContract]
		List<IncidentReport> GetAllReports();

		[OperationContract]
		List<IncidentReport> GetReportsForMrID(string mrID);

		[OperationContract]
		List<IncidentReport> GetReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime);

		[OperationContract]
		List<IncidentReport> GetReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime);

        [OperationContract]
        List<ElementStateReport> GetAllElementStateReports();

        [OperationContract]
        List<ElementStateReport> GetElementStateReportsForMrID(string mrID);

        [OperationContract]
        List<ElementStateReport> GetElementStateReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime);

        [OperationContract]
        List<ElementStateReport> GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime);

        [OperationContract]
		void SendCommandToSCADA(TypeOfSCADACommand command);

        //[OperationContract]
        //void SendCrew(string mrid);

        [OperationContract]
        void SendCrew(DateTime id);

		[OperationContract]
        List<Crew> GetCrews();

		[OperationContract]
		bool AddCrew(Crew crew);
	}
}
