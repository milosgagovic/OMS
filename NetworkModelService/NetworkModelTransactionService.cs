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
        private static NetworkModel newNetworkModel;

        private static NetworkModel NewNetworkModel { get => newNetworkModel; set => newNetworkModel = value; }

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
                // NewNetworkModel = gda.GetCopyOfNetworkModel();
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
                    //Commit();
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
            NewNetworkModel = null;
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackRollabck("Something went wrong on NMS");
        }
    }
}
