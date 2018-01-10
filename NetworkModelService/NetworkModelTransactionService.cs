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
        public void Commit()
        {
            Console.WriteLine("Pozvan je Commit na NMS-u");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackCommit("Uspjesno je prosao commit na NMS-u");
        }

        public void Enlist()
        {
            Console.WriteLine("Pozvan je enlist na NMS-u");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackEnlist();
        }

        public void Prepare()
        {
            Console.WriteLine("Pozvan je prepare na NMS-u");
            //TO DO Kopije i provjera da li moze da se primjeni delta
            //NEtwork model = new Network model
            //model.AppltyDelta();...
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackPrepare(true);
        }

        public void Rollback()
        {
            Console.WriteLine("Pozvan je RollBack na NMSu");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackRollabck("Something went wrong on NMS");
        }
    }
}
