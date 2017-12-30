using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionManagerContract;

namespace TransactionManager
{
    public class TransactionManager : IOMSClient
    {
        List<ITransaction> proxys;
        List<TransactionCallback> callbacks;

        public List<ITransaction> Proxys { get => proxys; set => proxys = value; }
        public List<TransactionCallback> Callbacks { get => callbacks; set => callbacks = value; }

        void IOMSClient.UpdateSystem(Delta d)
        {
            Console.WriteLine("Update System started." + d.Id);
        }

        private void InitializeChanels()
        {
            TransactionCallback callBackNMS = new TransactionCallback();
            Callbacks.Add(callBackNMS);
            DuplexChannelFactory<ITransaction> factory = new DuplexChannelFactory<ITransaction>(callBackNMS, new NetTcpBinding(),
            new EndpointAddress("net.tcp://localhost:8018/NetworkModelTransactionService"));
            ITransaction proxy = factory.CreateChannel();
            Proxys.Add(proxy);

           // TransactionCallback callBackDMS = new TransactionCallback();
           // Callbacks.Add(callBackDMS);

           // DuplexChannelFactory<ITransaction> factoryDMS = new DuplexChannelFactory<ITransaction>(callBackDMS, new NetTcpBinding(),
           //new EndpointAddress("net.tcp://localhost:9000/DMSTransactionService"));
           // ITransaction proxyDMS = factoryDMS.CreateChannel();
           // Proxys.Add(proxyDMS);
        }

        public TransactionManager()
        {
            Proxys = new List<ITransaction>();
            Callbacks = new List<TransactionCallback>();
            InitializeChanels();
        }

        public void Enlist()
        {
            Console.WriteLine("Transaction Manager calling enlist");
            foreach (ITransaction svc in Proxys)
            {
                svc.Enlist();
            }
        }
        public void Prepare()
        {
            Console.WriteLine("Transaction Manager calling prepare");
            foreach (ITransaction svc in Proxys)
            {
                svc.Prepare();
            }

            while (true)
            {
                if (Callbacks.Where(k => k.Answer == TransactionAnswer.Unanswered).Count() > 0)
                {
                    Thread.Sleep(100);
                    continue;
                }
                else if (Callbacks.Where(u => u.Answer == TransactionAnswer.Unprepared).Count() > 0)
                {
                    Rollback();
                    break;
                }

                Commit();
                break;
            }

        }

        private void Commit()
        {
            Console.WriteLine("Transaction Manager calling commit");
            foreach (ITransaction svc in Proxys)
            {
                svc.Commit();
            }
        }

        public void Rollback()
        {
            Console.WriteLine("Transaction Manager calling rollback");
            foreach (ITransaction svc in Proxys)
            {
                svc.Rollback();
            }
        }
    }
}
