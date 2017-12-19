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

        private ConcurrentQueue<IORequestBlock> ioRequests;
        private ConcurrentQueue<IORequestBlock> ioAnswers;

        public ConcurrentQueue<IORequestBlock> IORequests
        {
            get
            {
                return ioRequests;
            }
            set
            {
                if (value != null)
                {
                    ioRequests = value;
                }
            }
        }

        public ConcurrentQueue<IORequestBlock> IOAnswers
        {
            get
            {
                return ioAnswers;
            }
            set
            {
                if (value != null)
                {
                    ioAnswers = value;
                }
            }
        }
        private IORequestsQueue()
        {
            IORequests = new ConcurrentQueue<IORequestBlock>();
            IOAnswers = new ConcurrentQueue<IORequestBlock>();

        }

        public static IORequestsQueue GetQueue()
        {
            if (instance == null)
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new IORequestsQueue();
                    }
                }
            }
            return instance;
        }

        public void EnqueueIOReqForProcess(IORequestBlock iorb)
        {
            IORequests.Enqueue(iorb);
        }

        public IORequestBlock GetRequest()
        {
            IORequestBlock req;

            // pay attention -> made to be blocking
            while (!IORequests.TryDequeue(out req));


            return req;

        }

        public bool IsIORequstEmpty()
        {
            return IORequests.Count == 0;
        }
        public void EnqueueIOAnsForProcess(IORequestBlock iorb)
        {
            IOAnswers.Enqueue(iorb);
        }

        public IORequestBlock GetAnswer()
        {
            IORequestBlock answ;
            IOAnswers.TryDequeue(out answ);
            return answ;
        }

        public bool IsIOAnswersEmpty()
        {
            return IOAnswers.Count == 0;
        }
    }
}
