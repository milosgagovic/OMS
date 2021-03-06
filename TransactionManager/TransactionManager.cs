﻿using DMSCommon.Model;
using DMSContract;
using FTN.Common;
using FTN.ServiceContracts;
using IMSContract;
using OMSSCADACommon;
using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
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
        IDMSContract proxyToDispatcherDMS;

        ModelGDATMS gdaTMS;

        private IMSClient imsClient;
        private IMSClient IMSClient
        {
            get
            {
                if (imsClient == null)
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.CloseTimeout = TimeSpan.FromMinutes(10);
                    binding.OpenTimeout = TimeSpan.FromMinutes(10);
                    binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                    binding.SendTimeout = TimeSpan.FromMinutes(10);
                    binding.MaxReceivedMessageSize = Int32.MaxValue;
                    imsClient = new IMSClient(new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"), binding);
                }
                return imsClient;
            }
            set { imsClient = value; }
        }

        private SCADAClient scadaClient;
        private SCADAClient ScadaClient
        {
            get
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.CloseTimeout = TimeSpan.FromMinutes(10);
                binding.OpenTimeout = TimeSpan.FromMinutes(10);
                binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                binding.SendTimeout = TimeSpan.FromMinutes(10);
                binding.MaxReceivedMessageSize = Int32.MaxValue;

                if (scadaClient == null)
                {
                    scadaClient = new SCADAClient(new EndpointAddress("net.tcp://localhost:4000/SCADAService"), binding);
                }
                return scadaClient;
            }
            set { scadaClient = value; }
        }

        public List<ITransaction> TransactionProxys { get => transactionProxys; set => transactionProxys = value; }
        public List<TransactionCallback> TransactionCallbacks { get => transactionCallbacks; set => transactionCallbacks = value; }
        public ITransaction ProxyTransactionNMS { get => proxyTransactionNMS; set => proxyTransactionNMS = value; }
        public ITransaction ProxyTransactionDMS { get => proxyTransactionDMS; set => proxyTransactionDMS = value; }
        public ITransactionSCADA ProxyTransactionSCADA { get => proxyTransactionSCADA; set => proxyTransactionSCADA = value; }
        public TransactionCallback CallBackTransactionNMS { get => callBackTransactionNMS; set => callBackTransactionNMS = value; }
        public TransactionCallback CallBackTransactionDMS { get => callBackTransactionDMS; set => callBackTransactionDMS = value; }
        public TransactionCallback CallBackTransactionSCADA { get => callBackTransactionSCADA; set => callBackTransactionSCADA = value; }

        public TransactionManager()
        {
            TransactionProxys = new List<ITransaction>();
            TransactionCallbacks = new List<TransactionCallback>();

            InitializeChanels();

            gdaTMS = new ModelGDATMS();
        }

        private void InitializeChanels()
        {
            Console.WriteLine("InitializeChannels()");

            var binding = new NetTcpBinding();
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.TransactionFlow = true;
            binding.MaxReceivedMessageSize = Int32.MaxValue;

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

            // client channel for DMSDispatcherService
            ChannelFactory<IDMSContract> factoryDispatcherDMS = new ChannelFactory<IDMSContract>(binding, new EndpointAddress("net.tcp://localhost:8029/DMSDispatcherService"));
            proxyToDispatcherDMS = factoryDispatcherDMS.CreateChannel();

            //ProxyToNMSService = new NetworkModelGDAProxy("NetworkModelGDAEndpoint");
            //ProxyToNMSService.Open();
        }

        #region 2PC methods

        public void Enlist(Delta d)
        {
            Task[] tasks = new Task[TransactionCallbacks.Count];
            Console.WriteLine("Transaction Manager calling enlist");
            for(int i =0; i< TransactionProxys.Count; i++)
            {
                int x = i;
                tasks[x] = new Task(() =>TransactionProxys[x].Enlist());
                tasks[x].Start();
            }

            tasks[TransactionCallbacks.Count-1] = new Task(() => ProxyTransactionSCADA.Enlist());
            tasks[TransactionCallbacks.Count-1].Start();


            Task.WaitAll(tasks);
            while (true)
            {
                if (TransactionCallbacks.Where(k => k.AnswerForEnlist == TransactionAnswer.Unanswered).Count() > 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else if (TransactionCallbacks.Where(u => u.AnswerForEnlist == TransactionAnswer.Unprepared).Count() > 0)
                {
                    Rollback();
                    break;
                }
                else
                {
                    Prepare(d);
                    break;
                }
            }
        }

        public void Prepare(Delta deltaForNMS)
        {
            Console.WriteLine("Transaction Manager calling prepare");
            ScadaDelta deltaForScada = GetDeltaForSCADA(deltaForNMS);
            Delta fixedGuidDeltaForDMS = gdaTMS.GetFixedDelta(deltaForNMS);
            Task[] tasks = new Task[TransactionCallbacks.Count-1]; //-1 zato sto se ceka da nms bude spreman

            ProxyTransactionNMS.Prepare(deltaForNMS);
            //prvo pozovi nms
            do
            {
                Thread.Sleep(50);
            } while (CallBackTransactionNMS.AnswerForPrepare == TransactionAnswer.Unanswered); //cekaj dok ne odgovori

            if (CallBackTransactionNMS.AnswerForPrepare == TransactionAnswer.Prepared) //ukoliko je prosao na nms-u
            {
                tasks[0] = new Task(() => ProxyTransactionDMS.Prepare(fixedGuidDeltaForDMS));
                tasks[0].Start();

                tasks[1] = new Task(() => ProxyTransactionSCADA.Prepare(deltaForScada));
                tasks[1].Start();
                Task.WaitAll(tasks);

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
        }

        private void Commit()
        {
            Task[] tasks = new Task[TransactionCallbacks.Count];
            Console.WriteLine("Transaction Manager calling commit");
            for (int i = 0; i < TransactionProxys.Count; i++)
            {
                int x = i;
                tasks[x] = new Task(() => TransactionProxys[x].Commit());
                tasks[x].Start();
            }

            tasks[TransactionCallbacks.Count - 1] = new Task(() => ProxyTransactionSCADA.Commit());
            tasks[TransactionCallbacks.Count - 1].Start();


            Task.WaitAll(tasks);

           
            //foreach (ITransaction svc in TransactionProxys)
            //{
            //    svc.Commit();
            //}
            //ProxyTransactionSCADA.Commit();
        }

        public void Rollback()
        {
            Task[] tasks = new Task[TransactionCallbacks.Count];
            Console.WriteLine("Transaction Manager calling rollback");
            for (int i = 0; i < TransactionProxys.Count; i++)
            {
                int x = i;
                tasks[x] = new Task(() => TransactionProxys[x].Rollback());
                tasks[x].Start();
            }

            tasks[TransactionCallbacks.Count - 1] = new Task(() => ProxyTransactionSCADA.Rollback());
            tasks[TransactionCallbacks.Count - 1].Start();


            Task.WaitAll(tasks);


           
            //foreach (ITransaction svc in TransactionProxys)
            //{
            //    svc.Rollback();
            //}
            //ProxyTransactionSCADA.Rollback();
        }

        #endregion

        #region IOMSClient CIMAdapter Methods

        /// <summary>
        /// Called by ModelLabs(CIMAdapter) when Static data changes
        /// </summary>
        /// <param name="d">Delta</param>
        /// <returns></returns>
        public bool UpdateSystem(Delta d)
        {
            Console.WriteLine("Update System started." + d.Id);
            Enlist(d);
            return true;
        }

        public void ClearNMSDB()
        {
            using (NMSAdoNet ctx = new NMSAdoNet())
            {
                var tableNames = ctx.Database.SqlQuery<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT LIKE '%Migration%'").ToList();
                foreach (var tableName in tableNames)
                {
                    ctx.Database.ExecuteSqlCommand(string.Format("DELETE FROM {0}", tableName));
                }

                ctx.SaveChanges();
            }
        }

        #endregion

        #region  IOMSClient DispatcherApp Methods

        public TMSAnswerToClient GetNetwork()
        {
            // ako se ne podignu svi servisi na DMSu, ovde pada
            List<Element> listOfDMSElement = new List<Element>();
            try
            {
                listOfDMSElement = proxyToDispatcherDMS.GetAllElements();
            }
            catch (Exception e) { }

            List<ResourceDescription> resourceDescriptionFromNMS = new List<ResourceDescription>();
            List<ResourceDescription> descMeas = new List<ResourceDescription>();

            gdaTMS.GetExtentValues(ModelCode.BREAKER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.CONNECTNODE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGCONSUMER).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ENERGSOURCE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ACLINESEGMENT).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.DISCRETE).ForEach(u => resourceDescriptionFromNMS.Add(u));
            gdaTMS.GetExtentValues(ModelCode.ANALOG).ForEach(u => resourceDescriptionFromNMS.Add(u));

            int GraphDeep = proxyToDispatcherDMS.GetNetworkDepth();

            try
            {
                Command c = MappingEngineTransactionManager.Instance.MappCommand(TypeOfSCADACommand.ReadAll, "", 0, 0);

                do
                {
                    try
                    {
                        if (ScadaClient.State == CommunicationState.Created)
                        {
                            ScadaClient.Open();
                        }

                        if (ScadaClient.Ping())
                            break;
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e);
                        Console.WriteLine("GetNetwork() -> SCADA is not available yet.");
                        NetTcpBinding binding = new NetTcpBinding();
                        binding.CloseTimeout = TimeSpan.FromMinutes(10);
                        binding.OpenTimeout = TimeSpan.FromMinutes(10);
                        binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                        binding.SendTimeout = TimeSpan.FromMinutes(10);
                        binding.MaxReceivedMessageSize = Int32.MaxValue;
                        if (ScadaClient.State == CommunicationState.Faulted)
                            ScadaClient = new SCADAClient(new EndpointAddress("net.tcp://localhost:4000/SCADAService"), binding);
                    }
                    Thread.Sleep(500);
                } while (true);
                Console.WriteLine("GetNetwork() -> SCADA is available.");

                Response r = ScadaClient.ExecuteCommand(c);
                descMeas = MappingEngineTransactionManager.Instance.MappResult(r);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            do
            {
                try
                {
                    if (IMSClient.State == CommunicationState.Created)
                    {
                        IMSClient.Open();
                    }

                    if (IMSClient.Ping())
                        break;
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e);
                    Console.WriteLine("GetNetwork() -> IMS is not available yet.");
                    if (IMSClient.State == CommunicationState.Faulted)
                    {
                        NetTcpBinding binding = new NetTcpBinding();
                        binding.CloseTimeout = TimeSpan.FromMinutes(10);
                        binding.OpenTimeout = TimeSpan.FromMinutes(10);
                        binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                        binding.SendTimeout = TimeSpan.FromMinutes(10);
                        binding.MaxReceivedMessageSize = Int32.MaxValue;
                        IMSClient = new IMSClient(new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"), binding);
                    }
                }
                Thread.Sleep(1000);
            } while (true);

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

                // to do: ping
                Response r = scadaClient.ExecuteCommand(c);
                //Response r = SCADAClientInstance.ExecuteCommand(c);

            }
            catch (Exception e)
            { }
        }

        public void SendCrew(IncidentReport report, Crew crew)
        {
            proxyToDispatcherDMS.SendCrewToDms(report, crew);
            return;
        }

        private ScadaDelta GetDeltaForSCADA(Delta d)
        {
            // zasto je ovo bitno, da ima measurement direction?? 
            // po tome odvajas measuremente od ostatka?
            //DMSType type = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(d.);
            List<ResourceDescription> rescDesc = d.InsertOperations.Where(u => (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(u.Id) == DMSType.ANALOG || (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(u.Id) == DMSType.DISCRETE).ToList();
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
                        element.UnitSymbol = ((UnitSymbol)rd.GetProperty(ModelCode.MEASUREMENT_UNITSYMB).AsEnum()).ToString();
                        element.WorkPoint = rd.GetProperty(ModelCode.ANALOG_NORMVAL).AsFloat();
                    }
                    else if (type == "Discrete")
                    {
                        element.Type = DeviceTypes.DIGITAL;
                    }
                }

                
                if(rd.ContainsProperty(ModelCode.DISCRETE_VALIDCOMMANDS))
                {
                    element.ValidCommands = new List<CommandTypes>();
                    foreach( long value in rd.GetProperty(ModelCode.DISCRETE_VALIDCOMMANDS).PropertyValue.LongValues)
                    {
                        if(value == 1)
                        {
                            element.ValidCommands.Add(CommandTypes.OPEN);
                        }
                        else if(value == 0)
                        {
                            element.ValidCommands.Add(CommandTypes.CLOSE);
                        }
                    }
                  
                }

                if (rd.ContainsProperty(ModelCode.DISCRETE_VALIDSTATES))
                {
                    element.ValidStates = new List<OMSSCADACommon.States>();
                    foreach (long value in rd.GetProperty(ModelCode.DISCRETE_VALIDSTATES).PropertyValue.LongValues)
                    {
                        if (value == 1)
                        {
                            element.ValidStates.Add(OMSSCADACommon.States.OPENED);
                        }
                        else if (value == 0)
                        {
                            element.ValidStates.Add(OMSSCADACommon.States.CLOSED);
                        }
                        else
                        {
                            element.ValidStates.Add(OMSSCADACommon.States.UNKNOWN);
                        }
                    }

                }

                if (rd.ContainsProperty(ModelCode.IDOBJ_MRID))
                {
                    //element.Name = rd.GetProperty(ModelCode.IDOBJ_NAME).ToString();
                    element.Name = rd.GetProperty(ModelCode.IDOBJ_MRID).ToString();
                }
                scadaDelta.InsertOps.Add(element);
            }
            return scadaDelta;
        }

        public List<List<ElementStateReport>> GetElementStateReportsForMrID(string mrID)
        {
            return IMSClient.GetElementStateReportsForMrID(mrID);
        }

        public List<List<IncidentReport>> GetReportsForMrID(string mrID)
        {
            return IMSClient.GetReportsForMrID(mrID);
        }

        public List<List<IncidentReport>> GetReportsForSpecificDateSortByBreaker(List<string> mrids, DateTime date)
        {
            return IMSClient.GetReportsForSpecificDateSortByBreaker(mrids, date);
        }

        public List<List<IncidentReport>> GetAllReportsSortByBreaker(List<string> mrids)
        {
            return IMSClient.GetAllReportsSortByBreaker(mrids);
        }
        #endregion
    }
}