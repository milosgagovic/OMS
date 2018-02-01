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
using OMSSCADACommon;
using SCADA.SecondaryDataProcessing;
using OMSSCADACommon.Responses;
using SCADA.ClientHandler;
using SCADA.RealtimeDatabase.Catalogs;
using System.Net.Sockets;
using SCADA.ConfigurationParser;

namespace SCADA.CommAcqEngine
{
    // Acquisition-Commandig engine
    public class ACQEngine : ICommandReceiver
    {
        private static IORequestsQueue IORequests;
        private bool isShutdown;

        private int timerMsc;

        private DBContext dbContext = null;

        public ACQEngine()
        {
            Console.WriteLine("AcqEngine Instancing()");

            IORequests = IORequestsQueue.GetQueue();
            dbContext = new DBContext();

            isShutdown = false;
            timerMsc = 1000;
        }

        /// <summary>
        /// Reading database data from configPath,
        /// configuring RTUs and Process Variables
        /// </summary>
        /// <param name="configPath"></param>
        public void Configure(string configPath)
        {
            // dodati metode za serijalizaciju baze
            ScadaModelParser parser = new ScadaModelParser(configPath);
            if (!parser.DoParse())
            {
                // error....
            }
        }

        /// <summary>
        /// Producing IORB requests for automatic data acquistion
        /// </summary>
        public void StartAcquisition()
        {
            int processing = 0;
            List<ProcessVariable> pvs;

            while (!isShutdown)
            {
                pvs = dbContext.GetAllProcessVariables();
                foreach (ProcessVariable pv in pvs)
                {

                    IORequestBlock iorb = new IORequestBlock()
                    {
                        RequestType = RequestType.SEND_RECV,
                        ProcessControllerName = pv.ProcContrName
                    };

                    RTU rtu;
                    if ((rtu = dbContext.GetRTUByName(pv.ProcContrName)) != null)
                    {
                        iorb.ReqAddress = (ushort)rtu.GetAcqAddress(pv);

                        switch (rtu.Protocol)
                        {
                            case IndustryProtocols.ModbusTCP:

                                ModbusHandler mdbHandler = new ModbusHandler();

                                switch (pv.Type)
                                {
                                    case VariableTypes.DIGITAL:

                                        Digital digital = (Digital)pv;

                                        mdbHandler.Request = new ReadRequest()
                                        {
                                            FunCode = FunctionCodes.ReadDiscreteInput,
                                            StartAddr = (ushort)rtu.GetAcqAddress(pv),
                                            Quantity = (ushort)(Math.Floor((Math.Log(digital.ValidStates.Count, 2))))
                                        };
                                        mdbHandler.Header = new ModbusApplicationHeader()
                                        {
                                            TransactionId = 0,
                                            Length = 5,
                                            ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
                                            DeviceAddress = rtu.Address
                                        };
                                        break;

                                    case VariableTypes.ANALOGIN:

                                        AnalogIn analog = (AnalogIn)pv;
                                        break;

                                    case VariableTypes.COUNTER:

                                        Counter counter = (Counter)pv;
                                        break;
                                }

                                iorb.SendBuff = mdbHandler.PackData();
                                break;
                        }

                        iorb.SendMsgLength = iorb.SendBuff.Length;
                        IORequests.EnqueueRequest(iorb);

                    }
                    else
                    {
                        // ne postoji taj rtu sa tim imenom. izbrisati te procesne varijable sa rtu-om tog imena
                        // dodati message da je nevalidno podesavanje PV
                        Console.WriteLine("Invalid config: ProcContrName = {0} does not exists.", pv.ProcContrName);
                        continue;
                    }


                    /*

                    if (RTUs.TryGetValue(pv.ProcContrName, out rtu))
                    {
                        iorb.ReqAddress = (ushort)rtu.GetAcqAddress(pv);

                        switch (rtu.Protocol)
                        {
                            case IndustryProtocols.ModbusTCP:

                                ModbusHandler mdbHandler = new ModbusHandler();

                                switch (pv.Type)
                                {
                                    case VariableTypes.DIGITAL:

                                        Digital digital = (Digital)pv;

                                        mdbHandler.Request = new ReadRequest()
                                        {
                                            FunCode = FunctionCodes.ReadDiscreteInput,
                                            StartAddr = (ushort)rtu.GetAcqAddress(pv),
                                            Quantity = (ushort)(Math.Floor((Math.Log(digital.ValidStates.Count, 2))))
                                        };
                                        mdbHandler.Header = new ModbusApplicationHeader()
                                        {
                                            TransactionId = 0,
                                            Length = 5,
                                            ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
                                            DeviceAddress = rtu.Address
                                        };
                                        break;

                                    case VariableTypes.ANALOGIN:

                                        AnalogIn analog = (AnalogIn)pv;
                                        break;

                                    case VariableTypes.COUNTER:

                                        Counter counter = (Counter)pv;
                                        break;
                                }


                                iorb.SendBuff = mdbHandler.PackData();
                                //Console.WriteLine("packed data iorb.sendBuff= {0}", BitConverter.ToString(iorb.SendBuff, 0, 12));

                                break;
                        }

                        iorb.SendMsgLength = iorb.SendBuff.Length;

                        //Console.WriteLine("*** StartAcquisition(){0}, IORequests.Count = {1},  REQUEST for enqueue= ", processing, IORequests.IORequests.Count, BitConverter.ToString(iorb.SendBuff, 0, iorb.SendMsgLength));
                        IORequests.EnqueueRequest(iorb);
                        //Console.WriteLine("**** StartAcquisition(){0}, IORequests.Count = {1},  REQUEST ENQUEUED = ", processing, IORequests.IORequests.Count, BitConverter.ToString(iorb.SendBuff, 0, iorb.SendMsgLength));

                    }
                    else
                    {   // ne postoji taj rtu sa tim imenom. izbrisati te procesne varijable sa rtu-om tog imena
                        // dodati message da je nevalidno podesavanje PV
                        continue;
                    }

                    */

                    processing++;
                }

                // Thread.Sleep(millisecondsTimeout: timerMsc);
                Thread.Sleep(millisecondsTimeout: 2000);
            }
            // to do: close all communication channels
            // delete...

            return;
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
                    Console.WriteLine("answer");
                    RTU rtu;
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
                                        BitReadResponse response = (BitReadResponse)mdbHandler.Response;
                                        ProcessVariable pv;
                                        Digital target = null;
                                        if (rtu.GetProcessVariableByAddress(answer.ReqAddress, out pv))
                                        {
                                            target = (Digital)pv;
                                        }

                                        if (target != null)
                                        {
                                            int[] array = new int[1];
                                            response.BitValues.CopyTo(array, 0);

                                            try
                                            {
                                                // locking?
                                                lock (dbContext.Database.SyncObject)
                                                {
                                                    if (target.State != target.ValidStates[array[0]])
                                                    {
                                                        Console.WriteLine("CHANGE!");
                                                        target.State = target.ValidStates[array[0]];
                                                        DMSClient dMSClient = new DMSClient();
                                                        dMSClient.ChangeOnSCADA(target.Name, target.State);

                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                // Console.WriteLine("Digital variable {0}, state: INVALID", target.Name);
                                            }
                                        }

                                        break;
                                }

                                break;
                        }
                    }
                    else
                    {
                        // ...deleted rtu?
                    }
                }

                Thread.Sleep(100);
            }
            // to do: close all communication channels
            // delete...

            return;
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

            List<ProcessVariable> pvs = dbContext.GetAllProcessVariables();

            OMSSCADACommon.Responses.Response response = new OMSSCADACommon.Responses.Response();

            foreach (ProcessVariable pv in pvs)
            {
                switch (pv.Type)
                {
                    case VariableTypes.DIGITAL:
                        Digital digital = (Digital)pv;
                        response.Variables.Add(new DigitalVariable() { Id = digital.Name, State = (OMSSCADACommon.States)digital.State });
                        break;
                    case VariableTypes.ANALOGIN:
                        AnalogIn analog = (AnalogIn)pv;
                        response.Variables.Add(new AnalogVariable() { Id = analog.Name, Value = analog.Value });
                        break;
                    case VariableTypes.COUNTER:
                        Counter counter = (Counter)pv;
                        response.Variables.Add(new CounterVariable() { Id = counter.Name, Value = counter.Value });
                        break;
                }
            }

            response.ResultMessage = ResultMessage.OK;

            return response;
        }

        public OMSSCADACommon.Responses.Response WriteSingleAnalog(string id, float value)
        {
            throw new NotImplementedException();
        }

        public OMSSCADACommon.Responses.Response WriteSingleDigital(string id, CommandTypes command)
        {
            Console.WriteLine("WriteSingleDigital!");

            Digital digital = null;
            OMSSCADACommon.Responses.Response response = new OMSSCADACommon.Responses.Response();

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