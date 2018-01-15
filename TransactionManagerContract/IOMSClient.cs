using DMSCommon.Model;
using FTN.Common;
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
        void UpdateSystem(Delta d);

        [OperationContract]
        void GetNetworkWithOutParam(out List<Element> DMSElements, out List<ResourceDescription> resourceDescriptions, out int GraphDeep);

        [OperationContract]
        TMSAnswerToClient GetNetwork();
    }
}
