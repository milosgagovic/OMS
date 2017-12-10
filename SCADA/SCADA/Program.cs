using SCADA.ClientHandler;
using SCADA.RealtimeDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
    class Program
    {
        static void Main(string[] args)
        {
            DBContext context = new DBContext();

            // load and parse configuration
            // create process variables
            // fill database

            // start polling thread

            // start client thread

            try
            {
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
