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
using SCADA.ConfigurationParser;

namespace SCADA.CommunicationAndControlling
{
    // Logic for communication with Process Controller - ProcessControllerCommunicationEngine
    public class PCCommunicationEngine
    {
        IORequestsQueue IORequests;
        bool isShutdown;
        int timerMsc;

        private Dictionary<string, ProcessController> processControllers { get; set; }
        private Dictionary<string, TcpClient> TcpChannels { get; set; }


        public PCCommunicationEngine()
        {
            IORequests = IORequestsQueue.GetQueue();

            isShutdown = false;
            timerMsc = 200;

            processControllers = new Dictionary<string, ProcessController>();
            TcpChannels = new Dictionary<string, TcpClient>();
        }


        #region Establishing communication 

        /// <summary>
        /// Reading communication paremeters from configPath,
        /// and establishing communication links with Process Controllers
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        public bool Configure(string basePath, string configPath)
        {
            bool retVal = false;
            CommunicationModelParser parser = new CommunicationModelParser(basePath);

            if (parser.DeserializeCommunicationModel())
            {
                processControllers = parser.GetProcessControllers();
                retVal = CreateChannels();
            }

            return retVal;
        }

        /// <summary>
        /// Creating concrete communication channel for each Process Controller
        /// </summary>
        /// <returns></returns>
        private bool CreateChannels()
        {
            List<ProcessController> failedProcessControllers = new List<ProcessController>();

            foreach (var rtu in processControllers)
            {
                if (!EstablishCommunication(rtu.Value))
                {
                    Console.WriteLine("\nEstablishing communication with RTU - {0} failed.", rtu.Value.Name);
                    failedProcessControllers.Add(rtu.Value);
                }
                Console.WriteLine("\nSuccessfully established communication with RTU - {0}.", rtu.Value.Name);
            }

            foreach (var failedProcessController in failedProcessControllers)
            {
                processControllers.Remove(failedProcessController.Name);
            }
            failedProcessControllers.Clear();

            // if there is no any controller, do not start Processing
            if (processControllers.Count == 0)
                return false;

            return true;
        }

        private bool EstablishCommunication(ProcessController rtu)
        {
            bool retval = false;

            try
            {
                TcpClient tcpClient = new TcpClient();

                // connecting to slave
                tcpClient.Connect(rtu.HostName, rtu.HostPort);

                TcpChannels.Add(rtu.Name, tcpClient);

                retval = true;
            }
            catch (SocketException e)
            {
                // ako MdbSim nije podignut to dobijes -no connection can  be made because target machine activelly refused it
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

            return retval;
        }

        #endregion

        /// <summary>
        /// Getting IORB requests from IORequests queue, sending it to Simulator;
        /// Receiving Simulator answers, enqueing it to IOAnswers queue.
        /// </summary>
        public void ProcessRequestsFromQueue()
        {
            while (!isShutdown)
            {

                bool isSuccessful;
                IORequestBlock forProcess = IORequests.DequeueRequest(out isSuccessful);

                if (isSuccessful)
                {

                    // Console.WriteLine("** ProcessRequests(){0}, REQUEST = ", processing, BitConverter.ToString(forProcess.SendBuff, 0, forProcess.SendMsgLength));

                    TcpClient client;

                    if (TcpChannels.TryGetValue(forProcess.ProcessControllerName, out client))
                    {

                        try
                        {
                            // to do: test this case...connection lasts forever? 
                            if (!client.Connected)
                            {
                                processControllers.TryGetValue(forProcess.ProcessControllerName, out ProcessController rtu);
                                client.Connect(rtu.HostName, rtu.HostPort);
                            }

                            NetworkStream stream = client.GetStream();
                            int offset = 0;

                            stream.Write(forProcess.SendBuff, offset, forProcess.SendMsgLength);

                            // to do: processing big messages.  whole, or in parts?
                            // ...

                            forProcess.RcvBuff = new byte[client.ReceiveBufferSize];

                            var length = stream.Read(forProcess.RcvBuff, offset, client.ReceiveBufferSize);
                            forProcess.RcvMsgLength = length;

                            //Console.WriteLine("*** ANSWER <READ> = ", BitConverter.ToString(forProcess.RcvBuff, 0, forProcess.RcvMsgLength));

                            IORequests.EnqueueAnswer(forProcess);
                            //Console.WriteLine("**** ProcessRequests(){0}, Answer enqueued IOAnswers.Count = {1}", processing, IORequests.IOAnswers.Count);

                        }
                        catch (Exception e)
                        {
                            // to do: handle this...


                            Console.WriteLine(e.Message);

                            // kanal sa kontrolerom je zatvoren
                            //if (client.Connected)
                            //  client.Close();

                            // TcpChannels.Remove(toProcess.RtuName);
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nThere is no communication link with {0} rtu. Request is disposed.", forProcess.ProcessControllerName);
                    }
                }

                Thread.Sleep(millisecondsTimeout: timerMsc);
            }

            foreach (var channel in TcpChannels.Values)
            {
                // !!! change this. not valid calling all methods
                channel.GetStream().Close();
                channel.Close();
                channel.Client.Disconnect(true);
            }

            TcpChannels.Clear();
        }

        // dodati close kanala i ostalo...
        public void Stop()
        {
            isShutdown = true;
        }
    }
}
