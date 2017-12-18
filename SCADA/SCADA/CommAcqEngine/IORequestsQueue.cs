using PCCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SCADA.CommAcqEngine
{
    public class IORequestsQueue
    {
        private static object syncObj = new object();
        private static volatile IORequestsQueue instance;

        private static BlockingCollection<IORequestBlock> IORequests;

        private IORequestsQueue()
        {
            IORequests = new BlockingCollection<IORequestBlock>();
        }

        public static IORequestsQueue GetQueue()
        {
            if (IORequests == null)
            {
                lock (syncObj)
                {
                    if (IORequests == null)
                    {
                        instance = new IORequestsQueue();
                    }
                }
            }
            return instance;
        }

        public void EnqueueIOReqForProcess(IORequestBlock iorb)
        {
            IORequests.Add(iorb);
        }

        public IORequestBlock GetRequest()
        {
            return IORequests.Take();
        }

        public bool IsEmpty()
        {
            return IORequests.Count == 0;
        }

    }
}
