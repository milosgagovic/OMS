using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using OMSSCADACommon;
using TransactionManagerContract;
using SCADA.RealtimeDatabase;
using SCADA.ConfigurationParser;

namespace SCADA.ClientHandler
{
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class SCADATransactionService : ITransactionSCADA
    {
        private static string modifiedConfigFile = "NewScadaModel.xml";
        private static string currentConfigFile = "ScadaModel.xml";

        private DBContext dbContext = null;

        public SCADATransactionService()
        {
            dbContext = new DBContext();
        }

        // check if there is ANY free space in controller
        // at this point we do not know if delta will contain 1 or 10 measurements
        // so we only check if is it possible to add minimal memory occupying element
        public void Enlist()
        {
            Console.WriteLine("Pozvan je Enlist na SCADA");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();

            // at this point, we will only check if there is a free space for DIGITALS
 
            bool isSuccessfull = false;

            var availableRtus = dbContext.GettAllRTUs().Values.Where(r => r.FreeSpaceForDigitals == true).ToList();
            if (availableRtus.Count != 0)
                isSuccessfull = false;

            try
            {
                callback.CallbackEnlist(isSuccessfull);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                callback.CallbackEnlist(false);
            }

        }

        // trying to apply, apply if is possible, and write configuration to new file
        public void Prepare(ScadaDelta delta)
        {
            Console.WriteLine("Pozvan je Prepare na SCADA");

            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();

            if (dbContext.ApplyDelta(delta))
            {
                try
                {
                    ScadaModelParser parser = new ScadaModelParser();

                    // novu konfiguraciju cuvamo u fajlu
                    parser.SerializeScadaModel(modifiedConfigFile);

                    callback.CallbackPrepare(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    callback.CallbackPrepare(false);
                }
            }
            else
            {
                callback.CallbackPrepare(false);
            }
        }

        // setting configuration to new file
        public void Commit()
        {
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();

            ScadaModelParser parser = new ScadaModelParser();
            parser.SwapConfigs(currentConfigFile, modifiedConfigFile);

            callback.CallbackCommit("Commited on SCADA");
            Console.WriteLine("Pozvan je Commit na SCADA");
        }

        // returning to old config file, parse database again from file! 
        public void Rollback()
        {
            ITransactionCallback callback = OperationContext.Current.GetCallbackChannel<ITransactionCallback>();

            ScadaModelParser parser = new ScadaModelParser();
            parser.SwapConfigs(modifiedConfigFile, currentConfigFile);

            // proveriti ovo! iz kog fajla se deserijalizuje, tj. da li je swap odradio. 
            // prthodno izbrisati bazu...
            parser.DeserializeScadaModel();

            callback.CallbackRollback("Somethinw went wrong on SCADA");
            Console.WriteLine("Pozvan je Rollback na SCADA");
        }
    }
}
