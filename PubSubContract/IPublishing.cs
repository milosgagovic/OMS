using FTN.Common;
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
        void Publish(Delta delta);
    }
}
