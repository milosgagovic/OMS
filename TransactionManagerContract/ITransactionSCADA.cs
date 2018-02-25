using OMSSCADACommon;
using System.ServiceModel;

namespace TransactionManagerContract
{
    [ServiceContract(CallbackContract = typeof(ITransactionCallback))]
    public interface ITransactionSCADA
    {
        [OperationContract]
        void Enlist();

        [OperationContract]
        void Prepare(ScadaDelta delta);

        [OperationContract]
        void Commit();

        [OperationContract]
        void Rollback();
    }
}
