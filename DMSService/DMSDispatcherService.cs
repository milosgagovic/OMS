using DMSCommon;
using DMSCommon.Model;
using DMSContract;
using IMSContract;
using OMSSCADACommon;
using PubSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMSService
{
    public class DMSDispatcherService : IDMSContract
    {
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

        public DMSDispatcherService()
        {
            Console.WriteLine("Dispatcher instantiated");
        }

        public List<Element> GetAllElements()
        {
            List<Element> retVal = new List<Element>();
            try
            {
                foreach (Element e in DMSService.Instance.Tree.Data.Values)
                {
                    retVal.Add(e);
                }
                return retVal;
            }
            catch (Exception)
            {

                return new List<Element>();
            }
        }

        public int GetNetworkDepth()
        {
            try
            {
                return DMSService.Instance.Tree.Links.Max(x => x.Value.Depth) + 1;

            }
            catch (Exception)
            {
                return 1;
            }
        }

        public List<ACLine> GetAllACLines()
        {
            List<ACLine> pom = new List<ACLine>();
            try
            {
                foreach (var item in DMSService.Instance.Tree.Data.Values)
                {
                    if (item is ACLine)
                    {
                        pom.Add((ACLine)item);
                    }
                }
                return pom;
            }
            catch (Exception)
            {

                return new List<ACLine>();
            }

        }

        public List<Consumer> GetAllConsumers()
        {
            List<Consumer> pom = new List<Consumer>();
            try
            {
                foreach (var item in DMSService.Instance.Tree.Data.Values)
                {
                    if (item is Consumer)
                    {
                        pom.Add((Consumer)item);
                    }
                }
                return pom;
            }
            catch (Exception)
            {

                return new List<Consumer>();
            }

        }

        public List<Node> GetAllNodes()
        {
            List<Node> pom = new List<Node>();
            try
            {
                foreach (var item in DMSService.Instance.Tree.Data.Values)
                {
                    if (item is Node)
                    {
                        pom.Add((Node)item);
                    }
                }
                return pom;
            }
            catch (Exception)
            {

                return new List<Node>();
            }

        }

        public List<Source> GetAllSources()
        {
            List<Source> pom = new List<Source>();
            try
            {
                foreach (var item in DMSService.Instance.Tree.Data.Values)
                {
                    if (item is Source)
                    {
                        pom.Add((Source)item);
                    }
                }
                return pom;
            }
            catch (Exception)
            {

                return new List<Source>();
            }

        }

        public List<Switch> GetAllSwitches()
        {
            List<Switch> pom = new List<Switch>();
            try
            {
                foreach (var item in DMSService.Instance.Tree.Data.Values)
                {
                    if (item is Switch)
                    {
                        pom.Add((Switch)item);
                    }
                }
                return pom;
            }
            catch (Exception)
            {

                return new List<Switch>();
            }

        }

        public Source GetTreeRoot()
        {
            try
            {
                Source s = (Source)DMSService.Instance.Tree.Data[DMSService.Instance.Tree.Roots[0]];
                return s;


            }
            catch (Exception)
            {

                return new Source();
            }
        }

        public Dictionary<long, Element> InitNetwork()
        {
            try
            {
                return DMSService.Instance.Tree.Data;
            }
            catch (Exception)
            {

                return new Dictionary<long, Element>();
            }
        }

        public void SendCrewToDms(IncidentReport report)
        {
            /*Logic dms*/
            Thread crewprocess = new Thread(() => ProcessCrew(report));
            crewprocess.Start();
            return;

        }

        private void ProcessCrew(IncidentReport report)
        {
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
                    Console.WriteLine("ProcessCrew() -> IMS is not available yet.");
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

            Publisher publisher = new Publisher();
            report.Id = IMSClient.GetReport(report.Time).Id;

            if (report != null)
            {
                if (report.Crewtype == CrewType.Investigation)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(4));

                    var rnd = new Random(DateTime.Now.Second);
                    int repairtime = rnd.Next(30, 180);

                    Array values = Enum.GetValues(typeof(ReasonForIncident));
                    Random rand = new Random();
                    ReasonForIncident res = (ReasonForIncident)values.GetValue(rand.Next(1, values.Length));

                    report.Reason = res;
                    report.RepairTime = TimeSpan.FromMinutes(repairtime);
                    report.CrewSent = true;

                    report.IncidentState = IncidentState.READY_FOR_REPAIR;
                    report.Crewtype = CrewType.Repair;
                }
                else if (report.Crewtype == CrewType.Repair)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(report.RepairTime.TotalMinutes / 10));

                    report.IncidentState = IncidentState.REPAIRED;

                    Switch sw = null;
                    foreach (var item in DMSService.Instance.Tree.Data.Values)
                    {
                        if (item.MRID == report.MrID)
                        {
                            sw = (Switch)item;

                            if (item.UnderSCADA)
                            {
                                sw.CanCommand = true;
                                break;
                            }
                            else
                            {
                                sw.State = SwitchState.Closed;
                                ElementStateReport elementStateReport = new ElementStateReport() { MrID = sw.MRID, Time = DateTime.UtcNow, State = 0 };
                                IMSClient.AddElementStateReport(elementStateReport);

                                List<UIUpdateModel> networkChange = new List<UIUpdateModel>();
                                if (EnergizationAlgorithm.TraceUp((Node)DMSService.Instance.Tree.Data[sw.End1], DMSService.Instance.Tree))
                                {
                                    networkChange.Add(new UIUpdateModel(sw.ElementGID, true, OMSSCADACommon.States.CLOSED));
                                    sw.Marker = true;
                                    Node n = (Node)DMSService.Instance.Tree.Data[sw.End2];
                                    n.Marker = true;
                                    networkChange.Add(new UIUpdateModel(n.ElementGID, true));
                                    networkChange = EnergizationAlgorithm.TraceDown(n, networkChange, true, false, DMSService.Instance.Tree);
                                }
                                else
                                {
                                    networkChange.Add(new UIUpdateModel(sw.ElementGID, false, OMSSCADACommon.States.CLOSED));
                                }

                                publisher.PublishUpdateDigital(networkChange);
                                break;
                            }
                        }
                    }
                }

                IMSClient.UpdateReport(report);

                publisher.PublishIncident(report);
            }
        }

        public bool IsNetworkAvailable()
        {
            return DMSService.isNetworkInitialized;
        }
    }
}