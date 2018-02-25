using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace IMSContract
{

	[ServiceContract]
	public interface IIMSContract
	{
        //[OperationContract]
        //void AddReport(string mrID, DateTime time, string state);

        //// unused
        //[OperationContract]
        //List<IncidentReport> GetReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime);

        //// unused
        //[OperationContract]
        //List<IncidentReport> GetReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime);

        //// unused
        //[OperationContract]
        //List<ElementStateReport> GetElementStateReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime);

        //// unused
        //[OperationContract]
        //List<ElementStateReport> GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime);

        //// unused
        //[OperationContract]
        //bool AddCrew(Crew crew);

        [OperationContract]
        bool Ping(); 

        [OperationContract]
        void AddReport(IncidentReport report);

        [OperationContract]
		List<IncidentReport> GetAllReports();

        [OperationContract]
        IncidentReport GetReport(DateTime id);

        [OperationContract]
        void UpdateReport(IncidentReport report);

        [OperationContract]
		List<List<IncidentReport>> GetReportsForMrID(string mrID);
      
        [OperationContract]
        List<List<IncidentReport>> GetReportsForSpecificDateSortByBreaker(List<string> mrids, DateTime date);
      
        [OperationContract]
        void AddElementStateReport(ElementStateReport report);

        [OperationContract]
        List<ElementStateReport> GetAllElementStateReports();

        [OperationContract]
        List<List<ElementStateReport>> GetElementStateReportsForMrID(string mrID);
      
        [OperationContract]
        List<List<IncidentReport>> GetAllReportsSortByBreaker(List<string> mrids);

        [OperationContract]
        List<Crew> GetCrews();
	}
}
