using DMSCommon.Model;
using DMSContract;
using OMSSCADACommon;
using PubSubscribe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMSService
{
    public class DMSDispatcherService : IDMSContract
    {
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

        public void SendCrewToDms(string mrid)
        {
            /*Logic dms*/
            Thread crewprocess = new Thread(() => ProcessCrew(mrid));
            crewprocess.Start();
            return;

        }

        private void ProcessCrew(string v)
        {
            Thread.Sleep(10000);

            Switch sw = null;
            foreach (var item in DMSService.tree.Data.Values)
            {
                if (item.MRID == v)
                {
                    sw = (Switch)item;
                    sw.CanCommand = true;
                    break;
                }
            }
            Array values = Enum.GetValues(typeof(CrewResponse));
            Random rand = new Random();
            CrewResponse res = (CrewResponse)values.GetValue(rand.Next(values.Length));


            Publisher publisher = new Publisher();

            publisher.PublishCrew(new SCADAUpdateModel(sw.ElementGID, true, res));


        }
    }
}
