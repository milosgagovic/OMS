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


            /*
             There are two layers in SCADA implementation, and a message queue for communication/integration between layers. 

               - Message Queue component is implemented in <IORequestsQueue.cs> file 

               - Layer for handling communication with SCADA Clients (OMS) - defined in <ACQEngine.cs> file. It produces
                    Requests for data acquisition and repack commands from external client in appropriate Request. No matter of 
                    Request source (automatic acquistion or external client), each is enqueued to the same queue. Second part of
                    logic waits for Answers to Requests (dequeing from answers queue) and process them.

               - Layer for handling communication with Process Controllers - defined in <PCCommunicationEngine.cs> file 
                    Main logic consists of consuming (dequeuing) Requests (for commanding or acquistion) and 
                    dispatching it to target Process Controller. After recieving Reply/Answer from Process Controller, 
                    it enques it to queue for Answers.
         
            
            There are 4 threads.

            -> PCCommunicationEngine.cs
            1. Thread for dequeing from IORequests, sending Request to Simulator;
               getting reply from Simulator, and enqueing it to IOAnswers.
               This is SEND/RCV thread  
            
            -> ACQEngine.cs
            2. Thread for producing acquisition requests (enqueuing to IORequests)
            3. Thread for producing commanding requests (enqueing to IORequests)

            4. Thread for dequeing from IOAnswers
            
             */

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

            // uzimanje zahteva iz reda, i slanje zahteva MDBU-u.
            // dobijanje MDB odgovora i stavljanje u red
            Thread processingRequestsFromQueue = new Thread(PCCommEng.ProcessRequestsFromQueue);

            // stavljanje zahteva za akviziju u red
            Thread producingAcquisitonRequests = new Thread(AcqEngine.StartAcquisition);
            // stavljanje zahteva za komandovanje u red
            Thread producingCommandingRequests = new Thread(AcqEngine.TestWriteSingleDigital);


            // uzimanje odgovora iz reda
            Thread processingAnswersFromQueue = new Thread(AcqEngine.ProcessPCAnwers);


            processingRequestsFromQueue.Start();

            producingAcquisitonRequests.Start();
            //producingCommandingRequests.Start();

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
