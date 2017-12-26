using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManagerContract
{
    [ServiceContract]
   public interface ITransaction
    {
        [OperationContract]
        void Enlist();

        [OperationContract]
        bool Prepare();

        [OperationContract]
        void Commit();

        [OperationContract]
        void Rollback();

    }
}
