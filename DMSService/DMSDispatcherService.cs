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
        private static ChannelFactory<IIMSContract> factoryToIMS = new ChannelFactory<IIMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
        private static IIMSContract proxyToIMS = factoryToIMS.CreateChannel();

        public List<ACLine> GetAllACLines()
        {
            List<ACLine> pom = new List<ACLine>();
            foreach (var item in DMSService.tree.Data.Values)
            {
                if (item is ACLine)
                {
                    pom.Add((ACLine)item);
                }
            }
            return pom;
        }

        public List<Consumer> GetAllConsumers()
        {
            List<Consumer> pom = new List<Consumer>();
            foreach (var item in DMSService.tree.Data.Values)
            {
                if (item is Consumer)
                {
                    pom.Add((Consumer)item);
                }
            }
            return pom;
        }

        public List<Node> GetAllNodes()
        {
            List<Node> pom = new List<Node>();
            foreach (var item in DMSService.tree.Data.Values)
            {
                if (item is Node)
                {
                    pom.Add((Node)item);
                }
            }
            return pom;
        }

        public List<Source> GetAllSource()
        {
            List<Source> pom = new List<Source>();
            foreach (var item in DMSService.tree.Data.Values)
            {
                if (item is Source)
                {
                    pom.Add((Source)item);
                }
            }
            return pom;
        }

        public List<Switch> GetAllSwitches()
        {
            List<Switch> pom = new List<Switch>();
            foreach (var item in DMSService.tree.Data.Values)
            {
                if (item is Switch)
                {
                    pom.Add((Switch)item);
                }
            }
            return pom;
        }

        public int GetNetworkDepth()
        {
            return DMSService.tree.Links.Max(x => x.Value.Depth)+1;
        }

        public Source GetTreeRoot()
        {
            Source s = (Source)DMSService.tree.Data[DMSService.tree.Roots[0]];
            return s;
        }

        public Dictionary<long, Element> InitNetwork()
        {
            return DMSService.tree.Data;
        }

        public List<Element> GetAllElements()
        {
            List<Element> retVal = new List<Element>();
            foreach(Element e in DMSService.tree.Data.Values)
            {
                retVal.Add(e);
            }
            return retVal;
        }

        public void SendCrewToDms(DateTime id)
        {
            /*Logic dms*/
            Thread crewprocess = new Thread(() => ProcessCrew(id));
            crewprocess.Start();
            return;

        }

        private void ProcessCrew(DateTime id)
        {
            IncidentReport report = proxyToIMS.GetReport(id);

            if(report != null)
            {
                var rnd = new Random(DateTime.Now.Second);
                int repairtime = rnd.Next(5, 180);

                Thread.Sleep(repairtime*100);

                Switch sw = null;
                foreach (var item in DMSService.tree.Data.Values)
                {
                    if (item.MRID == report.MrID)
                    {
                        sw = (Switch)item;
                        sw.CanCommand = true;
                        break;
                    }
                }

                Array values = Enum.GetValues(typeof(CrewResponse));
                Random rand = new Random();
                ReasonForIncident res = (ReasonForIncident)values.GetValue(rand.Next(1, values.Length));

                report.Reason = res;
                report.RepairTime = TimeSpan.FromMinutes(repairtime);
                report.CrewSent = true;

                Array values1 = Enum.GetValues(typeof(IncidentState));
                report.IncidentState = (IncidentState)values1.GetValue(rand.Next(2, values.Length));

                proxyToIMS.UpdateReport(report);

                Publisher publisher = new Publisher();
                publisher.PublishIncident(report);

                //Publisher publisher = new Publisher();
                //publisher.PublishCrew(new SCADAUpdateModel(sw.ElementGID, true, res));
            }
        }
    }
}
