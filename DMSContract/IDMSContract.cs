using DMSCommon.Model;
using IMSContract;
using System.Collections.Generic;
using System.ServiceModel;

namespace DMSContract
{
    [ServiceContract]
    public interface IDMSContract
    {
        [OperationContract]
        bool IsNetworkAvailable();

        [OperationContract]
        List<Source> GetAllSources();

        [OperationContract]
        List<Consumer> GetAllConsumers();

        [OperationContract]
        List<Switch> GetAllSwitches();

        [OperationContract]
        List<ACLine> GetAllACLines();

        [OperationContract]
        List<Node> GetAllNodes();

        [OperationContract]
        Dictionary<long, Element> InitNetwork();

        [OperationContract]
        Source GetTreeRoot();

        [OperationContract]
        int GetNetworkDepth();

        [OperationContract]
        List<Element> GetAllElements();

        [OperationContract]
        void SendCrewToDms(IncidentReport report);
    }
}
