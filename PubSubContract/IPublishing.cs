using DMSCommon.Model;
using FTN.Common;
using IMSContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PubSubContract
{
    [ServiceContract]
    public interface IPublishing
    {

        [OperationContract(IsOneWay = true)]
        void Publish(List<SCADAUpdateModel> update);

        [OperationContract(IsOneWay = true)]
        void PublishCrewUpdate(SCADAUpdateModel update);

        [OperationContract(IsOneWay = true)]
        void PublishIncident(IncidentReport report);

        [OperationContract(IsOneWay = true)]
        void PublishCallIncident(SCADAUpdateModel call);
    }
}
