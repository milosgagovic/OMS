using DMSCommon.Model;
using DMSContract;
using FTN.Common;
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
    public class DMSServiceForSCADA : IDMSToSCADAContract
    {
		private static ChannelFactory<IIMSContract> factoryToIMS = new ChannelFactory<IIMSContract>(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:6090/IncidentManagementSystemService"));
		private static IIMSContract proxyToIMS = factoryToIMS.CreateChannel();
        
        public void ChangeOnSCADA(string mrID, OMSSCADACommon.States state)
        {
			
            ModelGdaDMS gda = new ModelGdaDMS();
            List<ResourceDescription> rdl = gda.GetExtentValuesExtended(ModelCode.DISCRETE);
            ResourceDescription rd = rdl.Where(r => r.GetProperty(ModelCode.IDOBJ_MRID).AsString() == mrID).FirstOrDefault();
           
            long res = rd.GetProperty(ModelCode.MEASUREMENT_PSR).AsLong();
            
            List<SCADAUpdateModel> networkChange = new List<SCADAUpdateModel>();

            Element el;
            Console.WriteLine("Change on scada Instance.Tree");
            DMSService.Instance.Tree.Data.TryGetValue(res, out el);
            Switch sw = (Switch)el;

            bool isIncident = false;
            IncidentReport incident = new IncidentReport() { MrID = sw.MRID };

            // ***************************************OVDE DODATI LOGIKU ZA BIRANJE TIPA TIMA************************************************************
            Random rand = new Random();
            Array values = Enum.GetValues(typeof(CrewType));
            incident.Crewtype = (CrewType)values.GetValue(rand.Next(0, values.Length));

            ElementStateReport elementStateReport = new ElementStateReport() { MrID = sw.MRID, Time = DateTime.UtcNow, State = state.ToString() };

            if (state == OMSSCADACommon.States.OPENED)
            {
                proxyToIMS.AddReport(incident);
                isIncident = true;

                sw.Marker = false;
                sw.State = SwitchState.Open;
                networkChange.Add(new SCADAUpdateModel(sw.ElementGID, false, OMSSCADACommon.States.OPENED));
                Node n = (Node)DMSService.Instance.Tree.Data[sw.End2];
                n.Marker = false;
                networkChange.Add(new SCADAUpdateModel(n.ElementGID, false));
                networkChange = GetNetworkChange(n, networkChange, false);
            }
            else if (state == OMSSCADACommon.States.CLOSED)
            {
                sw.State = SwitchState.Closed;
                if (TraceUp((Node)DMSService.Instance.Tree.Data[sw.End1]))
                {
                    networkChange.Add(new SCADAUpdateModel(sw.ElementGID, true, OMSSCADACommon.States.CLOSED));
                    sw.Marker = true;
                    Node n = (Node)DMSService.Instance.Tree.Data[sw.End2];
                    n.Marker = true;
                    networkChange.Add(new SCADAUpdateModel(n.ElementGID, true));
                    networkChange = GetNetworkChange(n, networkChange, true);
                }
                else
                {
                    networkChange.Add(new SCADAUpdateModel(sw.ElementGID, false, OMSSCADACommon.States.CLOSED));
                }
            }

            //upisati promijenu stanja elementa
            proxyToIMS.AddElementStateReport(elementStateReport);

            Source s = (Source)DMSService.Instance.Tree.Data[DMSService.Instance.Tree.Roots[0]];
            networkChange.Add(new SCADAUpdateModel(s.ElementGID, true));

            Publisher publisher = new Publisher();
            if (networkChange.Count > 0)
            {
                publisher.PublishUpdate(networkChange);
            }
            if (isIncident)
            {
                //Thread.Sleep(1000);
                publisher.PublishIncident(incident);
            }
        }

        private bool TraceUp(Node no)
        {
            Element el = DMSService.Instance.Tree.Data[no.Parent];

            if (DMSService.Instance.Tree.Data[el.ElementGID] is Source)
            {
                return true;
            }
            else if (DMSService.Instance.Tree.Data[el.ElementGID] is Switch)
            {
                Switch s = (Switch)DMSService.Instance.Tree.Data[el.ElementGID];
                if (s.Marker == true && s.State == SwitchState.Closed)
                    return true;
                else
                    return false;
            }
            else if (DMSService.Instance.Tree.Data[el.ElementGID] is ACLine)
            {
                ACLine acl = (ACLine)DMSService.Instance.Tree.Data[el.ElementGID];
                Node n = (Node)DMSService.Instance.Tree.Data[acl.End1];

                if (TraceUp(n))
                    return true;
                else
                    return false;
            }
            return false;
        }

        private List<SCADAUpdateModel> GetNetworkChange(Node n, List<SCADAUpdateModel> networkChange, bool isEnergized)
        {
            foreach (long item in n.Children)
            {
                Element e = DMSService.Instance.Tree.Data[item];
                if (e is Consumer)
                {
                    e.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(e.ElementGID, isEnergized));
                }
                else if (e is Switch)
                {
                    Element switche;
                    DMSService.Instance.Tree.Data.TryGetValue(e.ElementGID, out switche);
                    Switch s = (Switch)switche;
                    if (s.State == SwitchState.Open)
                    {
                        continue;
                    }

                    s.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(s.ElementGID, isEnergized));
                    Node node = (Node)DMSService.Instance.Tree.Data[s.End2];
                    node.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(node.ElementGID, isEnergized));
                    networkChange = GetNetworkChange(node, networkChange, isEnergized);
                }
                else if (e is ACLine)
                {
                    Element acl;
                    DMSService.Instance.Tree.Data.TryGetValue(e.ElementGID, out acl);
                    ACLine ac = (ACLine)acl;
                    ac.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(ac.ElementGID, isEnergized));
                    Node node = (Node)DMSService.Instance.Tree.Data[ac.End2];
                    node.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(node.ElementGID, isEnergized));
                    networkChange = GetNetworkChange(node, networkChange, isEnergized);
                }
            }

            return networkChange;

        }
    }
}
