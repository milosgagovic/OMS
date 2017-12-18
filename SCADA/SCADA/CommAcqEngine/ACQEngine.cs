using ModbusTCPDriver;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PCCommon;
using SCADA.RealtimeDatabase;

namespace SCADA.CommAcqEngine
{
    // Acquisition engine
    public class ACQEngine
    {
        private static IIndustryProtocolHandler protHandler;
        private static IORequestsQueue IORequests;

        bool shutdown;
        int timerMsc;
        
        RTU rtu1;
        private DBContext db = null;

        public static Dictionary<string, RTU> RTUs { get; set; }

        public ACQEngine()
        {
            IORequests = IORequestsQueue.GetQueue();
            shutdown = false;
            timerMsc = 1000;

            RTUs = new Dictionary<string, RTU>();
            db = new DBContext();
            //choosenProtocol = IndustryProtocols.Modbus;
        }

        // ovo sam za test krenula da pravim
        public void SetupRTUs()
        {
        }

        // Miljana setup samo 1 RTU podesi! pa pozovi metodu na handleru
        public void Setup()
        {
            // RTU imati referencu na otvoren komunikacioni kanal u kontekstu .NET-a
            // ili ce channel struktura omogucivati da se otvore kanalu iz .neta, videcu

            TCPClientChannel TCPChannel = new TCPClientChannel();
            TCPChannel.Protocol = IndustryProtocols.Modbus;


            rtu1 = new RTU(8, 8, 4, 4, 2);
            rtu1.HostName = "localhost";
            rtu1.HostPort = 4021;
            rtu1.RTUAddress = 21;
            rtu1.Name = "RTU-1";

            // ovde ide akvizicionid deo

            RTUs.Add(rtu1.Name, rtu1);

            SendReadCoilRequest();

        }

        public void StartAcquisition()
        {
            while (!shutdown)
            {
                if (RTUs.Count > 0)
                {
                    foreach (RTU rtu in RTUs.Values)
                    {
                        IORequestBlock iorb = new IORequestBlock()
                        {
                            RequestType = RequestType.SEND_RECV,
                            ChannelId = rtu.ChannelId,
                            ProcessControllerAddress = rtu.RTUAddress.ToString()
                        };

                             
                        if (!ProtocolSetter(rtu.Channel.Protocol))
                        {                           
                            continue;
                        }

                        //iorb.SendBuff = protHandler.PackData();

                        IORequests.EnqueueIOReqForProcess(iorb);
                    }
                }

                Thread.Sleep(millisecondsTimeout: timerMsc);
            }
        }


        public void SendReadCoilRequest()
        {

        }


        private static bool ProtocolSetter(IndustryProtocols protocol)
        {
            switch (protocol)
            {
                case IndustryProtocols.Modbus:
                    protHandler = new ModbusHandler();
                    return true;
            }

            return false;
        }

        public void FormRequestForReadCommand(string rtuId, int address, int value)
        {
            IORequestBlock iorb = new IORequestBlock()
            {
                RequestType = RequestType.SEND_RECV
            };

            CommonRequestPart(iorb, rtuId, address);
        }

        public void FormRequestForWriteCommand(string rtuId, int address, int value)
        {
            IORequestBlock iorb = new IORequestBlock()
            {
                RequestType = RequestType.SEND,
            };

            CommonRequestPart(iorb, rtuId, address);
        }

        private static void CommonRequestPart(IORequestBlock iorb, string rtuId, int address)
        {
            RTUs.TryGetValue(rtuId, out RTU rtu);

            iorb.ChannelId = rtu.ChannelId;
            iorb.ProcessControllerAddress = rtu.RTUAddress.ToString();

            //if (!ProtocolSetter(rtu.Channel.Protocol))
            //{
            //    return;
            //}

            IORequests.EnqueueIOReqForProcess(iorb);
        }

    }
}
