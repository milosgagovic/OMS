using CommunicationEngineContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationEngine
{
    public class CommunicEngineService : IDisposable
    {
        private ServiceHost host;
        private CommunicationEngine ce = null;
        public CommunicEngineService()
        {
            ce = new CommunicationEngine();
            InitializeHosts();
        }

        private void InitializeHosts()
        {
            host = new ServiceHost(typeof(CommunicationEngine));
        }

        public void Start()
        {
            if (host == null)
            {
                throw new Exception("Communication Engine Services can not be opend because it is not initialized.");
            }
            string message = string.Empty;
            host.Open();

            message = string.Format("The WCF service {0} is ready.", host.Description.Name);
            Console.WriteLine(message);

            message = "Endpoints:";
            Console.WriteLine(message);

            foreach (Uri uri in host.BaseAddresses)
            {
                Console.WriteLine(uri);
            }
            Console.WriteLine("\n");


            message = string.Format("Connection string: {0}", Config.Instance.ConnectionString);
            Console.WriteLine(message);

            Console.WriteLine(message);


            message = "The Network Model Service is started.";
            Console.WriteLine("\n{0}", message);
        }

        public void Dispose()
        {
            host.Close();
            GC.SuppressFinalize(this);
        }
    }
}
