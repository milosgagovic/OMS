using FTN.Common;
using PubSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionManagerContract;
using DMSCommon.Model;
namespace DMSService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class DMSTransactionService : ITransaction
    {
        public void Commit()
        {
            Console.WriteLine("Pozvan je Commit na DMS-u");
            if (DMSService.updatesCount > 2)
            {
                Publisher publisher = new Publisher();
                List<SCADAUpdateModel> update = new List<SCADAUpdateModel>();
                Source s = (Source)DMSService.tree.Data[DMSService.tree.Roots[0]];
                update.Add(new SCADAUpdateModel(true, s.ElementGID));

                publisher.PublishUpdate(update);
            }


            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackCommit("Uspjesno je prosao commit na DMS-u");
        }

        public void Enlist()
        {
            Console.WriteLine("Pozvan je enlist na DMS-u");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackEnlist(true);
        }

        public void Prepare(Delta delta)
        {
            Console.WriteLine("Pozvan je prepare na DMS-u");
            //TO DO Kopije i provjera da li moze da se primjeni delta
            //NEtwork model = new Network model
            //model.AppltyDelta();...
            DMSService.Instance.InitializeNetwork();
            DMSService.updatesCount += 1;
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            if (DMSService.tree.Data.Values.Count != 0)
            {
                callback.CallbackPrepare(true);
                return;
            }

            callback.CallbackPrepare(false);
        }

        public void Rollback()
        {
            Console.WriteLine("Pozvan je RollBack na DMSu");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();
            callback.CallbackRollabck("Something went wrong on DMS");
        }
    }
}
