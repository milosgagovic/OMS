using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using TransactionManagerContract;

namespace FTN.Services.NetworkModelService
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class NetworkModelTransactionService : ITransaction
    {
        private static GenericDataAccess gda = new GenericDataAccess();

        public void Commit()
        {
            Console.WriteLine("Pozvan je Commit na NMS-u");
            if (GenericDataAccess.NewNetworkModel != null)
            {
                GenericDataAccess.NetworkModel = GenericDataAccess.NewNetworkModel;
                ResourceIterator.NetworkModel = GenericDataAccess.NewNetworkModel;
            }


            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackCommit("Uspjesno je prosao commit na NMS-u");
        }

        public void Enlist()
        {
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            Console.WriteLine("Pozvan je enlist na NMS-u");
            try
            {

                gda.GetCopyOfNetworkModel();
                callback.CallbackEnlist(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                callback.CallbackEnlist(false);
            }
        }

        public void Prepare(Delta delta)
        {
            Console.WriteLine("Pozvan je prepare na NMS-u");
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();

            try
            {
                UpdateResult updateResult = gda.ApplyUpdate(delta);
                if (updateResult.Result == ResultType.Succeeded)
                {
                    callback.CallbackPrepare(true);
                }
                else
                {
                    Rollback();
                    callback.CallbackPrepare(false);
                }
            }
            catch (Exception ex)
            {
                Rollback();
                callback.CallbackPrepare(false);
                Console.WriteLine(ex.Message);
            }
        }

        public void Rollback()
        {
            Console.WriteLine("Pozvan je RollBack na NMSu");
            GenericDataAccess.NewNetworkModel = null;
            GenericDataAccess.NetworkModel = GenericDataAccess.OldNetworkModel;
            ResourceIterator.NetworkModel = GenericDataAccess.OldNetworkModel;
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackRollback("Something went wrong on NMS");
        }
    }
}
