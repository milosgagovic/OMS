using DMSCommon.Model;
using DMSContract;
using FTN.Common;
using IMSContract;
using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using TransactionManagerContract;

namespace TransactionManager
{
    public class TransactionManager : IOMSClient
    {
        List<ITransaction> transactionProxys;
        List<TransactionCallback> transactionCallbacks;

        IDMSContract proxyToDispatcherDMS;
        ModelGDATMS gdaTMS;

        SCADAClient scadaClient;

        public List<ITransaction> TransactionProxys { get => transactionProxys; set => transactionProxys = value; }
        public List<TransactionCallback> TransactionCallbacks { get => transactionCallbacks; set => transactionCallbacks = value; }

        ChannelFactory<IIMSContract> factoryToIMS;
        IIMSContract proxyToIMS;

        private SCADAClient SCADAClientInstance
        {
            get
            {
                if (scadaClient == null)
                {
                    scadaClient = new SCADAClient();
                }
                return scadaClient;
            }
        }

        public TransactionManager()
        {
            TransactionProxys = new List<ITransaction>();
            TransactionCallbacks = new List<TransactionCallback>();
            InitializeChanels();
            gdaTMS = new ModelGDATMS();
            scadaClient = new SCADAClient();
        }

        private void InitializeChanels()
        {
            // duplex channel for NMS transaction
            TransactionCallback callBackTransactionNMS = new TransactionCallback();
            TransactionCallbacks.Add(callBackTransactionNMS);
            DuplexChannelFactory<ITransaction> factoryTransactionNMS = new DuplexChannelFactory<ITransaction>(callBackTransactionNMS,
                                                         new NetTcpBinding(),
                                                         new EndpointAddress("net.tcp://localhost:8018/NetworkModelTransactionService"));
            ITransaction proxyTransactionNMS = factoryTransactionNMS.CreateChannel();
            TransactionProxys.Add(proxyTransactionNMS);

            // duplex channel for DMS transaction
            TransactionCallback callBackTransactionDMS = new TransactionCallback();
            TransactionCallbacks.Add(callBackTransactionDMS);
            DuplexChannelFactory<ITransaction> factoryTransactionDMS = new DuplexChannelFactory<ITransaction>(callBackTransactionDMS,
                                                            new NetTcpBinding(),
                                                            new EndpointAddress("net.tcp://localhost:8028/DMSTransactionService"));
            ITransaction proxyTransactionDMS = factoryTransactionDMS.CreateChannel();
            TransactionProxys.Add(proxyTransactionDMS);


            //  zar nije communcation engine izbrisan?
            // TransactionCallback callBackCommunicationEngine = new TransactionCallback();
            // Callbacks.Add(callBackCommunicationEngine);

            // DuplexChannelFactory<ITransaction> factoryCommEngine = new DuplexChannelFactory<ITransaction>(callBackCommunicationEngine, new NetTcpBinding(),
            //new EndpointAddress("net.tcp://localhost:8038/CommunicationEngineTransactionService"));
            // ITransaction proxyCommEngine = factoryCommEngine.CreateChannel();
            // Proxys.Add(proxyCommEngine);


            // client channel for DMSDispatcherService
            ChannelFactory<IDMSContract> factoryDispatcherDMS = new ChannelFactory<IDMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:8029/DMSDispatcherService"));
            proxyToDispatcherDMS = factoryDispatcherDMS.CreateChannel();


            factoryToIMS = new ChannelFactory<IIMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
            proxyToIMS = factoryToIMS.CreateChannel();

            //  ProxyToCommunicationEngine = new CommEngProxyUpdate("CommEngineEndpoint");
        }

        public void Enlist()
        {
            Console.WriteLine("Transaction Manager calling enlist");
            foreach (ITransaction svc in TransactionProxys)
            {
                svc.Enlist();
            }
        }

        public void Prepare(Delta delta)
        {
            Console.WriteLine("Transaction Manager calling prepare");
            foreach (ITransaction svc in TransactionProxys)
            {
                svc.Prepare(delta);
            }

            while (true)
            {
                if (TransactionCallbacks.Where(k => k.Answer == TransactionAnswer.Unanswered).Count() > 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else if (TransactionCallbacks.Where(u => u.Answer == TransactionAnswer.Unprepared).Count() > 0)
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
            foreach (ITransaction svc in TransactionProxys)
            {
                svc.Commit();
            }
        }

        public void Rollback()
        {
            Console.WriteLine("Transaction Manager calling rollback");
            foreach (ITransaction svc in TransactionProxys)
            {
                svc.Rollback();
            }
        }

        #region IOMSClient Methods

        public bool UpdateSystem(Delta d)
        {
            Console.WriteLine("Update System started." + d.Id);
            Enlist();
            Prepare(d);
            return true;
        }

        public void GetNetworkWithOutParam(out List<Element> DMSElements, out List<ResourceDescription> resourceDescriptions, out int GraphDeep)
        {
            List<Element> listOfDMSElement = new List<Element>();//proxyToDMS.GetAllElements();
            List<ResourceDescription> resourceDescriptionFromNMS = new List<ResourceDescription>();
            List<ACLine> acList = proxyToDispatcherDMS.GetAllACLines();
            List<Node> nodeList = proxyToDispatcherDMS.GetAllNodes();
            List<Source> sourceList = proxyToDispatcherDMS.GetAllSource();
            List<Switch> switchList = proxyToDispatcherDMS.GetAllSwitches();
            List<Consumer> consumerList = proxyToDispatcherDMS.GetAllConsumers();

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
            GraphDeep = proxyToDispatcherDMS.GetNetworkDepth();
            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, null, GraphDeep, null);
            resourceDescriptions = resourceDescriptionFromNMS;
            DMSElements = listOfDMSElement;
            GraphDeep = proxyToDispatcherDMS.GetNetworkDepth();

            // return resourceDescriptionFromNMS;
        }

        public TMSAnswerToClient GetNetwork()
        {
            List<Element> listOfDMSElement = proxyToDispatcherDMS.GetAllElements();
            List<ResourceDescription> resourceDescriptionFromNMS = new List<ResourceDescription>();
            List<ResourceDescription> descMeas = new List<ResourceDescription>();
            // ProxyToCommunicationEngine.ReceiveAllMeasValue(TypeOfSCADACommand.ReadAll);

            gdaTMS.GetExtentValues(ModelCode.BREAKER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.CONNECTNODE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGCONSUMER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGSOURCE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ACLINESEGMENT).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.DISCRETE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            int GraphDeep = proxyToDispatcherDMS.GetNetworkDepth();

            try
            {
                Command c = MappingEngineTransactionManager.Instance.MappCommand(TypeOfSCADACommand.ReadAll);
                Response r = SCADAClientInstance.ExecuteCommand(c);
                descMeas = MappingEngineTransactionManager.Instance.MappResult(r);

            }
            catch (Exception e)
            {

            }

            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, listOfDMSElement, GraphDeep, descMeas);
            return answer;
        }

        //public void AddReport(string mrID, DateTime time, string state)
        //{
        //    proxyToIMS.AddReport(mrID, time, state);
        //}

        public void AddReport(IncidentReport report)
        {
            proxyToIMS.AddReport(report);
        }

        public List<IncidentReport> GetAllReports()
        {
            return proxyToIMS.GetAllReports();
        }

        public List<IncidentReport> GetReportsForMrID(string mrID)
        {
            return proxyToIMS.GetReportsForMrID(mrID);
        }

        public List<IncidentReport> GetReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime)
        {
            return proxyToIMS.GetReportsForSpecificTimeInterval(startTime, endTime);
        }

        public List<IncidentReport> GetReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime)
        {
            return proxyToIMS.GetReportsForSpecificMrIDAndSpecificTimeInterval(mrID, startTime, endTime);
        }

        public void SendCommandToSCADA(TypeOfSCADACommand command)
        {
            try
            {
                Command c = MappingEngineTransactionManager.Instance.MappCommand(command);
                Response r = SCADAClientInstance.ExecuteCommand(c);

            }
            catch (Exception e)
            {

            }
        }

        //public void SendCrew(string mrid)
        //{
        //    proxyToDispatcherDMS.SendCrewToDms(mrid);
        //    return;
        //}

        public void SendCrew(DateTime id)
        {
            proxyToDispatcherDMS.SendCrewToDms(id);
            return;
        }

        public void SendCrew(string mrid)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
