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
using OMSSCADACommon.Response;
using SCADA.ClientHandler;

namespace SCADA.CommAcqEngine
{
    // Acquisition engine
    public class ACQEngine : ICommandReceiver
    {
        private static IIndustryProtocolHandler protHandler;
        private static IORequestsQueue IORequests;

        private bool shutdown;
        private int timerMsc;

        private DBContext db = null;

        public static Dictionary<string, RTU> RTUs { get; set; }

        public ACQEngine()
        {
            IORequests = IORequestsQueue.GetQueue();
            shutdown = false;
            timerMsc = 5000;

            RTUs = new Dictionary<string, RTU>();
            db = new DBContext();
            SetupRTUs();
        }

        public void SetupRTUs()
        {
            //RTU rtu1 = new RTU(8, 8, 4, 4, 2);
            RTU rtu1 = new RTU();
            rtu1.Address = 1;
            rtu1.Name = "RTU-1";
            RTUs.Add(rtu1.Name, rtu1);

            RTU rtu2 = new RTU();
            rtu2.Address = 1;
            rtu2.Name = "RTU-2";
            RTUs.Add(rtu2.Name, rtu2);
        }

        public void StartAcquisition()
        {
            List<ProcessVariable> pvs = db.GetAllProcessVariables();

            while (!shutdown)
            {
                foreach (ProcessVariable pv in pvs)
                {
                    IORequestBlock iorb = new IORequestBlock()
                    {
                        RequestType = RequestType.SEND_RECV,
                        RtuName = pv.RtuName,
                        Address = pv.Address
                    };

                    RTU rtu;
                    if (RTUs.TryGetValue(iorb.RtuName, out rtu))
                    {
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
                                            StartAddr = digital.Address,
                                            Quantity = (ushort)(Math.Ceiling((Math.Log(digital.ValidStates.Count, 2))))
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

                        IORequests.EnqueueIOReqForProcess(iorb);
                    }
                    else
                    {
                        // dodati message da je nevalidno podesavanje PV, da taj RTU ne postoji bla bla
                        // ishedlovati nekako tu pv ili rtu....
                        continue;
                    }                   
                }

                Thread.Sleep(millisecondsTimeout: timerMsc); // a ovo je timeout acq ciklusa
            }
        }

        public void ProcessPCAnwers()
        {
            // kad bude shutdown uraditi clean...          
            while (!shutdown)
            {
                bool isSuccessful;
                IORequestBlock answer = IORequests.GetAnswer(out isSuccessful);

                if (isSuccessful)
                {
                    RTU rtu;
                    if (RTUs.TryGetValue((answer.RtuName), out rtu))
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

                                        Digital target = (Digital)db.GetProcessVariableByAddress(answer.Address);

                                        if (target != null)
                                        {
                                            int bitNumber = (int)Math.Floor((Math.Log(target.ValidStates.Count, 2)));

                                            int[] array = new int[1];
                                            response.BitValues.CopyTo(array, 0);

                                            try
                                            {
                                                lock (db.Database.SyncObject)
                                                {
                                                    target.State = target.ValidStates[array[0]];
                                                }
                                                Console.WriteLine("Digital variable {0}, state: {1}", target.Name, target.State);
                                            }
                                            catch
                                            {
                                                Console.WriteLine("Digital variable {0}, state: INVALID", target.Name);
                                            }
                                        }

                                        break;
                                }

                                break;
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        // CommandReceiver methods
        //-------------------------

        public ResultMessage ReadAllAnalog(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public ResultMessage ReadAllCounter(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public ResultMessage ReadAllDigital(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public ResultMessage ReadSingleAnalog(string id)
        {
            throw new NotImplementedException();
        }

        public ResultMessage ReadSingleCounter(string id)
        {
            throw new NotImplementedException();
        }

        public ResultMessage ReadSingleDigital(string id)
        {
            throw new NotImplementedException();
        }

        public ResultMessage ReadAll()
        {
            List<ProcessVariable> pvs = db.GetAllProcessVariables();

            OMSSCADACommon.Response.Response response = new OMSSCADACommon.Response.Response();

            foreach (ProcessVariable pv in pvs)
            {
                switch (pv.Type)
                {
                    case VariableTypes.DIGITAL:
                        Digital digital = (Digital)pv;
                        response.Variables.Add(new DigitalVariable() { Id = digital.Name, State = (States)digital.State });
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

            Responser r = new Responser();
            r.ReceiveResponse(response);

            return ResultMessage.OK;
        }

        public ResultMessage WriteSingleAnalog(string id, float value)
        {
            throw new NotImplementedException();
        }

        public ResultMessage WriteSingleDigital(string id, CommandTypes command)
        {
            Digital digital = null;

            // is ID set in the request
            try
            {
                digital = (Digital)db.GetProcessVariableByName(id);
            }
            catch (Exception e)
            {
                return ResultMessage.ID_NOT_SET;
            }

            // does this ID exist in the database
            if (digital == null)
            {
                return ResultMessage.INVALID_ID;
            }

            // is this a valid command for this digital device
            if (!CommandValidator.ValidateDigitalCommand(digital, command))
            {
                return ResultMessage.INVALID_DIG_COMM;
            }

            // execute command if it's different from current command
            if (digital.Command != command)
            {
                digital.Command = command;
                RTU rtu;
                RTUs.TryGetValue(digital.RtuName, out rtu);

                IORequestBlock iorb = new IORequestBlock()
                {
                    RequestType = RequestType.SEND,
                    RTUAddress = rtu.Address
                };

                protHandler = new ModbusHandler();

                switch (rtu.Protocol)
                {
                    case IndustryProtocols.ModbusTCP:
                        ModbusHandler mdbHandler = (ModbusHandler)protHandler;

                        mdbHandler.Header = new ModbusApplicationHeader()
                        {
                            TransactionId = 0,
                            Length = 0,
                            ProtocolId = (ushort)IndustryProtocols.ModbusTCP,
                            DeviceAddress = rtu.Address
                        };

                        mdbHandler.Request = new WriteRequest()
                        {
                            FunCode = FunctionCodes.WriteSingleCoil,
                            StartAddr = digital.Address,
                            Value = (ushort)command
                        };

                        iorb.SendBuff = mdbHandler.PackData();
                        break;
                }

                IORequests.EnqueueIOReqForProcess(iorb);

                // yet to be implemented
                //CommandValidator.CheckCommandExecution();

                IORequests.EnqueueIOReqForProcess(iorb);

                CommandValidator.CheckCommandExecution();

                return ResultMessage.OK;
            }

            return ResultMessage.OK;
        }
    }
}