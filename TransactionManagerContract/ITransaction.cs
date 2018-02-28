using FTN.Common;
using System.ServiceModel;

namespace TransactionManagerContract
{
    [ServiceContract(CallbackContract = typeof(ITransactionCallback))]
    public interface ITransaction
    {
        [OperationContract]
        void Enlist();

        [OperationContract]
        void Prepare(Delta delta);

        [OperationContract]
        void Commit();

        [OperationContract]
        void Rollback();
    }
}
