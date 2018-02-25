using DMSCommon.Model;
using IMSContract;
using System.Collections.Generic;
using System.ServiceModel;

namespace PubSubContract
{
    [ServiceContract]
    public interface IPublishing
    {
        [OperationContract(IsOneWay = true)]
        void PublishDigitalUpdate(List<SCADAUpdateModel> update);

        [OperationContract(IsOneWay = true)]
        void PublishAnalogUpdate(List<SCADAUpdateModel> update);

        [OperationContract(IsOneWay = true)]
        void PublishCrewUpdate(SCADAUpdateModel update);

        [OperationContract(IsOneWay = true)]
        void PublishIncident(IncidentReport report);

        [OperationContract(IsOneWay = true)]
        void PublishCallIncident(SCADAUpdateModel call);

        [OperationContract(IsOneWay = true)]
        void PublishUIBreakers(bool isIncident,long incidentBreaker);
    }
}
