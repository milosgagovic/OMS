using PCCommon;
using System;
using System.Collections.Concurrent;

namespace SCADA.CommunicationAndControlling
{
    /// <summary>
    /// Used for inter-thread communication
    /// (between ACQEngine and PCCommunicationEngine)
    /// </summary>
    public class IORequestsQueue
    {
        private static object syncObj = new object();
        private static volatile IORequestsQueue instance;

        private BlockingCollection<IORequestBlock> ioRequests;
        private BlockingCollection<IORequestBlock> ioAnswers;

        public BlockingCollection<IORequestBlock> IORequests
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
        public BlockingCollection<IORequestBlock> IOAnswers
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
            IORequests = new BlockingCollection<IORequestBlock>();
            IOAnswers = new BlockingCollection<IORequestBlock>();
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

        /* IORequests queue methods */
        public void EnqueueRequest(IORequestBlock iorb)
        {
            IORequests.Add(iorb);
        }

        //public IORequestBlock DequeueRequest(out bool isSuccessful)
        //{
        //    IORequestBlock req;
        //    // try dequeue is not blocking
        //    //isSuccessful = IORequests.TryDequeue(out req);
        //    isSuccessful = IORequests.TryTake(out req);
        //    return req;
        //}

        public IORequestBlock DequeueRequest(out bool isSuccessful, TimeSpan? timeout = null)
        {
            if (timeout == null)
                timeout = TimeSpan.FromMilliseconds(0);

            TimeSpan tryTakeTimeout = (TimeSpan)timeout;
            IORequestBlock req;
            isSuccessful = IORequests.TryTake(out req, tryTakeTimeout);
            return req;
        }

        /* IOAnswers queue methods */
        public void EnqueueAnswer(IORequestBlock iorb)
        {
            IOAnswers.Add(iorb);
        }

        public IORequestBlock DequeueAnswer(out bool isSuccessful, TimeSpan? timeout = null)
        {
            if (timeout == null)
                timeout = TimeSpan.FromMilliseconds(0);

            TimeSpan tryTakeTimeout = (TimeSpan)timeout;
            IORequestBlock answ;
            isSuccessful = IOAnswers.TryTake(out answ, tryTakeTimeout);
            return answ;
        }
    }
}
