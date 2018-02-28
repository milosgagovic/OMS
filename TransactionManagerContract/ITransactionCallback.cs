using System.ServiceModel;

namespace TransactionManagerContract
{
    public interface ITransactionCallback
    {
        [OperationContract(IsOneWay = true)]
        void CallbackEnlist(bool prepare);

        [OperationContract(IsOneWay = true)]
        void CallbackPrepare(bool prepare);

        [OperationContract(IsOneWay = true)]
        void CallbackCommit(string commit);

        [OperationContract(IsOneWay = true)]
        void CallbackRollback(string rollback);
    }
}
