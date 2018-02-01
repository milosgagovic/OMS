using SCADA.CommAcqEngine;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionManagerContract;

namespace SCADA.ClientHandler
{
    public class SCADAService
    {
        private List<ServiceHost> hosts = null;

        public SCADAService()
        {
            InitializeHosts();           
        }

        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>();

            // service for handlling client requests
            ServiceHost invokerhost = new ServiceHost(typeof(Invoker));
            invokerhost.Description.Name = "SCADAInvokerservice";
            invokerhost.AddServiceEndpoint(typeof(ISCADAContract),
               new NetTcpBinding(),
               new Uri("net.tcp://localhost:4000/SCADAService"));
            hosts.Add(invokerhost);

            // to do: add transaction host
            // 2PC transaction service
            ServiceHost transactionServiceHost = new ServiceHost(typeof(SCADATransactionService));
            transactionServiceHost.Description.Name = "SCADATransactionService";
            transactionServiceHost.AddServiceEndpoint(typeof(ITransactionSCADA),
                new NetTcpBinding(),
                new Uri("net.tcp://localhost:8058/SCADATransactionService"));

            hosts.Add(transactionServiceHost);
        }

        public void Start()
        {

            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("SCADA service can not start because it is not initialized.");
            }

            string message = string.Empty;

            foreach(ServiceHost host in hosts)
            {
                host.Open();

                message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                Console.WriteLine(message);
                //CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

                message = "Endpoints:";
                Console.WriteLine(message);
                //CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine(uri);
                    //CommonTrace.WriteTrace(CommonTrace.TraceInfo, uri.ToString());
                }

                Console.WriteLine("\n");
            }

        }

        public void Dispose()
        {
            foreach(var host in hosts)
            {
                host.Close();
            }

            hosts.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
