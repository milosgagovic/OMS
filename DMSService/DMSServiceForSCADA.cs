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
            // error ako ne ugasis modbus simulator nakon gasenja sistema, i onda
            // opet pokrenes sve
            DMSService.tree.Data.TryGetValue(res, out el);
            Switch sw = (Switch)el;
            //proxyToIMS.AddReport(sw.MRID, DateTime.UtcNow, state.ToString());

            bool isIncident = false;
            IncidentReport incident = new IncidentReport() { MrID = sw.MRID };

            if (state == OMSSCADACommon.States.OPENED)
            {
                proxyToIMS.AddReport(incident);
                isIncident = true;

                sw.Marker = false;
                sw.State = SwitchState.Open;
                networkChange.Add(new SCADAUpdateModel(sw.ElementGID, false));
                Node n = (Node)DMSService.tree.Data[sw.End2];
                n.Marker = false;
                networkChange.Add(new SCADAUpdateModel(n.ElementGID, false));
                networkChange = GetNetworkChange(n, networkChange, false);
            }
            else if (state == OMSSCADACommon.States.CLOSED)
            {
                sw.State = SwitchState.Closed;
                if (TraceUp((Node)DMSService.tree.Data[sw.End1]))
                {
                    
                    sw.Marker = true;
                    networkChange.Add(new SCADAUpdateModel(sw.ElementGID, true));
                    Node n = (Node)DMSService.tree.Data[sw.End2];
                    n.Marker = true;
                    networkChange.Add(new SCADAUpdateModel(n.ElementGID, true));
                    networkChange = GetNetworkChange(n, networkChange, true);
                }
                else
                {
                    return;
                }
            }
            Source s = (Source)DMSService.tree.Data[DMSService.tree.Roots[0]];
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
            Element el = DMSService.tree.Data[no.Parent];

            if (DMSService.tree.Data[el.ElementGID] is Source)
            {
                return true;
            }
            else if (DMSService.tree.Data[el.ElementGID] is Switch)
            {
                Switch s = (Switch)DMSService.tree.Data[el.ElementGID];
                if (s.Marker == true && s.State == SwitchState.Closed)
                    return true;
                else
                    return false;
            }
            else if (DMSService.tree.Data[el.ElementGID] is ACLine)
            {
                ACLine acl = (ACLine)DMSService.tree.Data[el.ElementGID];
                Node n = (Node)DMSService.tree.Data[acl.End1];

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
                Element e = DMSService.tree.Data[item];
                if (e is Consumer)
                {
                    e.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(e.ElementGID, isEnergized));
                }
                else if (e is Switch)
                {
                    Element switche;
                    DMSService.tree.Data.TryGetValue(e.ElementGID, out switche);
                    Switch s = (Switch)switche;
                    if (s.State == SwitchState.Open)
                    {
                        continue;
                    }

                    s.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(s.ElementGID, isEnergized));
                    Node node = (Node)DMSService.tree.Data[s.End2];
                    node.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(node.ElementGID, isEnergized));
                    networkChange = GetNetworkChange(node, networkChange, isEnergized);
                }
                else if (e is ACLine)
                {
                    Element acl;
                    DMSService.tree.Data.TryGetValue(e.ElementGID, out acl);
                    ACLine ac = (ACLine)acl;
                    ac.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(ac.ElementGID, isEnergized));
                    Node node = (Node)DMSService.tree.Data[ac.End2];
                    node.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(node.ElementGID, isEnergized));
                    networkChange = GetNetworkChange(node, networkChange, isEnergized);
                }
            }

            return networkChange;

        }


    }
}
