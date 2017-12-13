using SCADA.ClientHandler;
using SCADA.CommAcqEngine;
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
            
            // komentar za aleksandru posto ce ona ovo gledati, a nedovrseno je:      
            // ako budes htela da radis scada service ostavi ovo zakomentarisi dok ja odradim sve sto trebam...
            
            /*Ovo moraju biti taskovi-threadovi, da bi se izvrsavali konkuretno i asinhrono
             ovako samo PCCommEng vrti...to cu veceras odraditi*/
            PCCommunicationEngine PCCommEng = new PCCommunicationEngine();
            PCCommEng.StartProcessing();


            ACQEngine AcqEngine = new ACQEngine();
            AcqEngine.StartAcquisition();

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
