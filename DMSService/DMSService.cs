using DMSCommon.Model;
using DMSCommon.TreeGraph;
using DMSCommon.TreeGraph.Tree;
using DMSContract;
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
		private static Tree<Element> tree;

        private static DMSService instance = null;
        private DMSService()
        {
            InitializeHosts();
        }
        public static DMSService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DMSService();
                }
                return instance;
            }
        }
        public static int updatesCount = 0;

        #region Properties
        public List<Source> Sources { get => _sources; set => _sources = value; }
        public List<Consumer> Consumers { get => _consumers; set => _consumers = value; }
        public List<ACLine> Aclines { get => _aclines; set => _aclines = value; }
        public List<Switch> Switches { get => _switches; set => _switches = value; }
        public List<Node> ConnecNodes { get => _connecnodes; set => _connecnodes = value; }
		public Tree<Element> Tree
		{
			get
			{
				return tree;
			}
			set
			{
				if (value != null)
				{
					tree = value;
				}
			}
		}
		#endregion



		public void Start()
        {
            StartHosts();
            Tree = InitializeNetwork();
        }

		public Tree<Element> InitializeNetwork()
        {
            ClearLists();
			Tree<Element> retVal = new Tree<Element>();
            List<long> eSources = gda.GetExtentValues(ModelCode.ENERGSOURCE);
            if (eSources.Count == 0)
            {
                return new Tree<Element>();
            }
            List<long> terminals = gda.GetExtentValues(ModelCode.TERMINAL);
            ResourceDescription rd;
            string mrid = "";
            List<NodeLink> links = new List<NodeLink>();
            //Petlja za prikupljanje svih ES i njihovo povezivanje sa CN
            //Imacemo za sad jedan ES
            foreach (long item in eSources)
            {
                rd = gda.GetValues(item);
                mrid = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();

                Source ESource = new Source(item, 0, mrid);

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
                        ESource.End2 = n.ElementGID;
                        ConnecNodes.Add(n);
                        Sources.Add(ESource);
						//dodavanje ES u koren stabla i prvog childa
						retVal.AddRoot(Sources[0].ElementGID, Sources[0]);
						retVal.AddChild(n.Parent, n.ElementGID, n);
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
                        acline.End1 = n.ElementGID;
                        n.Children.Add(acline.ElementGID);
                        long downNodegid = GetConnNodeConnectedWithTerminal(branchTerminals[0]);
                        rd = gda.GetValues(downNodegid);
                        mrid = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                        Node downNode = new Node(downNodegid, mrid, acline, branchTerminals[0]);
                        acline.End2 = downNode.ElementGID;
                        Aclines.Add(acline);
                        ConnecNodes.Add(downNode);
                        terminals.Remove(branchTerminals[0]);

						retVal.AddChild(n.ElementGID, acline.ElementGID, acline);
						retVal.AddChild(acline.ElementGID, downNode.ElementGID, downNode);
                    }
                    else if (mc.Equals(DMSType.BREAKER))
                    {
                        Switch sw = new Switch(bransch, mrid);
                        sw.End1 = n.ElementGID;
                        n.Children.Add(sw.ElementGID);
                        long downNodegid = GetConnNodeConnectedWithTerminal(branchTerminals[0]);
                        rd = gda.GetValues(downNodegid);
                        mrid = rd.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                        Node downNode = new Node(downNodegid, mrid, sw, branchTerminals[0]);
                        sw.End2 = downNode.ElementGID;
                        Switches.Add(sw);
                        ConnecNodes.Add(downNode);
                        terminals.Remove(branchTerminals[0]);

						retVal.AddChild(n.ElementGID, sw.ElementGID, sw);
						retVal.AddChild(sw.ElementGID, downNode.ElementGID, downNode);
                    }
                    else if (mc.Equals(DMSType.ENERGCONSUMER))
                    {
                        Consumer consumer = new Consumer(bransch, mrid);
                        consumer.End1 = n.ElementGID;
                        n.Children.Add(consumer.ElementGID);
                        Consumers.Add(consumer);

						retVal.AddChild(n.ElementGID, consumer.ElementGID, consumer);
                    }
                    else break;

                    terminals.Remove(item);
                }
                count++;
            }
            watch.Stop();
            updatesCount += 1;
            Console.WriteLine("\nNewtork Initialization finished in {0} sec", watch.ElapsedMilliseconds / 1000);
			return retVal;
        }

        private void ClearLists()
        {
            this.Aclines.Clear();
            this.ConnecNodes.Clear();
            this.Consumers.Clear();
            this.Switches.Clear();
            this.Sources.Clear();
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
            ServiceHost transactionHost = new ServiceHost(typeof(DMSTransactionService));
            transactionHost.Description.Name = "DMSTransactionService";
            transactionHost.AddServiceEndpoint(typeof(ITransaction), new NetTcpBinding(), new
            Uri("net.tcp://localhost:8028/DMSTransactionService"));
            hosts.Add(transactionHost);

            ServiceHost dispatcherHost = new ServiceHost(typeof(DMSDispatcherService));
            dispatcherHost.Description.Name = "DMSDispatcherService";
            dispatcherHost.AddServiceEndpoint(typeof(IDMSContract), new NetTcpBinding(), new
            Uri("net.tcp://localhost:8029/DMSDispatcherService"));
            hosts.Add(dispatcherHost);

            ServiceHost scadaHost = new ServiceHost(typeof(DMSServiceForSCADA));
            scadaHost.Description.Name = "DMSServiceForSCADA";
            scadaHost.AddServiceEndpoint(typeof(IDMSToSCADAContract), new NetTcpBinding(), new
            Uri("net.tcp://localhost:8039/IDMSToSCADAContract"));
            hosts.Add(scadaHost);


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
