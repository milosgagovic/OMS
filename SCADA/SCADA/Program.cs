using OMSSCADACommon;
using SCADA.ClientHandler;
using SCADA.CommAcqEngine;
using SCADA.RealtimeDatabase;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
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
            Thread requestConsumer = new Thread(PCCommEng.StartProcessing);


            ACQEngine AcqEngine = new ACQEngine();
            AcqEngine.Configure("AcqConfiguration.xml");
            Thread reqProducer = new Thread(AcqEngine.StartAcquisition);
            Thread answConsumer = new Thread(AcqEngine.ProcessPCAnwers);

            requestConsumer.Start();
            reqProducer.Start();
            answConsumer.Start();

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
