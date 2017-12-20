using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using CommunicationEngine;
using OMSSCADACommon;
using OMSSCADACommon.Commands;
using SCADAContracts;

namespace CommunicationEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string message = "";
            try
            {
                CommunicEngineService ces = new CommunicEngineService();
                ces.Start();
                message = "Press <Enter> to stop the service.";
                Console.WriteLine(message);
                //Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("CommunicationEngine service failed.");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }

            SCADAProxy proxy = new SCADAProxy(new NetTcpBinding(), "net.tcp://localhost:4000/SCADAService");
            Command command = new ReadAll();

            proxy.ExecuteCommand(command);

            Console.ReadKey();
        }
    }
}
