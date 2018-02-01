using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using OMSSCADACommon;
using TransactionManagerContract;

namespace SCADA.ClientHandler
{
    public class SCADATransactionService : ITransactionSCADA
    {
        public void Commit()
        {
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackCommit("Commited on SCADA");
        }

        public void Enlist()
        {
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackEnlist(true);
        }

        public void Prepare(ScadaDelta delta)
        {
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackPrepare(true);
        }

        public void Rollback()
        {
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackRollback("Rollback on scada");
        }
    }
}
