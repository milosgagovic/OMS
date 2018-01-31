using DMSCommon.Model;
using DMSContract;
using FTN.Common;
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
        List<ITransaction> transactionProxys;
        List<TransactionCallback> transactionCallbacks;
        ChannelFactory<IIMSContract> factoryToIMS;
        IIMSContract proxyToIMS;
        ITransaction proxyTransactionNMS;
        ITransaction proxyTransactionDMS;
        IDMSContract proxyToDispatcherDMS;
        ModelGDATMS gdaTMS;
        SCADAClient scadaClient;
        TransactionCallback callBackTransactionNMS;
        TransactionCallback callBackTransactionDMS;

        public List<ITransaction> TransactionProxys { get => transactionProxys; set => transactionProxys = value; }
        public List<TransactionCallback> TransactionCallbacks { get => transactionCallbacks; set => transactionCallbacks = value; }
        public ITransaction ProxyTransactionNMS { get => proxyTransactionNMS; set => proxyTransactionNMS = value; }
        public ITransaction ProxyTransactionDMS { get => proxyTransactionDMS; set => proxyTransactionDMS = value; }
        public TransactionCallback CallBackTransactionNMS { get => callBackTransactionNMS; set => callBackTransactionNMS = value; }
        public TransactionCallback CallBackTransactionDMS { get => callBackTransactionDMS; set => callBackTransactionDMS = value; }

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
            CallBackTransactionNMS = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionNMS);
            DuplexChannelFactory<ITransaction> factoryTransactionNMS = new DuplexChannelFactory<ITransaction>(CallBackTransactionNMS,
                                                         new NetTcpBinding(),
                                                         new EndpointAddress("net.tcp://localhost:8018/NetworkModelTransactionService"));
            ProxyTransactionNMS = factoryTransactionNMS.CreateChannel();
            TransactionProxys.Add(ProxyTransactionNMS);

            // duplex channel for DMS transaction
            CallBackTransactionDMS = new TransactionCallback();
            TransactionCallbacks.Add(CallBackTransactionDMS);
            DuplexChannelFactory<ITransaction> factoryTransactionDMS = new DuplexChannelFactory<ITransaction>(CallBackTransactionDMS,
                                                            new NetTcpBinding(),
                                                            new EndpointAddress("net.tcp://localhost:8028/DMSTransactionService"));
            ProxyTransactionDMS = factoryTransactionDMS.CreateChannel();
            TransactionProxys.Add(ProxyTransactionDMS);



            // client channel for DMSDispatcherService
            ChannelFactory<IDMSContract> factoryDispatcherDMS = new ChannelFactory<IDMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:8029/DMSDispatcherService"));
            proxyToDispatcherDMS = factoryDispatcherDMS.CreateChannel();


            factoryToIMS = new ChannelFactory<IIMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
            proxyToIMS = factoryToIMS.CreateChannel();
        }

        public void Enlist(Delta d)
        {
            Console.WriteLine("Transaction Manager calling enlist");
            foreach (ITransaction svc in TransactionProxys)
            {
                svc.Enlist();
            }

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

			proxyTransactionNMS.Prepare(delta);
			ScadaDelta scadaDelta = GetDeltaForSCADA(delta);
			do
			{
				Thread.Sleep(50);
			} while (CallBackTransactionNMS.AnswerForPrepare.Equals(TransactionAnswer.Unanswered));

			if (CallBackTransactionNMS.AnswerForPrepare.Equals(TransactionAnswer.Unprepared))
			{
				Rollback();
			}
			else
			{

				TransactionProxys.Where(u => !u.Equals(ProxyTransactionNMS)).ToList().ForEach(x => x.Prepare(delta));
				//foreach (ITransaction svc in TransactionProxys.Where(u => !u.Equals(ProxyTransactionNMS)))
				//{
				//    Console.WriteLine("Type of proxy in prepare:" + svc.GetType().Assembly);
				//    svc.Prepare(delta);
				//}

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
            Enlist(d);
          //  Prepare(d);
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
            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, null, GraphDeep, null, null, null);
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

            }

            var crews = proxyToIMS.GetCrews();
            var incidentReports = proxyToIMS.GetAllReports();

            TMSAnswerToClient answer = new TMSAnswerToClient(resourceDescriptionFromNMS, listOfDMSElement, GraphDeep, descMeas, crews, incidentReports);
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

    //public void SendCrew(string mrid)
    //{
    //    proxyToDispatcherDMS.SendCrewToDms(mrid);
    //    return;
    //}

    public void SendCrew(IncidentReport report)
        {
            proxyToDispatcherDMS.SendCrewToDms(report);
            return;
        }

        public void SendCrew(string mrid)
        {
            throw new NotImplementedException();
        }

        public List<ElementStateReport> GetAllElementStateReports()
        {
            return proxyToIMS.GetAllElementStateReports();
        }

        public List<ElementStateReport> GetElementStateReportsForMrID(string mrID)
        {
            return proxyToIMS.GetElementStateReportsForMrID(mrID);
        }

        public List<ElementStateReport> GetElementStateReportsForSpecificTimeInterval(DateTime startTime, DateTime endTime)
        {
            return proxyToIMS.GetElementStateReportsForSpecificTimeInterval(startTime, endTime);
        }

        public List<ElementStateReport> GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(string mrID, DateTime startTime, DateTime endTime)
        {
            return proxyToIMS.GetElementStateReportsForSpecificMrIDAndSpecificTimeInterval(mrID, startTime, endTime);
        }

		public List<Crew> GetCrews()
		{
			return proxyToIMS.GetCrews();
		}

		public bool AddCrew(Crew crew)
		{
			return proxyToIMS.AddCrew(crew);
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
					element.Name = rd.GetProperty(ModelCode.IDOBJ_NAME).ToString();
				}
				scadaDelta.InsertOps.Add(element);
			}
			return scadaDelta;
		}
		#endregion
	}
}
