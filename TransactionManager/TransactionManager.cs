﻿using DMSCommon.Model;
using DMSContract;
using FTN.Common;
using FTN.ServiceContracts;
using IMSContract;
using OMSSCADACommon;
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
        // properties for providing communication infrastructure for 2PC protocol
        List<ITransaction> transactionProxys;
        List<TransactionCallback> transactionCallbacks;
        ITransaction proxyTransactionNMS;
        ITransaction proxyTransactionDMS;
        ITransactionSCADA proxyTransactionSCADA;
        TransactionCallback callBackTransactionNMS;
        TransactionCallback callBackTransactionDMS;
        TransactionCallback callBackTransactionSCADA;
        NetworkModelGDAProxy ProxyToNMSService;




        //ChannelFactory<IIMSContract> factoryToIMS;
        //IIMSContract IMSClient;
        IDMSContract proxyToDispatcherDMS;


        ModelGDATMS gdaTMS;
        SCADAClient scadaClient;

        public List<ITransaction> TransactionProxys { get => transactionProxys; set => transactionProxys = value; }
        public List<TransactionCallback> TransactionCallbacks { get => transactionCallbacks; set => transactionCallbacks = value; }
        public ITransaction ProxyTransactionNMS { get => proxyTransactionNMS; set => proxyTransactionNMS = value; }
        public ITransaction ProxyTransactionDMS { get => proxyTransactionDMS; set => proxyTransactionDMS = value; }
        public ITransactionSCADA ProxyTransactionSCADA { get => proxyTransactionSCADA; set => proxyTransactionSCADA = value; }
        public TransactionCallback CallBackTransactionNMS { get => callBackTransactionNMS; set => callBackTransactionNMS = value; }
        public TransactionCallback CallBackTransactionDMS { get => callBackTransactionDMS; set => callBackTransactionDMS = value; }
        public TransactionCallback CallBackTransactionSCADA { get => callBackTransactionSCADA; set => callBackTransactionSCADA = value; }

        private IMSClient imsClient;
        private IMSClient IMSClient
        {
            get
            {
                if (imsClient == null)
                {
                    imsClient = new IMSClient(new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
                }
                return imsClient;
            }
            set { imsClient = value; }
        }

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
            var binding = new NetTcpBinding();
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.TransactionFlow = true;
            // duplex channel for NMS transaction
            CallBackTransactionNMS = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionNMS);
            DuplexChannelFactory<ITransaction> factoryTransactionNMS = new DuplexChannelFactory<ITransaction>(CallBackTransactionNMS,
                                                         binding,
                                                         new EndpointAddress("net.tcp://localhost:8018/NetworkModelTransactionService"));
            ProxyTransactionNMS = factoryTransactionNMS.CreateChannel();
            TransactionProxys.Add(ProxyTransactionNMS);

            // duplex channel for DMS transaction
            CallBackTransactionDMS = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionDMS);
            DuplexChannelFactory<ITransaction> factoryTransactionDMS = new DuplexChannelFactory<ITransaction>(CallBackTransactionDMS,
                                                            binding,
                                                            new EndpointAddress("net.tcp://localhost:8028/DMSTransactionService"));
            ProxyTransactionDMS = factoryTransactionDMS.CreateChannel();
            TransactionProxys.Add(ProxyTransactionDMS);

            // duplex channel for SCADA transaction
            CallBackTransactionSCADA = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionSCADA);
            DuplexChannelFactory<ITransactionSCADA> factoryTransactionSCADA = new DuplexChannelFactory<ITransactionSCADA>(CallBackTransactionSCADA,
                                                            binding,
                                                            new EndpointAddress("net.tcp://localhost:8078/SCADATransactionService"));
            ProxyTransactionSCADA = factoryTransactionSCADA.CreateChannel();

            // client channel for SCADA 


            // client channel for DMSDispatcherService
            ChannelFactory<IDMSContract> factoryDispatcherDMS = new ChannelFactory<IDMSContract>(binding, new EndpointAddress("net.tcp://localhost:8029/DMSDispatcherService"));
            proxyToDispatcherDMS = factoryDispatcherDMS.CreateChannel();



            //ChannelFactory<IIMSContract> factoryToIMS = new ChannelFactory<IIMSContract>(binding, new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
           // proxyToIMS = factoryToIMS.CreateChannel();

            ProxyToNMSService = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
            ProxyToNMSService.Open();


            // client channel for IMS
           // factoryToIMS = new ChannelFactory<IIMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
            //IMSClient = factoryToIMS.CreateChannel();

        }

        #region 2PC methods

        public void Enlist(Delta d)
        {
            Console.WriteLine("Transaction Manager calling enlist");
            foreach (ITransaction svc in TransactionProxys)
            {
                svc.Enlist();
            }

            ProxyTransactionSCADA.Enlist();

            while (true)
            {
                if (TransactionCallbacks.Where(k => k.AnswerForEnlist == TransactionAnswer.Unanswered).Count() > 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    Prepare(d);
                    break;
                }
            }
        }

        public void Prepare(Delta delta)
        {
            Console.WriteLine("Transaction Manager calling prepare");


            ScadaDelta scadaDelta = GetDeltaForSCADA(delta);
            Delta fixedGuidDelta = ProxyToNMSService.GetFixedDelta(delta);

            //TransactionProxys.ToList().ForEach(x => x.Prepare(delta));
            ProxyTransactionNMS.Prepare(delta);
            ProxyTransactionDMS.Prepare(fixedGuidDelta);
            ProxyTransactionSCADA.Prepare(scadaDelta);

            while (true)
            {
                if (TransactionCallbacks.Where(k => k.AnswerForPrepare == TransactionAnswer.Unanswered).Count() > 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else if (TransactionCallbacks.Where(u => u.AnswerForPrepare == TransactionAnswer.Unprepared).Count() > 0)
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
            ProxyTransactionSCADA.Commit();
        }

        public void Rollback()
        {
            Console.WriteLine("Transaction Manager calling rollback");
            foreach (ITransaction svc in TransactionProxys)
            {
                svc.Rollback();
            }
            ProxyTransactionSCADA.Rollback();
        }

        #endregion

        #region IOMSClient CIMAdapter Methods

        // so, in order for network to be initialized, UpdateSystem must be called first

        /// <summary>
        /// Called by ModelLabs(CIMAdapter) when Static data changes
        /// </summary>
        /// <param name="d">Delta</param>
        /// <returns></returns>
        public bool UpdateSystem(Delta d)
        {
            Console.WriteLine("Update System started." + d.Id);
            Enlist(d);
            //  Prepare(d);
            return true;
        }

        #endregion

        #region  IOMSClient DispatcherApp Methods

        public TMSAnswerToClient GetNetwork()
        {
            // ako se ne podignu svi servisi na DMSu, ovde pada
            List<Element> listOfDMSElement = proxyToDispatcherDMS.GetAllElements();

            List<ResourceDescription> resourceDescriptionFromNMS = new List<ResourceDescription>();
            List<ResourceDescription> descMeas = new List<ResourceDescription>();

            gdaTMS.GetExtentValues(ModelCode.BREAKER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.CONNECTNODE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGCONSUMER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGSOURCE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ACLINESEGMENT).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.DISCRETE).ForEach(u => resourceDescriptionFromNMS.Add(u));

            int GraphDeep = proxyToDispatcherDMS.GetNetworkDepth();

            try
            {
                Command c = MappingEngineTransactionManager.Instance.MappCommand(TypeOfSCADACommand.ReadAll, "", 0, 0);
                Response r = SCADAClientInstance.ExecuteCommand(c);
                descMeas = MappingEngineTransactionManager.Instance.MappResult(r);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            bool isImsAvailable = false;
            do
            {
                try
                {
                    if (IMSClient.State == CommunicationState.Created)
                    {
                        IMSClient.Open();                      
                    }

                    isImsAvailable = IMSClient.Ping();
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e);
                    Console.WriteLine("GetNetwork() -> IMS is not available yet.");
                    if (IMSClient.State == CommunicationState.Faulted)
                        IMSClient = new IMSClient(new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
                }
                Thread.Sleep(2000);
            } while (!isImsAvailable);

            var crews = IMSClient.GetCrews();
            var incidentReports = IMSClient.GetAllReports();

            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, listOfDMSElement, GraphDeep, descMeas, crews, incidentReports);
            return answer;
        }

        public void SendCommandToSCADA(TypeOfSCADACommand command, string mrid, CommandTypes commandtype, float value)
        {
            try
            {
                Command c = MappingEngineTransactionManager.Instance.MappCommand(command, mrid, commandtype, value);
                Response r = SCADAClientInstance.ExecuteCommand(c);

            }
            catch (Exception e)
            { }
        }

        public void SendCrew(IncidentReport report)
        {
            proxyToDispatcherDMS.SendCrewToDms(report);
            return;
        }

        // currently unused
        public bool IsNetworkAvailable()
        {
            bool retVal = false;
            try
            {
                retVal = proxyToDispatcherDMS.IsNetworkAvailable();
            }
            catch (System.ServiceModel.EndpointNotFoundException e)
            {
                //Console.WriteLine("DMSDispatcher is not available yet.");
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {

            }

            return retVal;
        }

        private ScadaDelta GetDeltaForSCADA(Delta d)
        {
            List<ResourceDescription> rescDesc = d.InsertOperations.Where(u => u.ContainsProperty(ModelCode.MEASUREMENT_DIRECTION)).ToList();
            ScadaDelta scadaDelta = new ScadaDelta();

            foreach (ResourceDescription rd in rescDesc)
            {
                ScadaElement element = new ScadaElement();
                if (rd.ContainsProperty(ModelCode.MEASUREMENT_TYPE))
                {
                    string type = rd.GetProperty(ModelCode.MEASUREMENT_TYPE).ToString();
                    if (type == "Analog")
                    {
                        element.Type = DeviceTypes.ANALOG;
                    }
                    else if (type == "Discrete")
                    {
                        element.Type = DeviceTypes.DIGITAL;
                    }
                }

                element.ValidCommands = new List<CommandTypes>() { CommandTypes.CLOSE, CommandTypes.OPEN };
                element.ValidStates = new List<OMSSCADACommon.States>() { OMSSCADACommon.States.CLOSED, OMSSCADACommon.States.OPENED };

                if (rd.ContainsProperty(ModelCode.IDOBJ_MRID))
                {
                    //element.Name = rd.GetProperty(ModelCode.IDOBJ_NAME).ToString();
                    element.Name = rd.GetProperty(ModelCode.IDOBJ_MRID).ToString();
                }
                scadaDelta.InsertOps.Add(element);
            }
            return scadaDelta;
        }

        #endregion

        // da li se ove metode ikada pozivaju?  Onaj console1 ne koristimo?

        // SVUDA PRVO PROVERITI DA LI JE IMS DOSTUPAN? 
        // tj naprviti metodu koja to radi
        #region Unused? check this!!!

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
            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, null, GraphDeep, null, null, null);
            resourceDescriptions = resourceDescriptionFromNMS;
            DMSElements = listOfDMSElement;
            GraphDeep = proxyToDispatcherDMS.GetNetworkDepth();

            // return resourceDescriptionFromNMS;
        }

        //public void AddReport(string mrID, DateTime time, string state)
        //{
        //    IMSClient.AddReport(mrID, time, state);
        //}

        public List<List<ElementStateReport>> GetElementStateReportsForMrID(string mrID)
        {
            return IMSClient.GetElementStateReportsForMrID(mrID);
        }

        public List<ElementStateReport> GetElementStateReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime)
        {
            return IMSClient.GetElementStateReportsForSpecificTimeInterval(startTime, endTime);
        }

        public List<ElementStateReport> GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime)
        {
            return IMSClient.GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(mrID, startTime, endTime);
        }

        public void SendCrew(string mrid)
        {
            throw new NotImplementedException();
        }

        public List<Crew> GetCrews()
        {
            return IMSClient.GetCrews();
        }

        //public void SendCrew(string mrid)
        //{
        //    proxyToDispatcherDMS.SendCrewToDms(mrid);
        //    return;
        //}

        public bool AddCrew(Crew crew)
        {
            return IMSClient.AddCrew(crew);
        }

        public void AddReport(IncidentReport report)
        {
            IMSClient.AddReport(report);
        }

        public List<IncidentReport> GetAllReports()
        {
            return IMSClient.GetAllReports();
        }

        public List<List<IncidentReport>> GetReportsForMrID(string mrID)
        {
            return IMSClient.GetReportsForMrID(mrID);
        }

        public List<IncidentReport> GetReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime)
        {
            return IMSClient.GetReportsForSpecificTimeInterval(startTime, endTime);
        }

        public List<IncidentReport> GetReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime)
        {
            return IMSClient.GetReportsForSpecificMrIDAndSpecificTimeInterval(mrID, startTime, endTime);
        }

        public List<ElementStateReport> GetAllElementStateReports()
        {
            return IMSClient.GetAllElementStateReports();
        }

        public List<List<IncidentReport>> GetReportsForSpecificDateSortByBreaker(List<string> mrids, DateTime date)
        {
            return IMSClient.GetReportsForSpecificDateSortByBreaker(mrids, date);
        }
        #endregion
    }
}