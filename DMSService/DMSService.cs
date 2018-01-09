using DMSCommon.Model;
using DMSCommon.TreeGraph;
using DMSCommon.TreeGraph.Tree;
using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionManagerContract;

namespace DMSService
{
    public class DMSService : IDisposable
    {
        private List<ServiceHost> hosts = null;
        private List<Source> _sources = new List<Source>();
        private List<Consumer> _consumers = new List<Consumer>();
        private List<ACLine> _aclines = new List<ACLine>();
        private List<Switch> _switches = new List<Switch>();
        private List<Node> _connecnodes = new List<Node>();
        private ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private ModelGdaDMS gda = new ModelGdaDMS();


        #region Properties
        public List<Source> Sources { get => _sources; set => _sources = value; }
        public List<Consumer> Consumers { get => _consumers; set => _consumers = value; }
        public List<ACLine> Aclines { get => _aclines; set => _aclines = value; }
        public List<Switch> Switches { get => _switches; set => _switches = value; }
        public List<Node> ConnecNodes { get => _connecnodes; set => _connecnodes = value; }
        public static Tree<Element> tree;
        #endregion

        public DMSService()
        {
            InitializeHosts();
        }

        public void Start()
        {
            StartHosts();
            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            List<long> eSources = gda.GetExtentValues(ModelCode.ENERGSOURCE);
            List<long> terminals = gda.GetExtentValues(ModelCode.TERMINAL);
            ResourceDescription rd;
            string mrid = "";
            tree = new Tree<Element>();
            List<NodeLink> links = new List<NodeLink>();
            //Petlja za prikupljanje svih ES i njihovo povezivanje sa CN
            //Imacemo za sad jedan ES
            foreach (long item in eSources)
            {
                rd = gda.GetValues(item);
                mrid = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();

                Source ESource = new Source(item, null, mrid);

                ModelCode propertyId = ModelCode.CONDUCTEQUIP_TERMINALS;
                ModelCode type = ModelCode.TERMINAL;

                Association asso = new Association()
                {
                    PropertyId = propertyId,
                    Type = type
                };
                //Veza ES i CN preko terminala
                List<long> terms = gda.GetRelatedValues(item, asso);
                if (terms.Count != 0)
                {
                    Association assoTerm = new Association()
                    {
                        PropertyId = ModelCode.TERMINAL_CONNECTNODE,
                        Type = ModelCode.CONNECTNODE
                    };
                    List<long> connNode = gda.GetRelatedValues(terms[0], assoTerm);
                    if (connNode.Count != 0)
                    {
                        rd = gda.GetValues(connNode[0]);
                        mrid = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                        Node n = new Node(connNode[0], mrid, ESource, terms[0]);
                        ESource.End2 = n;
                        ConnecNodes.Add(n);
                        Sources.Add(ESource);
                        //dodavanje ES u koren stabla i prvog childa
                        tree.AddRoot(Sources[0].ElementGID, Sources[0]);
                        tree.AddChild(n.Parent.ElementGID, n.ElementGID, n);
                    }
                }
                terminals.Remove(terms[0]);
            }
            //Obrada od pocetnog CN ka svim ostalima. Iteracija po terminalima
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int count = 0;
            while (terminals.Count != 0)
            {
                Node n = ConnecNodes.ElementAt(count);
                List<long> terms = GetTerminalsConnectedWithConnNode(n.ElementGID);
                if (terms.Contains(n.UpTerminal))
                {
                    terms.Remove(n.UpTerminal);
                }
                foreach (long item in terms)
                {
                    long bransch = GetBranchConnectedWithTerminal(item);
                    DMSType mc;
                    if (bransch != 0)
                    {
                        mc = (DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(bransch);
                        rd = gda.GetValues(bransch);
                        mrid = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                    }
                    else
                    {
                        continue;
                    }

                    List<long> branchTerminals = GetTerminalsConnectedWithBranch(bransch);
                    if (branchTerminals.Contains(item))
                    {
                        branchTerminals.Remove(item);
                    }

                    if (mc.Equals(DMSType.ACLINESEGMENT))
                    {
                        ACLine acline = new ACLine(bransch, mrid);
                        acline.End1 = n;
                        n.Children.Add(acline);
                        long downNodegid = GetConnNodeConnectedWithTerminal(branchTerminals[0]);
                        rd = gda.GetValues(downNodegid);
                        mrid = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                        Node downNode = new Node(downNodegid, mrid, acline, branchTerminals[0]);
                        acline.End2 = downNode;
                        Aclines.Add(acline);
                        ConnecNodes.Add(downNode);
                        terminals.Remove(branchTerminals[0]);

                        tree.AddChild(n.ElementGID, acline.ElementGID, acline);
                        tree.AddChild(acline.ElementGID, downNode.ElementGID, downNode);
                    }
                    else if (mc.Equals(DMSType.BREAKER))
                    {
                        Switch sw = new Switch(bransch, mrid);
                        sw.End1 = n;
                        n.Children.Add(sw);
                        long downNodegid = GetConnNodeConnectedWithTerminal(branchTerminals[0]);
                        rd = gda.GetValues(downNodegid);
                        mrid = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                        Node downNode = new Node(downNodegid, mrid, sw, branchTerminals[0]);
                        sw.End2 = downNode;
                        Switches.Add(sw);
                        ConnecNodes.Add(downNode);
                        terminals.Remove(branchTerminals[0]);

                        tree.AddChild(n.ElementGID, sw.ElementGID, sw);
                        tree.AddChild(sw.ElementGID, downNode.ElementGID, downNode);
                    }
                    else if (mc.Equals(DMSType.ENERGCONSUMER))
                    {
                        Consumer consumer = new Consumer(bransch, mrid);
                        consumer.End1 = n;
                        n.Children.Add(consumer);
                        Consumers.Add(consumer);

                        tree.AddChild(n.ElementGID, consumer.ElementGID, consumer);
                    }
                    else break;

                    terminals.Remove(item);
                }
                count++;
            }
            watch.Stop();
            Console.WriteLine("\nNewtork Initialization end in {0} sec",watch.ElapsedMilliseconds/1000);
        }

        #region GetRelatedMethods

        private long GetConnNodeConnectedWithTerminal(long terminal)
        {
            Association assoTerm = new Association()
            {
                PropertyId = ModelCode.TERMINAL_CONNECTNODE,
                Type = ModelCode.CONNECTNODE
            };
            List<long> connNode = gda.GetRelatedValues(terminal, assoTerm);

            return connNode[0];
        }
        private long GetTerminalConnectedWithBranch(long branch)
        {
            Association assoTerm = new Association()
            {
                PropertyId = ModelCode.CONDUCTEQUIP_TERMINALS,
                Type = ModelCode.TERMINAL
            };
            List<long> term = gda.GetRelatedValues(branch, assoTerm);

            return term[0];
        }

        private List<long> GetTerminalsConnectedWithConnNode(long connNode)
        {
            Association assoTerm = new Association()
            {
                PropertyId = ModelCode.CONNECTNODE_TERMINALS,
                Type = ModelCode.TERMINAL
            };
            List<long> term = gda.GetRelatedValues(connNode, assoTerm);

            return term;
        }

        private long GetBranchConnectedWithTerminal(long terminal)
        {
            ModelCode[] branc = { ModelCode.BREAKER, ModelCode.ACLINESEGMENT, ModelCode.ENERGCONSUMER };
            foreach (ModelCode item in branc)
            {
                Association assoTerm = new Association()
                {
                    PropertyId = ModelCode.TERMINAL_CONDEQUIP,
                    Type = item
                };
                List<long> branch = gda.GetRelatedValues(terminal, assoTerm);
                if (branch.Count > 0)
                    return branch[0];
            }
            return 0;
        }

        private List<long> GetTerminalsConnectedWithBranch(long branch)
        {
            Association assoTerm = new Association()
            {
                PropertyId = ModelCode.CONDUCTEQUIP_TERMINALS,
                Type = ModelCode.TERMINAL
            };
            List<long> term = gda.GetRelatedValues(branch, assoTerm);

            return term;
        }
        #endregion

        public void Dispose()
        {
            CloseHosts();
            GC.SuppressFinalize(this);
        }

        private void InitializeHosts()
        {
            hosts = new List<ServiceHost>();
            ServiceHost svc = new ServiceHost(typeof(DMSTransactionService));
            svc.Description.Name = "DMSTransactionService";
            svc.AddServiceEndpoint(typeof(ITransaction), new NetTcpBinding(), new
            Uri("net.tcp://localhost:8028/DMSTransactionService"));
            hosts.Add(svc);
        }

        private void StartHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("DMS Services can not be opend because it is not initialized.");
            }

            string message = string.Empty;
            foreach (ServiceHost host in hosts)
            {
                host.Open();

                message = string.Format("The WCF service {0} is ready.", host.Description.Name);
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

                message = "Endpoints:";
                Console.WriteLine(message);
                CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);

                foreach (Uri uri in host.BaseAddresses)
                {
                    Console.WriteLine(uri);
                    CommonTrace.WriteTrace(CommonTrace.TraceInfo, uri.ToString());
                }

                Console.WriteLine("\n");
            }

            message = string.Format("Trace level: {0}", CommonTrace.TraceLevel);
            Console.WriteLine(message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);


            message = "The Distribution Management System Service is started.";
            Console.WriteLine("\n{0}", message);
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
        }

        public void CloseHosts()
        {
            if (hosts == null || hosts.Count == 0)
            {
                throw new Exception("DMS Services can not be closed because it is not initialized.");
            }

            foreach (ServiceHost host in hosts)
            {
                host.Close();
            }

            string message = "The DMS Service is closed.";
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            Console.WriteLine("\n\n{0}", message);
        }
    }
}
