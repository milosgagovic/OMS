using CommunicationEngineContract;
using SCADA.ClientHandler;
using SCADA.CommAcqEngine;
using SCADA.RealtimeDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
    public class Program
    {
        private static InstanceContext instanceContext;
        public static string commEngineAdress = "net.tcp://localhost:4100/CommunEngine";
        public static InstanceContext InstanceContext
        {
            get
            {
                return instanceContext;
            }

            set
            {
                instanceContext = value;
            }
        }
        static void Main(string[] args)
        {
            InstanceContext = new InstanceContext(new SCADACommuncEngineService());
            DBContext context = new DBContext();
            // load and parse configuration
            // create process variables
            // fill database

            // start polling thread

           
           
            //PCCommunicationEngine PCCommEng = new PCCommunicationEngine();
            //PCCommEng.StartProcessing();


            //ACQEngine AcqEngine = new ACQEngine();
            //AcqEngine.StartAcquisition();

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
