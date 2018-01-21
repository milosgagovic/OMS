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
        IORequestsQueue IORequests;
        bool shutdown;
        int timerMsc;

        private Dictionary<string, ProcessController> processControllers { get; set; }

        private Dictionary<string, TcpClient> TcpChannels { get; set; }


        public PCCommunicationEngine()
        {
            IORequests = IORequestsQueue.GetQueue();

            shutdown = false;
            timerMsc = 1000;

            processControllers = new Dictionary<string, ProcessController>();

            TcpChannels = new Dictionary<string, TcpClient>();
        }

        // Komunikacioni sloj treba da konfigurise samo deo vezan bas za komunikaciju
        public bool Configure(string configPath)
        {

            ProcessController rtu1 = new ProcessController()
            {
                DeviceAddress = 1,
                HostName = "localhost",
                HostPort = 4021,
                Name = "RTU-1"
            };

            ProcessController rtu2 = new ProcessController()
            {
                DeviceAddress = 2,
                HostName = "localhost",
                HostPort = 4022,
                Name = "RTU-2",
            };

            ProcessController rtu3 = new ProcessController()
            {
                DeviceAddress = 3,
                HostName = "localhost",
                HostPort = 502,
                Name = "RTU-3",
            };

            processControllers.Add(rtu1.Name, rtu1);
            processControllers.Add(rtu2.Name, rtu2);
            processControllers.Add(rtu3.Name, rtu3);

            return CreateChannels();
        }

        bool CreateChannels()
        {
            List<ProcessController> failedRtus = new List<ProcessController>();

            // za svaki RTU pravimo kokretan komunikacioni link
            foreach (var rtu in processControllers)
            {
                if (!EstablishCommunication(rtu.Value))
                {
                    Console.WriteLine("\nEstablishing communication with RTU - {0} failed.", rtu.Value.Name);
                    failedRtus.Add(rtu.Value);
                }
                Console.WriteLine("\nSuccessfully established communication with RTU - {0}.", rtu.Value.Name);
            }

            foreach (var failedRTU in failedRtus)
            {
                processControllers.Remove(failedRTU.Name);
            }
            failedRtus.Clear();

            // ako nije povezano sa kontrolerima 
            // ni ne pocinjati StartProcessing
            if (processControllers.Count == 0)
                return false;

            return true;
        }

        private bool EstablishCommunication(ProcessController rtu)
        {
            bool retval = false;

            try
            {
                TcpClient tcpClient = new TcpClient() { SendTimeout = 1000, ReceiveTimeout = 1000 };

                // connecting to slave
                tcpClient.Connect(rtu.HostName, rtu.HostPort);

                TcpChannels.Add(rtu.Name, tcpClient);

                retval = true;
            }
            catch (SocketException e)
            {
                // no connection can  be made because target machine activelly refused it
                // ako MdbSim nije podignut to dobijes
                Console.WriteLine("ErrorCode = {0}", e.ErrorCode);
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
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
                //Console.WriteLine("StartProcessing");

                // treba mi concurent queueu a ne blocking collection, bas
                // ovako sa isSuccessfull
                // da ne blokiram ceo pccommunicationengine ako nema zahteva
                // da ogu da se obraduju odgovori od modbusa
                bool isSuccessful;
                IORequestBlock toProcess = IORequests.GetRequest(out isSuccessful);
                if (isSuccessful)
                {
                    TcpClient client;

                    if (TcpChannels.TryGetValue(toProcess.RtuName, out client))
                    {
                        NetworkStream stream = client.GetStream();
                        int offset = 0;

                        stream.Write(toProcess.SendBuff, offset, toProcess.SendMsgLength);


                        // kasnije razmisliti da li poruke dolaze cele, ili u
                        // delovima ako su velike...da li je potrebno u petlji
                        // citati

                        toProcess.RcvBuff = new byte[client.ReceiveBufferSize];

                        var length = stream.Read(toProcess.RcvBuff, offset, client.ReceiveBufferSize);
                        toProcess.RcvMsgLength = length;

                        IORequests.EnqueueIOAnswerForProcess(toProcess);
                    }
                    else
                    {
                        Console.WriteLine("\nThere is no communication link with {0} rtu. Request will be disposed.", toProcess.RtuName);
                    }
                }
                Thread.Sleep(100);
            }
            {
                foreach (var channel in TcpChannels.Values)
                {
                    // ne treba sve
                    channel.GetStream().Close();
                    channel.Close();
                    channel.Client.Disconnect(true);
                }
                TcpChannels.Clear();

            }
        }

        public void AddIOReqForProcess(IORequestBlock iorb)
        {
            IORequests.EnqueueIOReqForProcess(iorb);
        }

    }
}
