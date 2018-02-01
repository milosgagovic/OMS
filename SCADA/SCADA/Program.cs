using OMSSCADACommon;
using SCADA.ClientHandler;
using SCADA.CommAcqEngine;
using SCADA.RealtimeDatabase;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA
{
    public class Program
    {
        static void Main(string[] args)
        {
            DBContext dbContext = new DBContext();

            string acqConfig = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "ScadaModel.xml");
            string pcConfig = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "PCConfiguration.xml");

            // ovde neku shutdown promenljivu dodati...

            PCCommunicationEngine PCCommEng;
            while (true)
            {
                PCCommEng = new PCCommunicationEngine();

                if (!PCCommEng.Configure("PCConfiguration.xml"))
                {
                    Console.WriteLine("\nStart the simulator then press any key to continue the application.\n");
                    Console.ReadKey();
                    continue;
                }
                break;
            }


            ACQEngine AcqEngine = new ACQEngine();
            AcqEngine.Configure(acqConfig);

            // uzimanje zahteva iz reda, i slanje zahteva MDBU-u. dobijanje MDB odgovora i stavljanje u red
            Thread processingRequestsFromQueue = new Thread(PCCommEng.ProcessRequestsFromQueue);

            // stavljanje zahteva za akviziju u red
            Thread producingAcquisitonRequests = new Thread(AcqEngine.StartAcquisition);

            // uzimanje odgovora iz reda
            Thread processingAnswersFromQueue = new Thread(AcqEngine.ProcessPCAnwers);


            processingRequestsFromQueue.Start();
            producingAcquisitonRequests.Start();
            processingAnswersFromQueue.Start();


            try
            {
                Console.WriteLine("\n....");
                SCADAService ss = new SCADAService();
                ss.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("SCADA service failed.");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Press <Enter> to stop the service.");
            Console.ReadKey();
        }
    }

}
