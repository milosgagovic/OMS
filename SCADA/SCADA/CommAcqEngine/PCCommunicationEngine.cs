using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ModbusTCPDriver;
using PCCommon;
using System.Net.Sockets;

namespace SCADA.CommAcqEngine
{
    // Logic for communication with Process Controller
    public class PCCommunicationEngine
    {
        // rukovaoc konkretnog industrijskog protokola
        private static IIndustryProtocolHandler protHandler;
        private static IORequestsQueue IORequests;
        private bool shutdown;
        private int timerMsc;

        // ovo mi zapravo ni ne treba za sada
        private static Dictionary<string, IChannel> channels { get; set; }
        private Dictionary<string, ProcessController> processControllers { get; set; }

        private Dictionary<string, TcpClient> TcpChannels { get; set; }

        public PCCommunicationEngine()
        {
            protHandler = new ModbusHandler(); //? mozda ne ovde, nego kad se prot type vidi

            IORequests = IORequestsQueue.GetQueue();

            shutdown = false;
            timerMsc = 1000;

            channels = new Dictionary<string, IChannel>();
            processControllers = new Dictionary<string, ProcessController>();

            TcpChannels = new Dictionary<string, TcpClient>();
        }

        // citanje iz fajlova ovde...
        public void Configure()
        {
            // videti kako da iskoristis IChnannel ili Channel
            TCPClientChannel chan1 = new TCPClientChannel()
            {
                Protocol = IndustryProtocols.Modbus,
                TimeOutMsc = 10, // za sada nam ovo nece trebati
                Name = "CHAN-1",
                Info = "Acquistion Channel 1"
            };

            TCPClientChannel chan2 = new TCPClientChannel()
            {
                Protocol = IndustryProtocols.Modbus,
                TimeOutMsc = 10000,
                Name = "CHAN-2",
                Info = "Acquistion Channel 2"
            };

            channels.Add(chan1.Name, chan1);
            channels.Add(chan2.Name, chan2);

            ProcessController rtu1 = new ProcessController()
            {
                HostName = "localhost",
                HostPort = 4021,
                Name = "RTU-1",
                ChannelName = "CHAN-1",

            };

            ProcessController rtu2 = new ProcessController()
            {
                HostName = "localhost",
                HostPort = 4022,
                Name = "RTU-2",
                ChannelName = "CHAN-1",

            };

            ProcessController rtu3 = new ProcessController()
            {
                HostName = "localhost",
                HostPort = 502,
                Name = "RTU-3",
                ChannelName = "CHAN-2",

            };

            processControllers.Add(rtu1.Name, rtu1);
            processControllers.Add(rtu2.Name, rtu2);
            processControllers.Add(rtu3.Name, rtu3);

            CreateChannels();
        }

        void CreateChannels()
        {
            // citamo sve kanale

            // otvaramo komunikacione linkove 


            // za svaki RTU pravimo komunikacioni link
            foreach (var rtu in processControllers)
            {
                if (!EstablishCommunication(rtu.Value)) 
                {
                    // ako nije uspelo pobrisi sta ne treba i tako to

                }
            }
        }

        // ovo je izdvojeno da bude zasebna metoda, jer cemo mozda vrsiti povezivanje sa rtu-om sa specificiarnim time-out-om u channelu
        // jer ovo sinhrono povezivanje se ceka dugo excewption ako ne valja
        // // https://stackoverflow.com/questions/17118632/how-to-set-the-timeout-for-a-tcpclient 
       
       // testirati to koliko se dugo ceka za exception

        // jedan kanal moze biti pridruzen vecem broju RTU-ova. kao BaseVoltage sto je pridruzen vecem broju opreme
        // taj kanal odredjuje prirodu komunikacije

       
        private bool EstablishCommunication(ProcessController rtu)
        {
            bool retval = false;
            TcpClient tcpClient = new TcpClient(rtu.HostName, rtu.HostPort) // connecting to slave
            {
                SendTimeout = 1000,
                ReceiveTimeout = 1000
            };

            TcpChannels.Add(rtu.Name, tcpClient); 


            
            //IChannel ch;
            //if (channels.TryGetValue(rtu.ChannelName, out ch))
            //{
            //   

            //    TcpClient tcpClient;
            //    if (!tcpClient.ConnectAsync(rtu.HostName, rtu.HostPort).Wait(TimeSpan.FromMilliseconds(ch.TimeOutMsc)))
            //    {
            //        // timed out
            //        tcpClient.Close();
            //        throw new ApplicationException("Failed to connect to slave device.");
            //    }
            //    else
            //    {

            //    }
            //}
            //else
            //{
            //    // invalid config, clean up
            //}

            return retval;
        }

        public void StartProcessing()
        {
            while (!shutdown)
            {
                IORequestBlock toProcess = IORequests.GetRequest();
                // toProcess.ChannelId; // ne treba mi zapravo

                if (TcpChannels.TryGetValue(toProcess.ProcessControllerAddress, out TcpClient client))
                {
                    NetworkStream stream = client.GetStream();
                    int offset = 0;
                   
                    stream.Write(toProcess.SendBuff, offset, toProcess.SendMsgLength);


                    // kasnije razmisliti da li poruke dolaze cele, ili u
                    // delovima ako su velike?...da li je potrebno u petlji
                    // citati

                    toProcess.RcvBuff = new byte[client.ReceiveBufferSize];

                    stream.Read(toProcess.RcvBuff, offset, 512);                   
                    
                }
                else
                {

                }
            }
            {
                // close svega i dispose
                //tcpClient.GetStream().Close();
                //tcpClient.Close();
                //tcpClient.Client.Disconnect(true);
            }
        }




        // ovde treba srediti komunikaciju sa svim ucitanim-konfigurisanim RTU-ovima i kanalima
        public void StartCommunication()
        {

        }

        public void AddIOReqForProcess(IORequestBlock iorb)
        {
            IORequests.EnqueueIOReqForProcess(iorb);

        }


    }
}
