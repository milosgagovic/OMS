using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionManagerContract;

namespace DMSService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class DMSTransactionService : ITransaction
    {
        public void Commit()
        {
            Console.WriteLine("Pozvan je Commit na DMS-u");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackCommit("Uspjesno je prosao commit na DMS-u");
        }

        public void Enlist()
        {
            Console.WriteLine("Pozvan je enlist na DMS-u");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackEnlist();
        }

        public void Prepare(Delta delta)
        {
            Console.WriteLine("Pozvan je prepare na DMS-u");
            //TO DO Kopije i provjera da li moze da se primjeni delta
            //NEtwork model = new Network model
            //model.AppltyDelta();...
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackPrepare(true);
        }

        public void Rollback()
        {
            Console.WriteLine("Pozvan je RollBack na DMSu");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackRollabck("Something went wrong on DMS");
        }
    }
}
