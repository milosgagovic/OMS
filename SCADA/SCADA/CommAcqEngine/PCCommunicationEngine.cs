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
       // IIndustryProtocolHandler protHandler = new ModbusHandler();

        IORequestsQueue IORequests;
        bool shutdown;
        int timerMsc;
      
        private static Dictionary<string, Channel> channels { get; set; }  // ovo mi zapravo ni ne treba za sada
        private Dictionary<string, ProcessController> processControllers { get; set; }

        private Dictionary<string, TcpClient> TcpChannels { get; set; }


        public PCCommunicationEngine()
        {
           // protHandler = new ModbusHandler(); 

            IORequests = IORequestsQueue.GetQueue();

            shutdown = false;
            timerMsc = 1000;

            channels = new Dictionary<string, Channel>();
            processControllers = new Dictionary<string, ProcessController>();

            TcpChannels = new Dictionary<string, TcpClient>();
        }



        // citanje iz fajlova ovde da bude...
        public void Configure()
        {
            // videti kako da iskoristis Channel
            Channel chan1 = new Channel()
            {
                Protocol = IndustryProtocols.Modbus,
                TimeOutMsc = 10, // za sada nam ovo nece trebati
                Name = "CHAN-1",
                Info = "Acquistion Channel 1"
            };

            Channel chan2 = new Channel()
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
            //processControllers.Add(rtu3.Name, rtu3);

            CreateChannels();
        }

        void CreateChannels()
        {

            // za svaki RTU pravimo kokretan komunikacioni link
            foreach (var rtu in processControllers)
            {
                if (!EstablishCommunication(rtu.Value))
                {
                    // ako nije uspelo pobrisi sta ne treba i tako to

                    // ako nije povezano sa kontrolerima - nisu podignuti onda vratiti neki error ovde i ni ne pocinjati StartProcessing

                }
            }
        }

        // ovo je izdvojeno da bude zasebna metoda, jer se tu nazire potencijalna upotrebna Channel.cs...
        // Ondno mozemo specificiarati time-out u channelu
        // jer ovo TcpClient() radi sinhrono povezivanje sa serverom, pa se ceka dugo exception ako ne valja server
        // https://stackoverflow.com/questions/17118632/how-to-set-the-timeout-for-a-tcpclient 

        // jedan kanal - kada mu utvrdim mesto, moze biti pridruzen vecem broju RTU-ova. (To je npr. kao BaseVoltage sto je pridruzen vecem broju opreme u CIMu)
        // taj kanal odredjuje prirodu komunikacije


        private bool EstablishCommunication(ProcessController rtu)
        {
            bool retval = false;
           
            try
            {
                TcpClient tcpClient = new TcpClient() { SendTimeout = 1000, ReceiveTimeout = 1000 };

                // connecting to slave
                tcpClient.Connect(rtu.HostName, rtu.HostPort);

                TcpChannels.Add(rtu.Name, tcpClient);
            }
            catch (SocketException e)
            {
                // no connection can  be made because target machine activelly refused it
                // ako MdbSim nije podignut to dobijes
                Console.WriteLine(e);
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }
            finally
            {

            }

           

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
                Console.WriteLine("StartProcessing");
                IORequestBlock toProcess = IORequests.GetRequest();

                TcpClient client=new TcpClient();
                if (TcpChannels.TryGetValue(toProcess.RtuName, out client))
                {
                    NetworkStream stream = client.GetStream();
                    int offset = 0;

                    stream.Write(toProcess.SendBuff, offset, toProcess.SendMsgLength);


                    // kasnije razmisliti da li poruke dolaze cele, ili u
                    // delovima ako su velike?...da li je potrebno u petlji
                    // citati

                    toProcess.RcvBuff = new byte[client.ReceiveBufferSize];

                    stream.Read(toProcess.RcvBuff, offset, 512);

                    // nisam proverila da li ovo radi dobro citanje pisanje, jer nisam uspela konkretan zahtev da dobijem
                    // sad treba odmah value upisivati nazad u iorb

                }
                else
                {
                    
                }

                Thread.Sleep(1000);
            }
            {
                // close svega i dispose
                //tcpClient.GetStream().Close();
                //tcpClient.Close();
                //tcpClient.Client.Disconnect(true);
            }
        }

        public void AddIOReqForProcess(IORequestBlock iorb)
        {
            IORequests.EnqueueIOReqForProcess(iorb);

        }

    }
}
