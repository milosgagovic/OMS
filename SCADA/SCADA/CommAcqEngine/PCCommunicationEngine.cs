using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ModbusTCPDriver;
using PCCommon;

namespace SCADA.CommAcqEngine
{
    // Logic for communication with Process Controller
    public class PCCommunicationEngine
    {
        IIndustryProtocolHandler protHandler = new ModbusHandler();

        IORequestsQueue IORequests;
        bool shutdown;
        int timerMsc;

        public PCCommunicationEngine()
        {
            IORequests = IORequestsQueue.GetQueue();
            shutdown = false;
            timerMsc = 1000;
        }
        public void AddIOReqForProcess(IORequestBlock iorb)
        {
            IORequests.EnqueueIOReqForProcess(iorb);
           
        }

        // ovde treba srediti komunikaciju sa svim ucitanim-konfigurisanim RTU-ovima i kanalima
        public void StartCommunication()
        {

        }

        // obrada zahteva iz IORB Queue-a
        public async void StartProcessing()
        {
            while (!shutdown)
            {

                if (!IORequests.IsIORequstEmpty())
                {
                    Console.WriteLine("Request processing");
                    var req=IORequests.GetRequest();
                    protHandler.PackData(req.sendBuff);
                    
                }
                else
                {
                    Console.WriteLine("There is no requests for processing");
                }

                await Task.Delay(timerMsc);

                //Thread.Sleep(timerMsc);               
            }
        }

    }
}
