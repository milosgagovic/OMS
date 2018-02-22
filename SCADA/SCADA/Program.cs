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

            // ako je druga platforma npr. x86 nije dobra putanja!

            string acqComConfigPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "ScadaModel.xml");
            string pcConfig = "RtuConfiguration.xml";
            string fullPcConfig = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "RtuConfiguration.xml");
            string basePath = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

            // to do: use cancellation tokens and TPL

            Task requestsConsumer, answersConsumer, acqRequestsProducer;

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
                AcqEngine.InitializeSimulator();
              
                // Thread processingRequestsFromQueue = new Thread(PCCommEng.ProcessRequestsFromQueue);
                // to do: for IO bound operatin you <await an operation which returns a task inside of an async method>
                // await yields control to the caller of the method thet performed await
                requestsConsumer = Task.Factory.StartNew(() => PCCommEng.ProcessRequestsFromQueue(),
                   TaskCreationOptions.LongRunning);

                // Thread processingAnswersFromQueue = new Thread(AcqEngine.ProcessPCAnwers);
                answersConsumer = Task.Factory.StartNew(() => AcqEngine.ProcessPCAnwers(),
                   TaskCreationOptions.LongRunning);

                //Thread producingAcquisitonRequests = new Thread(AcqEngine.StartAcquisition);

                // processingRequestsFromQueue.Start();
                // processingAnswersFromQueue.Start();

                // give simulator some time, and when everything is ready start acquisition
                Thread.Sleep(3000);

                // producingAcquisitonRequests.Start();
                //AcqEngine.StartAcquisition();
                acqRequestsProducer = Task.Factory.StartNew(() => AcqEngine.Acquisition());


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

            // wait tasks
            AcqEngine.Stop();
            PCCommEng.Stop();
        }
    }
}
