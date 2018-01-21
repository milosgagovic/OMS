using DMSCommon.Model;
using DMSContract;
using FTN.Common;
using OMSSCADACommon;
using PubSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSService
{
    public class DMSServiceForSCADA : IDMSToSCADAContract
    {
        public void ChangeOnSCADA(string mrID, OMSSCADACommon.States state)
        {
            ModelGdaDMS gda = new ModelGdaDMS();
            List<ResourceDescription> rdl = gda.GetExtentValuesExtended(ModelCode.DISCRETE);
            ResourceDescription rd = rdl.Where(r => r.GetProperty(ModelCode.IDOBJ_MRID).AsString() == mrID).FirstOrDefault();

            long res = rd.GetProperty(ModelCode.MEASUREMENT_PSR).AsLong();

            // ovde pozvati algoritam za energizaciju/deenergizaciju koji vraca ovu listu promena (GID, true/false)
            List<SCADAUpdateModel> networkChange = new List<SCADAUpdateModel>();

            Element sw;
            DMSService.tree.Data.TryGetValue(res, out sw);
            Switch sw1 = (Switch)sw;
            

            if (state == OMSSCADACommon.States.OPENED)
            {
                
                sw1.Marker = false;
                networkChange.Add(new SCADAUpdateModel(sw.ElementGID,false));
                Node n = (Node)DMSService.tree.Data[sw1.End2];
                n.Marker = false;
                networkChange.Add(new SCADAUpdateModel(n.ElementGID,false));
                networkChange = GetNewtworkChange(n, networkChange,false);

            }
            else if (state == OMSSCADACommon.States.CLOSED)
            {
                sw1.Marker = true;
                networkChange.Add(new SCADAUpdateModel(sw1.ElementGID,true));
                Node n = (Node)DMSService.tree.Data[sw1.End2];
                n.Marker = true;
                networkChange.Add(new SCADAUpdateModel(n.ElementGID,true));
                networkChange = GetNewtworkChange(n, networkChange, true);
            }
            Source s =(Source) DMSService.tree.Data[DMSService.tree.Roots[0]];
            networkChange.Add(new SCADAUpdateModel(s.ElementGID, true));

            if (networkChange.Count > 0)
            {
                Publisher publisher = new Publisher();

                publisher.PublishUpdate(networkChange);
            }
       
        }
        public List<SCADAUpdateModel> GetNewtworkChange(Node n, List<SCADAUpdateModel> networkChange,bool isEnergized)
        {
            foreach (long item in n.Children)
            {
                Element e = DMSService.tree.Data[item];
                if (e is Consumer)
                {
                    e.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(e.ElementGID,isEnergized));
                }
                else if (e is Switch)
                {
                    Element switche;
                    DMSService.tree.Data.TryGetValue(e.ElementGID, out switche);
                    Switch s = (Switch)switche;
                    s.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(s.ElementGID,isEnergized));
                    Node node = (Node)DMSService.tree.Data[s.End2];
                    node.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(node.ElementGID,isEnergized));
                    networkChange = GetNewtworkChange(node, networkChange,isEnergized);
                }
                else if (e is ACLine)
                {
                    Element acl;
                    DMSService.tree.Data.TryGetValue(e.ElementGID, out acl);
                    ACLine ac = (ACLine)acl;
                    ac.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(ac.ElementGID,isEnergized));
                    Node node = (Node)DMSService.tree.Data[ac.End2];
                    node.Marker = isEnergized;
                    networkChange.Add(new SCADAUpdateModel(node.ElementGID,isEnergized));
                    networkChange = GetNewtworkChange(node, networkChange,isEnergized);
                }
            }

            return networkChange;

        }


    }
}
