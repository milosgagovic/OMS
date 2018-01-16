using DMSCommon.Model;
using DMSContract;
using FTN.Common;
using FTN.Services.NetworkModelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelventDMS.Services.NetworkModelService.TestClient.Tests;
using TransactionManagerContract;

namespace TransactionManager
{
    public class TransactionManager : IOMSClient
    {
        List<ITransaction> proxys;
        List<TransactionCallback> callbacks;
        IDMSContract proxyToDMS;
        ModelGDATMS gdaTMS;
        public List<ITransaction> Proxys { get => proxys; set => proxys = value; }
        public List<TransactionCallback> Callbacks { get => callbacks; set => callbacks = value; }

        void IOMSClient.UpdateSystem(Delta d)
        {
            Console.WriteLine("Update System started." + d.Id);
            Enlist();
            Prepare(d);
        }

        private void InitializeChanels()
        {
            TransactionCallback callBackNMS = new TransactionCallback();
            Callbacks.Add(callBackNMS);
            DuplexChannelFactory<ITransaction> factory = new DuplexChannelFactory<ITransaction>(callBackNMS, new NetTcpBinding(),
            new EndpointAddress("net.tcp://localhost:8018/NetworkModelTransactionService"));
            ITransaction proxy = factory.CreateChannel();
            Proxys.Add(proxy);

            TransactionCallback callBackDMS = new TransactionCallback();
            Callbacks.Add(callBackDMS);

            DuplexChannelFactory<ITransaction> factoryDMS = new DuplexChannelFactory<ITransaction>(callBackDMS, new NetTcpBinding(),
           new EndpointAddress("net.tcp://localhost:8028/DMSTransactionService"));
            ITransaction proxyDMS = factoryDMS.CreateChannel();
            Proxys.Add(proxyDMS);

            TransactionCallback callBackCommunicationEngine = new TransactionCallback();
            Callbacks.Add(callBackCommunicationEngine);

            DuplexChannelFactory<ITransaction> factoryCommEngine = new DuplexChannelFactory<ITransaction>(callBackCommunicationEngine, new NetTcpBinding(),
           new EndpointAddress("net.tcp://localhost:8038/CommunicationEngineTransactionService"));
            ITransaction proxyCommEngine = factoryCommEngine.CreateChannel();
            Proxys.Add(proxyCommEngine);

            ChannelFactory<IDMSContract> factoryToDMS = new ChannelFactory<IDMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:8029/DMSDispatcherService"));
            proxyToDMS = factoryToDMS.CreateChannel();

        }

        public TransactionManager()
        {
            Proxys = new List<ITransaction>();
            Callbacks = new List<TransactionCallback>();
            InitializeChanels();
            gdaTMS = new ModelGDATMS();
        }

        public void Enlist()
        {
            Console.WriteLine("Transaction Manager calling enlist");
            foreach (ITransaction svc in Proxys)
            {
                svc.Enlist();
            }
        }
        public void Prepare(Delta delta)
        {
            Console.WriteLine("Transaction Manager calling prepare");
            foreach (ITransaction svc in Proxys)
            {
                svc.Prepare(delta);
            }

            while (true)
            {
                if (Callbacks.Where(k => k.Answer == TransactionAnswer.Unanswered).Count() > 0)
                {
                    Thread.Sleep(1000);
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

        public void GetNetworkWithOutParam(out List<Element> DMSElements, out List<ResourceDescription> resourceDescriptions, out int GraphDeep)
        {
            List<Element> listOfDMSElement = new List<Element>();//proxyToDMS.GetAllElements();
            List<ResourceDescription> resourceDescriptionFromNMS = new List<ResourceDescription>();
            List<ACLine> acList = proxyToDMS.GetAllACLines();
            List<Node> nodeList = proxyToDMS.GetAllNodes();
            List<Source> sourceList = proxyToDMS.GetAllSource();
            List<Switch> switchList = proxyToDMS.GetAllSwitches();
            List<Consumer> consumerList = proxyToDMS.GetAllConsumers();

            acList.ForEach(u => listOfDMSElement.Add(u));
            nodeList.ForEach(u => listOfDMSElement.Add(u));
            sourceList.ForEach(u => listOfDMSElement.Add(u));
            switchList.ForEach(u => listOfDMSElement.Add(u));
            consumerList.ForEach(u => listOfDMSElement.Add(u));

            gdaTMS.GetExtentValues(ModelCode.BREAKER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.CONNECTNODE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGCONSUMER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGSOURCE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ACLINESEGMENT).ForEach(u => resourceDescriptionFromNMS.Add(u));
            GraphDeep = proxyToDMS.GetNetworkDepth();
            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, null, GraphDeep);
            resourceDescriptions = resourceDescriptionFromNMS;
            DMSElements = listOfDMSElement;
            GraphDeep = proxyToDMS.GetNetworkDepth();
            // return resourceDescriptionFromNMS;
        }

        public TMSAnswerToClient GetNetwork()
        {
            List<Element> listOfDMSElement = proxyToDMS.GetAllElements();
            List<ResourceDescription> resourceDescriptionFromNMS = new List<ResourceDescription>();
            //List<ACLine> acList = proxyToDMS.GetAllACLines();
            //List<Node> nodeList = proxyToDMS.GetAllNodes();
            //List<Source> sourceList = proxyToDMS.GetAllSource();
            //List<Switch> switchList = proxyToDMS.GetAllSwitches();
            //List<Consumer> consumerList = proxyToDMS.GetAllConsumers();
            //acList.ForEach(u => listOfDMSElement.Add(u));
            //nodeList.ForEach(u => listOfDMSElement.Add(u));
            //sourceList.ForEach(u => listOfDMSElement.Add(u));
            //switchList.ForEach(u => listOfDMSElement.Add(u));
            //consumerList.ForEach(u => listOfDMSElement.Add(u));

            gdaTMS.GetExtentValues(ModelCode.BREAKER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.CONNECTNODE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGCONSUMER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGSOURCE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ACLINESEGMENT).ForEach(u => resourceDescriptionFromNMS.Add(u));
            int GraphDeep = proxyToDMS.GetNetworkDepth();
            return new TMSAnswerToClient(resourceDescriptionFromNMS, listOfDMSElement, GraphDeep);
        }
    }
}
