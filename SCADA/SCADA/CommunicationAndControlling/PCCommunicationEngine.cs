using System;
using System.Collections.Generic;
using System.Threading;
using PCCommon;
using System.Net.Sockets;
using SCADA.ConfigurationParser;
using SCADA.CommunicationAndControlling.Communication;

namespace SCADA.CommunicationAndControlling
{
    // Logic for communication with Process Controller - ProcessControllerCommunicationEngine
    public class PCCommunicationEngine
    {
        IORequestsQueue IORequests;
        int timerMsc;
        private Dictionary<string, ProcessController> processControllers { get; set; }

        public PCCommunicationEngine()
        {
            IORequests = IORequestsQueue.GetQueue();
            // timerMsc = 100;

            processControllers = new Dictionary<string, ProcessController>();
        }

        #region Establishing communication 

        /// <summary>
        /// Reading communication parameters from configPath,
        /// and establishing communication links with Process Controllers
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        public bool ConfigureEngine(string basePath, string configPath)
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
                if (!SetupCommunication(rtu.Value))
                {
                    Console.WriteLine("\nEstablishing communication with RTU - {0} failed.", rtu.Value.Name);
                    failedProcessControllers.Add(rtu.Value);
                }
                else
                    Console.WriteLine("\nSuccessfully established communication with RTU - {0}.", rtu.Value.Name);
            }

            foreach (var failedProcessController in failedProcessControllers)
            {
                processControllers.Remove(failedProcessController.Name);
            }

            if (failedProcessControllers.Count != 0)
                failedProcessControllers.Clear();

            // if there is no any controller, do not start Processing
            if (processControllers.Count == 0)
                return false;

            return true;
        }

        private bool SetupCommunication(ProcessController rtu)
        {
            bool retval = false;


            switch (rtu.TransportHandler)
            {
                case TransportHandler.TCP:

                    // na osnovu ovoga on sad zna u factoryju da pravi sta treba konkretno za Tcp...
                    CommunicationManager.CurrentTransportHndl = TransportHandler.TCP;

                    CommunicationParameters commPar = new CommunicationParameters(rtu.HostName, rtu.HostPort);
                    var commObj = CommunicationManager.Factory.CreateNew(commPar);

                    if (commObj.Setup())
                    {
                        CommunicationManager.CommunicationObjects.TryAdd(rtu.Name, commObj);
                        retval = true;
                    }

                    break;

                default:
                    // not implemented yet
                    break;
            }
            return retval;
        }

        #endregion

        /// <summary>
        /// Getting IORB requests from IORequests queue, sending it to Simulator;
        /// Receiving Simulator answers, enqueing it to IOAnswers queue.
        /// </summary>
        public void ProcessRequestsFromQueue(TimeSpan timeout, CancellationToken token)
        {

            Console.WriteLine("Process Request form queue thread id={0}", Thread.CurrentThread.ManagedThreadId);
            while (!token.IsCancellationRequested)
            {
                bool isSuccessful;
                IORequestBlock forProcess = IORequests.DequeueRequest(out isSuccessful, timeout);

                if (isSuccessful)
                {
                    // communication object.try get value
                    CommunicationObject commObj;
                    if (CommunicationManager.CommunicationObjects.TryGetValue(forProcess.ProcessControllerName, out commObj))
                    {
                        // to dooo taskove i asinhrono
                        //to do...nparaviti da su ovo taskovi nekad u buducnosti
                        if (commObj.ProcessRequest(forProcess))
                        {

                        }
                        else
                        {
                            
                        }

                    }

                    //TcpClient client;
                    //if (TcpChannels.TryGetValue(forProcess.ProcessControllerName, out client))
                    //{
                    //    try
                    //    {
                    //        // to do: test this case...connection lasts forever? 
                    //        if (!client.Connected)
                    //        {
                    //            processControllers.TryGetValue(forProcess.ProcessControllerName, out ProcessController rtu);
                    //            client.Connect(rtu.HostName, rtu.HostPort);
                    //        }

                    //        NetworkStream stream = client.GetStream();
                    //        int offset = 0;

                    //        stream.Write(forProcess.SendBuff, offset, forProcess.SendMsgLength);

                    //        // to do: processing big messages.  whole, or in parts?
                    //        // ...
                    //        // ovde dodati taski koji receieve radi

                    //        forProcess.RcvBuff = new byte[client.ReceiveBufferSize];

                    //        var length = stream.Read(forProcess.RcvBuff, offset, client.ReceiveBufferSize);
                    //        forProcess.RcvMsgLength = length;

                    //        IORequests.EnqueueAnswer(forProcess);
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        // to do: handle this...
                    //        Console.WriteLine(e.Message);
                    //        //if (client.Connected)
                    //        //  client.Close();

                    //        // TcpChannels.Remove(toProcess.RtuName);
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine("\nThere is no communication link with {0} rtu. Request is disposed.", forProcess.ProcessControllerName);
                    //}

                }
            }
        }

        // to do...cancelation token i communicaition maanbger da brise sve bla bla
        public void Stop()
        {
            // clear, dispose...
        }
    }
}
