using ModbusTCPDriver;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PCCommon;
using SCADA.RealtimeDatabase;
using OMSSCADACommon;
using OMSSCADACommon.Responses;
using SCADA.ClientHandler;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.ConfigurationParser;
using System.Linq;

namespace SCADA.CommunicationAndControlling.SecondaryDataProcessing
{
    // Commanding-Acquisition Engine
    public class CommAcqEngine : ICommandReceiver
    {
        private static IORequestsQueue IORequests;
        private static bool isShutdown;
        private int timerMsc;


        private DBContext dbContext = null;

        public CommAcqEngine()
        {
            Console.WriteLine("AcqEngine Instancing()");

            IORequests = IORequestsQueue.GetQueue();
            dbContext = new DBContext();
            isShutdown = false;
            timerMsc = 5000;
        }
        /// <summary>
        /// Reading database data from configPath,
        /// configuring RTUs and Process Variables
        /// </summary>
        /// <param name="configPath"></param>
        public bool Configure(string configPath)
        {
            ScadaModelParser parser = new ScadaModelParser();
            return parser.DeserializeScadaModel();
        }

        /// <summary>
        /// Send Commands to simulator, to make its state consistent with RTDB
        /// </summary>
        public void InitializeSimulator()
        {
            List<ProcessVariable> pvs = dbContext.GetProcessVariable().ToList();
            if (pvs.Count != 0)
            {
                foreach (ProcessVariable pv in pvs)
                {
                    IORequestBlock iorb = new IORequestBlock()
                    {
                        RequestType = RequestType.SEND,
                        ProcessControllerName = pv.ProcContrName
                    };

                    RTU rtu;
                    if ((rtu = dbContext.GetRTUByName(pv.ProcContrName)) != null)
                    {
                        iorb.ReqAddress = (ushort)rtu.GetCommandAddress(pv);
                        bool shouldCommand = false;

                        switch (rtu.Protocol)
                        {
                            case IndustryProtocols.ModbusTCP:

                                ModbusHandler mdbHandler = new ModbusHandler();
                                mdbHandler.Header = new ModbusApplicationHeader()
                                {
                                    TransactionId = 0,
                                    Length = 5,
                                    ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
                                    DeviceAddress = rtu.Address
                                };

                                mdbHandler.Request = new WriteRequest() { StartAddr = (ushort)rtu.GetCommandAddress(pv) };

                                switch (pv.Type)
                                {
                                    // initialy, on simulator all digitals are set to 0 -> closed state
                                    case VariableTypes.DIGITAL:
                                        Digital digital = (Digital)pv;

                                        CommandTypes comm;
                                        if (shouldCommand = CommandValidator.InitialCommandingForDigital(digital, out comm))
                                        {
                                            mdbHandler.Request.FunCode = FunctionCodes.WriteSingleCoil;
                                            ((WriteRequest)mdbHandler.Request).Value = (ushort)comm;
                                        }
                                        break;

                                    case VariableTypes.ANALOG:
                                        Analog analog = (Analog)pv;

                                        if (shouldCommand = AnalogProcessor.InitialWorkPointAnalog(analog))
                                        {
                                            mdbHandler.Request.FunCode = FunctionCodes.WriteSingleRegister;
                                            ((WriteRequest)mdbHandler.Request).Value = (ushort)analog.RawCommValue;
                                        }
                                        analog.IsInit = true;
                                        break;

                                    case VariableTypes.COUNTER:
                                        Counter counter = (Counter)pv;

                                        break;
                                }

                                if (shouldCommand)
                                {
                                    iorb.SendBuff = mdbHandler.PackData();
                                    iorb.SendMsgLength = iorb.SendBuff.Length;
                                    if (pv.Name == "MEAS_AN_1")
                                        Console.WriteLine(BitConverter.ToString(iorb.SendBuff, 0, 12));
                                }

                                break;
                        }

                        if (shouldCommand)
                        {
                            IORequests.EnqueueRequest(iorb);
                        }
                    }
                    else
                    {
                        // izbrisati omdah te procesne varijable sa rtu-om tog imena?
                        Console.WriteLine("Invalid config: ProcContrName = {0} does not exists.", pv.ProcContrName);
                        continue;
                    }

                }
            }
        }

        /// <summary>
        /// Producing IORB requests for automatic data acquistion
        /// </summary>
        public void StartAcquisition()
        {
            DBContext.OnAnalogAdded += OnAnalogAddedEvent;

            List<ProcessVariable> pvs;

            // toArray -> snapshot. ni ne mora to, mozda je pametnije bez toga, kopiranje kosta.
            // ovde  nije skupo jer nema puno rtuova
            var rtusSnapshot = dbContext.GettAllRTUs().ToArray();
            List<Thread> acqThreads = new List<Thread>();
            foreach (var rtu in rtusSnapshot)
            {
                Thread t = new Thread(() => RtuAcquisition(rtu));
                acqThreads.Add(t);
            }
            foreach (var t in acqThreads)
                t.Start();

            foreach (var t in acqThreads)
                t.Join();
            //while (!isShutdown)
            //{
            //    // sporno to do:
            //    //while (!Database.IsConfigurationRunning)
            //    //    Thread.Sleep(100);

            //    pvs = dbContext.GetProcessVariable().ToList();
            //    foreach (ProcessVariable pv in pvs)
            //    {
            //        IORequestBlock iorb = new IORequestBlock()
            //        {
            //            RequestType = RequestType.SEND_RECV,
            //            ProcessControllerName = pv.ProcContrName
            //        };

            //        RTU rtu;
            //        if ((rtu = dbContext.GetRTUByName(pv.ProcContrName)) != null)
            //        {
            //            iorb.ReqAddress = (ushort)rtu.GetAcqAddress(pv);

            //            switch (rtu.Protocol)
            //            {
            //                case IndustryProtocols.ModbusTCP:

            //                    ModbusHandler mdbHandler = new ModbusHandler();

            //                    // header is same for all read - acquistion requests
            //                    mdbHandler.Header = new ModbusApplicationHeader()
            //                    {
            //                        TransactionId = 0,
            //                        Length = 5,
            //                        ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
            //                        DeviceAddress = rtu.Address
            //                    };

            //                    mdbHandler.Request = new ReadRequest() { StartAddr = iorb.ReqAddress };

            //                    switch (pv.Type)
            //                    {
            //                        case VariableTypes.DIGITAL:

            //                            Digital digital = (Digital)pv;

            //                            mdbHandler.Request.FunCode = FunctionCodes.ReadDiscreteInput;
            //                            ((ReadRequest)mdbHandler.Request).Quantity = (ushort)(Math.Floor((Math.Log(digital.ValidStates.Count, 2))));

            //                            break;

            //                        case VariableTypes.ANALOG:
            //                            Analog analog = (Analog)pv;
            //                            if (!analog.IsInit)
            //                                continue; // dok se ne setuje inicijalna vrednost

            //                            mdbHandler.Request.FunCode = FunctionCodes.ReadInputRegisters;
            //                            ((ReadRequest)mdbHandler.Request).Quantity = analog.NumOfRegisters;

            //                            break;

            //                        case VariableTypes.COUNTER:
            //                            Counter counter = (Counter)pv;

            //                            break;
            //                    }

            //                    iorb.SendBuff = mdbHandler.PackData();
            //                    break;
            //            }

            //            iorb.SendMsgLength = iorb.SendBuff.Length;
            //            //if (pv.Name == "MEAS_AN_1")
            //            //  Console.WriteLine(BitConverter.ToString(iorb.SendBuff, 0, 12));
            //            IORequests.EnqueueRequest(iorb);
            //        }
            //        else
            //        {
            //            // ne postoji taj rtu sa tim imenom. izbrisati te procesne varijable sa rtu-om tog imena
            //            Console.WriteLine("Invalid config: ProcContrName = {0} does not exists.", pv.ProcContrName);
            //            continue;
            //        }
            //    }
            //    Thread.Sleep(millisecondsTimeout: timerMsc);
            //}


            // to do - wait all threads
            Console.WriteLine("StartAcq.shutdown=true");
            return;
        }

        // zaseban thread da bi mogla da kasnije imas acq period na nivou rtu-a, i da ti lakse bude kad budes prebacivala na TPL
        // mozda da ne radis sa pair, nego samo string i onda iz baze uzimas taj rtu...
        public void RtuAcquisition(KeyValuePair<string, RTU> rtuPair)
        {
            IIndustryProtocolHandler IProtHandler = null;
            Console.WriteLine("RtuAcquistion Thread id = {0} started", Thread.CurrentThread.ManagedThreadId);

            while (!isShutdown)
            {
                RTU rtu = dbContext.GetRTUByName(rtuPair.Key);
                if (rtu != null)
                {
                    IORequestBlock iorb = new IORequestBlock()
                    {
                        RequestType = RequestType.SEND_RECV,
                        ProcessControllerName = rtuPair.Key
                    };

                    switch (rtu.Protocol)
                    {
                        case IndustryProtocols.ModbusTCP:
                            IProtHandler = new ModbusHandler()
                            {
                                Header = new ModbusApplicationHeader
                                {
                                    TransactionId = 0,
                                    ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
                                    DeviceAddress = rtu.Address,
                                    Length = 5
                                },
                                Request = new ReadRequest()
                            };
                            break;
                    }

                    //to do. praviti nove iorbove
                    //-------------analogs---------------
                    var analogs = dbContext.GetProcessVariable().Where(pv => pv.Type == VariableTypes.ANALOG && pv.IsInit == true &&
                                                                        pv.ProcContrName.Equals(rtuPair.Key)).OrderBy(pv => pv.RelativeAddress);
                    int requestCount = analogs.ToList().Count();
                    if (requestCount != 0)
                    {
                        ProcessVariable firstPV = analogs.FirstOrDefault();
                        iorb.ReqAddress = (ushort)rtu.GetAcqAddress(firstPV);


                        if (IProtHandler != null)
                        {
                            switch (rtu.Protocol)
                            {
                                case IndustryProtocols.ModbusTCP:

                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).FunCode = FunctionCodes.ReadInputRegisters;
                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).Quantity = (ushort)requestCount;
                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).StartAddr = iorb.ReqAddress;
                                    break;
                            }

                            iorb.flags = requestCount;
                            iorb.SendBuff = IProtHandler.PackData();
                            iorb.SendMsgLength = iorb.SendBuff.Length;
                            IORequests.EnqueueRequest(iorb);
                        }
                    }

                    //-------------digitals---------------(to do: add init flag...)
                    var digitals = dbContext.GetProcessVariable().Where(pv => pv.Type == VariableTypes.DIGITAL &&
                                                                        pv.ProcContrName.Equals(rtuPair.Key)).OrderBy(pv => pv.RelativeAddress);
                    requestCount = digitals.ToList().Count();
                    if (requestCount != 0)
                    {
                        ProcessVariable firstPV = digitals.FirstOrDefault();
                        iorb.ReqAddress = (ushort)rtu.GetAcqAddress(firstPV);

                        if (IProtHandler != null)
                        {
                            switch (rtu.Protocol)
                            {
                                case IndustryProtocols.ModbusTCP:

                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).FunCode = FunctionCodes.ReadDiscreteInput;
                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).Quantity = (ushort)requestCount;
                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).StartAddr = iorb.ReqAddress;
                                    break;
                            }

                            iorb.flags = requestCount;
                            iorb.SendBuff = IProtHandler.PackData();
                            iorb.SendMsgLength = iorb.SendBuff.Length;
                            IORequests.EnqueueRequest(iorb);
                        }
                    }

                    // not implemented yet
                    //-------------counters---------------(to do: add init flag...)
                    var counters = dbContext.GetProcessVariable().Where(pv => pv.Type == VariableTypes.COUNTER &&
                                                                        pv.ProcContrName.Equals(rtuPair.Key)).OrderBy(pv => pv.RelativeAddress);
                    requestCount = counters.ToList().Count();
                    if (requestCount != 0)
                    {
                        ProcessVariable firstPV = counters.FirstOrDefault();
                        iorb.ReqAddress = (ushort)rtu.GetAcqAddress(firstPV);

                        if (IProtHandler != null)
                        {
                            switch (rtu.Protocol)
                            {
                                case IndustryProtocols.ModbusTCP:

                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).FunCode = FunctionCodes.ReadInputRegisters;
                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).Quantity = (ushort)requestCount;
                                    ((ReadRequest)((ModbusHandler)IProtHandler).Request).StartAddr = iorb.ReqAddress;
                                    break;
                            }
                            iorb.flags = requestCount;
                            iorb.SendBuff = IProtHandler.PackData();
                            iorb.SendMsgLength = iorb.SendBuff.Length;
                            IORequests.EnqueueRequest(iorb);
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Returning thread id = {0}", Thread.CurrentThread.ManagedThreadId);
                    return;
                }

                // ovde staviti da spava acqPeriod za taj odredjeni rtu
                Thread.Sleep(timerMsc);
            }
        }

        /// <summary>
        /// Processing answers from Simulator - Process Controller
        /// </summary>
        public void ProcessPCAnwers()
        {
            while (!isShutdown)
            {
                bool isSuccessful;
                IORequestBlock answer = IORequests.DequeueAnswer(out isSuccessful);

                if (isSuccessful)
                {
                    RTU rtu;
                    // sporno
                    //while (!Database.IsConfigurationRunning)
                    //    Thread.Sleep(100);

                    if ((rtu = dbContext.GetRTUByName(answer.ProcessControllerName)) != null)
                    {
                        switch (rtu.Protocol)
                        {
                            case IndustryProtocols.ModbusTCP:

                                ModbusHandler mdbHandler = new ModbusHandler();
                                mdbHandler.UnpackData(answer.RcvBuff, answer.RcvMsgLength);

                                switch (mdbHandler.Response.FunCode)
                                {
                                    case FunctionCodes.ReadDiscreteInput:
                                        {
                                            BitReadResponse response = (BitReadResponse)mdbHandler.Response;
                                            var responsePVCount = answer.flags;
                                            bool[] boolArray = new bool[response.BitValues.Count];
                                            response.BitValues.CopyTo(boolArray, 0);

                                            ProcessVariable pv;
                                            for (int i = 0; i < responsePVCount; i++)
                                            {
                                                Console.WriteLine(boolArray[i]);
                                                if (rtu.GetProcessVariableByAddress(answer.ReqAddress, out pv))
                                                {
                                                    Digital target = (Digital)pv;
                                                   
                                                    try
                                                    {
                                                        //if (target.State != target.ValidStates[array[i]])
                                                        //{
                                                        //    Console.WriteLine("CHANGE!");
                                                        //    target.State = target.ValidStates[array[i]];
                      

                                                        //    ScadaModelParser parser = new ScadaModelParser();
                                                        //    parser.SerializeScadaModel();

                                                        //    DMSClient dMSClient = new DMSClient();
                                                        //    dMSClient.ChangeOnSCADA(target.Name, target.State);
                                                        //}
                                                    }
                                                    catch
                                                    {
                                                        Console.WriteLine("Digital variable {0}, state: INVALID", target.Name);
                                                    }
                                                }
                                            }
                                        }

                                        break;

                                    case FunctionCodes.ReadInputRegisters:
                                        {
                                            RegisterReadResponse response = (RegisterReadResponse)mdbHandler.Response;
                                            ProcessVariable pv;

                                            Analog target = null;

                                            if (rtu.GetProcessVariableByAddress(answer.ReqAddress, out pv))
                                            {
                                                target = (Analog)pv;
                                            }

                                            if (target != null)
                                            {
                                                ushort newRawAcqValue = response.RegValues[0];

                                                try
                                                {
                                                    float newAcqValue;
                                                    AnalogProcessor.RawValueToEGU(target, newRawAcqValue, out newAcqValue);

                                                    // videti kad menjas kommande
                                                    if (target.AcqValue != newAcqValue)
                                                    {
                                                        Console.WriteLine("CHANGE analog!");
                                                        target.RawAcqValue = newRawAcqValue;
                                                        target.AcqValue = newAcqValue;

                                                        ScadaModelParser parser = new ScadaModelParser();
                                                        parser.SerializeScadaModel();

                                                        DMSClient dMSClient = new DMSClient();
                                                        // to do
                                                        // dMSClient.ChangeOnSCADA(target.Name, target.State);
                                                    }
                                                }
                                                catch
                                                {
                                                    // Console.WriteLine("Digital variable {0}, state: INVALID", target.Name);
                                                }
                                            }
                                        }

                                        break;
                                }

                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Answer disposed. Process Controller with name ={0} does not exit.", answer.ProcessControllerName);
                    }
                }

                Thread.Sleep(100);
            }

            Console.WriteLine("ProcessPCAnswers.shutdown=true");
            return;
        }

        // to do: close all communication channels? dispose resources?
        public void Stop()
        {
            isShutdown = true;
            ScadaModelParser parser = new ScadaModelParser();
            parser.SerializeScadaModel();
        }

        // private static void OnAnalogAddedEvent(object sender, EventArgs e)
        private void OnAnalogAddedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("OnAnalogEventAdded started");
            Analog analog = (Analog)e;
            IORequestBlock iorb = new IORequestBlock()
            {
                RequestType = RequestType.SEND,
                ProcessControllerName = analog.ProcContrName
            };

            DBContext dbContext = new DBContext();
            RTU rtu;
            if ((rtu = dbContext.GetRTUByName(analog.ProcContrName)) != null)
            {
                iorb.ReqAddress = (ushort)rtu.GetCommandAddress(analog);
                bool shouldCommand = false;

                switch (rtu.Protocol)
                {
                    case IndustryProtocols.ModbusTCP:

                        ModbusHandler mdbHandler = new ModbusHandler();
                        mdbHandler.Header = new ModbusApplicationHeader()
                        {
                            TransactionId = 0,
                            Length = 5,
                            ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
                            DeviceAddress = rtu.Address
                        };

                        mdbHandler.Request = new WriteRequest() { StartAddr = (ushort)rtu.GetCommandAddress(analog) };

                        if (shouldCommand = AnalogProcessor.InitialWorkPointAnalog(analog))
                        {
                            mdbHandler.Request.FunCode = FunctionCodes.WriteSingleRegister;
                            AnalogProcessor.EGUToRawValue(analog);
                            ((WriteRequest)mdbHandler.Request).Value = analog.RawCommValue;
                            iorb.SendBuff = mdbHandler.PackData();
                            iorb.SendMsgLength = iorb.SendBuff.Length;
                            Console.WriteLine(BitConverter.ToString(iorb.SendBuff, 0, 12));
                            IORequests.EnqueueRequest(iorb);

                            analog.IsInit = true;
                        }

                        break;
                }
            }
            Console.WriteLine("OnAnalogEventAdded finished");
        }

        #region Command Receiver methods
        public OMSSCADACommon.Responses.Response ReadAllAnalog(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public OMSSCADACommon.Responses.Response ReadAllCounter(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public OMSSCADACommon.Responses.Response ReadAllDigital(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public OMSSCADACommon.Responses.Response ReadSingleAnalog(string id)
        {
            throw new NotImplementedException();
        }

        public OMSSCADACommon.Responses.Response ReadSingleCounter(string id)
        {
            throw new NotImplementedException();
        }

        public OMSSCADACommon.Responses.Response ReadSingleDigital(string id)
        {
            throw new NotImplementedException();
        }

        public OMSSCADACommon.Responses.Response ReadAll()
        {
            Console.WriteLine("Response ReadAll");

            //while (!Database.IsConfigurationRunning)
            //    Thread.Sleep(100);

            List<ProcessVariable> pvs = dbContext.GetProcessVariable().ToList();

            OMSSCADACommon.Responses.Response response = new OMSSCADACommon.Responses.Response();

            foreach (ProcessVariable pv in pvs)
            {
                switch (pv.Type)
                {
                    case VariableTypes.DIGITAL:
                        Digital digital = (Digital)pv;
                        response.Variables.Add(new DigitalVariable() { VariableType = ResponseType.Digital, Id = digital.Name, State = (OMSSCADACommon.States)digital.State });
                        break;

                    case VariableTypes.ANALOG:
                        Analog analog = (Analog)pv;
                        // to do: fix this
                        response.Variables.Add(new AnalogVariable() { VariableType = ResponseType.Analog, Id = analog.Name, Value = analog.AcqValue, UnitSymbol = "w" });
                        break;


                    case VariableTypes.COUNTER:
                        Counter counter = (Counter)pv;
                        response.Variables.Add(new CounterVariable() { VariableType = ResponseType.Counter, Id = counter.Name, Value = counter.Value });
                        break;
                }
            }

            response.ResultMessage = ResultMessage.OK;

            return response;
        }

        // napravila, ali nisam testirala do kraja, to je nekad za buducnost, svakako ne treba sad :)
        public OMSSCADACommon.Responses.Response WriteSingleAnalog(string id, float value)
        {

            Console.WriteLine("WriteSingleAnalog!");

            Analog analog = null;
            OMSSCADACommon.Responses.Response response = new OMSSCADACommon.Responses.Response();

            // to do:
            //while (!Database.IsConfigurationRunning)
            //    Thread.Sleep(100);

            // getting PV from db
            ProcessVariable pv;
            if (dbContext.GetProcessVariableByName(id, out pv))
            {
                analog = (Analog)pv;
            }
            // does this ID exist in the database
            if (analog == null)
            {
                response.ResultMessage = ResultMessage.INVALID_ID;
                return response;
            }

            // to do:
            // ovde provera opsega, alarma...bla, bla


            RTU rtu;
            if ((rtu = dbContext.GetRTUByName(analog.ProcContrName)) != null)
            {
                IORequestBlock iorb = new IORequestBlock()
                {
                    RequestType = RequestType.SEND,
                    ProcessControllerName = analog.ProcContrName
                };

                iorb.ReqAddress = (ushort)rtu.GetCommandAddress(analog);

                bool shouldCommand = false;
                switch (rtu.Protocol)
                {
                    case IndustryProtocols.ModbusTCP:

                        ModbusHandler mdbHandler = new ModbusHandler
                        {
                            Header = new ModbusApplicationHeader()
                            {
                                TransactionId = 0,
                                Length = 5,
                                ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
                                DeviceAddress = rtu.Address
                            },

                            Request = new WriteRequest()
                            {
                                FunCode = FunctionCodes.WriteSingleCoil,
                                StartAddr = (ushort)rtu.GetCommandAddress(analog)
                            }
                        };

                        if (shouldCommand = AnalogProcessor.SetNewWorkPoint(analog, value))
                        {
                            mdbHandler.Request.FunCode = FunctionCodes.WriteSingleRegister;
                            ((WriteRequest)mdbHandler.Request).Value = (ushort)analog.RawCommValue;
                        }

                        iorb.SendBuff = mdbHandler.PackData();
                        iorb.SendMsgLength = iorb.SendBuff.Length;
                        break;
                }

                IORequests.EnqueueRequest(iorb);
                Console.WriteLine("enqued {0}", BitConverter.ToString(iorb.SendBuff, 0, 12));
                response.ResultMessage = ResultMessage.OK;
            }
            else
            {
                // rtu does not exist
            }

            return response;
        }

        public OMSSCADACommon.Responses.Response WriteSingleDigital(string id, CommandTypes command)
        {
            Console.WriteLine("WriteSingleDigital!");

            Digital digital = null;
            OMSSCADACommon.Responses.Response response = new OMSSCADACommon.Responses.Response();

            //while (!Database.IsConfigurationRunning)
            //    Thread.Sleep(100);


            // getting PV from db
            ProcessVariable pv;
            if (dbContext.GetProcessVariableByName(id, out pv))
            {
                digital = (Digital)pv;
            }

            // does this ID exist in the database
            if (digital == null)
            {
                response.ResultMessage = ResultMessage.INVALID_ID;
                return response;
            }

            // is this a valid command for this digital device
            if (!CommandValidator.ValidateDigitalCommand(digital, command))
            {
                response.ResultMessage = ResultMessage.INVALID_DIG_COMM;
                return response;
            }


            RTU rtu;
            if ((rtu = dbContext.GetRTUByName(digital.ProcContrName)) != null)
            {
                IORequestBlock iorb = new IORequestBlock()
                {
                    RequestType = RequestType.SEND,
                    ProcessControllerName = digital.ProcContrName
                };

                iorb.ReqAddress = (ushort)rtu.GetCommandAddress(digital);

                switch (rtu.Protocol)
                {
                    case IndustryProtocols.ModbusTCP:

                        ModbusHandler mdbHandler = new ModbusHandler
                        {
                            Header = new ModbusApplicationHeader()
                            {
                                TransactionId = 0,
                                Length = 5,
                                ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
                                DeviceAddress = rtu.Address
                            },

                            Request = new WriteRequest()
                            {
                                FunCode = FunctionCodes.WriteSingleCoil,
                                StartAddr = (ushort)rtu.GetCommandAddress(digital),
                                Value = (ushort)command
                            }
                        };

                        iorb.SendBuff = mdbHandler.PackData();
                        iorb.SendMsgLength = iorb.SendBuff.Length;
                        break;
                }

                IORequests.EnqueueRequest(iorb);
                Console.WriteLine("enqued {0}", BitConverter.ToString(iorb.SendBuff, 0, 12));

                digital.Command = command;

                response.ResultMessage = ResultMessage.OK;
            }
            else
            {
                // rtu does not exist
            }

            return response;
        }
        #endregion

    }
}