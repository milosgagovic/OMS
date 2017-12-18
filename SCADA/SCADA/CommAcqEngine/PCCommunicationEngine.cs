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

        // konfigurisati komunikacione linkove
        // pokrenuti nit za procesuiranje IORB-a

        // rukovaoc konkretnog industrijskog protokola
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
        // tu treba cela logika prosledjivanja na odredjeni kanal, koji je otvoren ranije
        public async void StartProcessing(CancellationToken token)
        {
            Task<byte[]> readBytesTask = Task.Factory.StartNew(() =>
            {
                byte[] bytes;

                if (!IORequests.IsIORequstEmpty())
                do                {
                    bytes = null;
                } while (bytes == null);

                return bytes;
            }, /*token,*/ TaskCreationOptions.LongRunning);


            if (!IORequests.IsEmpty())
            {
                Console.WriteLine("Request processing");
                var req = IORequests.GetRequest();

                // 0 hardkodovano dok ne vidim sta tu 
                protHandler.PackData(0, req.sendBuff);

            }
            else
            {
                Console.WriteLine("There is no requests for processing");
            }

            await Task.Delay(timerMsc, token);

        }

    }
}
