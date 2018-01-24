using SCADA.CommAcqEngine;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ClientHandler
{
    public class SCADAService
    {
        private ServiceHost host;

        public SCADAService()
        {
            host = new ServiceHost(typeof(Invoker));
        }

        public void Start()
        {
            if (host == null)
            {
                throw new Exception("SCADA service can not start because it is not initialized.");
            }

            string message = string.Empty;

            host.AddServiceEndpoint(typeof(ISCADAContract),
               new NetTcpBinding(),
               new Uri("net.tcp://localhost:4000/SCADAService"));
            host.Open();

            message = "SCADA service is up and running.";

            Console.WriteLine("\n{0}", message);
        }

        public void Dispose()
        {
            host.Close();
            GC.SuppressFinalize(this);
        }
    }
}
