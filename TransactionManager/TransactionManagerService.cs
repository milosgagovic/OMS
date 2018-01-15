﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionManagerContract;

namespace TransactionManager
{
   public class TransactionManagerService
    {
        private ServiceHost svc = null;
        public void Start()
        {
            svc = new ServiceHost(typeof(TransactionManager));
            var binding = new NetTcpBinding();
            binding.TransactionFlow = true;
            svc.AddServiceEndpoint(typeof(IOMSClient), binding, new
            Uri("net.tcp://localhost:6080/TransactionManagerService"));
            svc.Open();
            Console.WriteLine("TransactionManagerService ready and waiting for requests.");
        }
        public void Stop()
        {
            svc.Close();
            Console.WriteLine("TransactionCoordinator server stopped.");
        }
    }
}
