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
        void UpdateSystem();
    }
}
