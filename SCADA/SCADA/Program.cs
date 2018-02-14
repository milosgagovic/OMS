using SCADA.ClientHandler;
using SCADA.RealtimeDatabase;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SCADA.CommunicationAndControlling;
using SCADA.CommunicationAndControlling.SecondaryDataProcessing;

namespace SCADA
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "SCADA";

            DBContext dbContext = new DBContext();

            string acqComConfigPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "ScadaModel.xml");
            string pcConfig = "RtuConfiguration.xml";
            string fullPcConfig = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "RtuConfiguration.xml");
            string basePath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

            // ovde neku shutdown promenljivu dodati...
            // verovatno ces morati sa cancellation token

            PCCommunicationEngine PCCommEng;
            while (true)
            {
                PCCommEng = new PCCommunicationEngine();

                if (!PCCommEng.Configure(basePath, pcConfig))
                {
                    Console.WriteLine("\nStart the simulator then press any key to continue the application.\n");
                    Console.ReadKey();
                    continue;
                }
                break;
            }


            CommAcqEngine AcqEngine = new CommAcqEngine();
            if (AcqEngine.Configure(acqComConfigPath))
            {
                // to do:
                // redosled

                AcqEngine.InitializeSimulator();

                // uzimanje zahteva iz reda, i slanje zahteva MDBU-u. dobijanje MDB odgovora i stavljanje u red
                Thread processingRequestsFromQueue = new Thread(PCCommEng.ProcessRequestsFromQueue);

                // uzimanje odgovora iz reda
                Thread processingAnswersFromQueue = new Thread(AcqEngine.ProcessPCAnwers);


                //zatim pokreni metodu na acq engineu koja ce inicijalizovati MdbSIm, to ne mora biti nit...

                // stavljanje zahteva za akviziju u red
                Thread producingAcquisitonRequests = new Thread(AcqEngine.StartAcquisition);


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
            }
            else
            {
                Console.WriteLine("Configuration of scada failed.");
            }

            Console.WriteLine("Press <Enter> to stop the service.");

            Console.ReadKey();

            AcqEngine.Stop();
            PCCommEng.Stop();
        }
    }
}
