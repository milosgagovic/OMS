using ModbusTCPDriver;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PCCommon;

namespace SCADA.CommAcqEngine
{
    // Acquisition engine
    public class ACQEngine
    {
        IORequestsQueue IORequests;
        bool shutdown;
        int timerMsc;
        RTU rtu1;

        public ACQEngine()
        {
            IORequests = IORequestsQueue.GetQueue();
            shutdown = false;
            timerMsc = 1000;
        }

        // ovo sam za test krenula da pravim
        public void Setup()
        {
            // promenicu ovo, necu koristiti ove kao Channele - verovatno mi nece ipak trebati,
            // nego ce RTU imati referencu na otvoren komunikacioni kanal u kontekstu .NET-a
            // ili ce channel struktura omogucivati da se otvore kanalu iz .neta, videcu

            Channel TCPChannel = new TCPClientChannel();
            TCPChannel.Protocol = IndustryProtocols.Modbus;
           

            rtu1 = new RTU(8, 8, 4, 4, 2);
            rtu1.HostName = "localhost";
            rtu1.HostPort = 4021;
            rtu1.RTUAddress = 21;
            rtu1.Name = "RTU-1";

            // ovde ide akvizicionid deo

            SendReadCoilRequest();

        }

        public void StartAcquisition()
        {
            while (!shutdown)
            {
                // zapravo treba da ovaj toProcess IORB sadrzi RTU koji gadja, ali to dolazi iz RTDB baze i tako to...

                IORequestBlock toProcess = new IORequestBlock();

                toProcess.Rtu = rtu1;             
                IORequests.EnqueueIOReqForProcess(toProcess);

                Console.WriteLine("Request added to processing buffer.");

                // ne koritisti thread.sleep -> menjam to sve...
                Thread.Sleep(timerMsc);
            }
        }


        public void SendReadCoilRequest()
        {

        }

    }
}
