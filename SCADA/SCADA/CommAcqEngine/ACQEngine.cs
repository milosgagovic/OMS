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
using SCADA.RealtimeDatabase.Catalogs;

namespace SCADA.CommAcqEngine
{
    // Acquisition engine
    public class ACQEngine : ICommandReceiver
    {
        private static IIndustryProtocolHandler protHandler;
        private static IORequestsQueue IORequests;

        private bool shutdown;
        private int timerMsc;

        private DBContext dbContext = null;
        public static Dictionary<string, RTU> RTUs { get; set; }

        public ACQEngine()
        {
            IORequests = IORequestsQueue.GetQueue();
            shutdown = false;
            timerMsc = 5000;
            dbContext = new DBContext();
        }

        // ovde konfigurisati varijable, rtu-ove. celu rtdb
        public void Configure(string path)
        {
            RTUs = dbContext.GettAllRTUs();

            RTU rtu1 = new RTU()
            {
                Address = 1,
                Name = "RTU-1",
                Protocol = PCCommon.IndustryProtocols.ModbusTCP,

                DigOutStartAddr = 0,
                DigInStartAddr = 1000,
                AnaInStartAddr = 2000,
                AnaOutStartAddr = 3000,
                CounterStartAddr = 3500,

                DigOutCount = 8,
                DigInCount = 8,
                AnaInCount = 4,
                AnaOutCount = 2,
                CounterCount = 2
            };

            RTU rtu2 = new RTU()
            {
                Address = 1,
                Name = "RTU-2",
                Protocol = PCCommon.IndustryProtocols.ModbusTCP,

                DigOutStartAddr = 0,
                DigInStartAddr = 1000,
                AnaInStartAddr = 2000,
                AnaOutStartAddr = 3000,
                CounterStartAddr = 3500,

                DigOutCount = 8,
                DigInCount = 8,
                AnaInCount = 4,
                AnaOutCount = 2,
                CounterCount = 2

            };

            RTUs.Add(rtu1.Name, rtu1);
            RTUs.Add(rtu2.Name, rtu2);

            // rtu 1
            Digital d1 = new Digital()
            {
                ProcContrName = "RTU-1",
                Name = "MEAS_D_1",
                RelativeAddress = 0,
                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { States.CLOSED, States.OPENED },
                Command = CommandTypes.OPEN,
                State = States.CLOSED,
            };

            Digital d2 = new Digital()
            {
                ProcContrName = "RTU-1",
                Name = "MEAS_D_2",
                RelativeAddress = 1,
                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { States.CLOSED, States.OPENED },
                Command = CommandTypes.OPEN,
                State = States.CLOSED,
            };

            Digital d3 = new Digital()
            {
                ProcContrName = "RTU-1",
                Name = "MEAS_D_3",
                RelativeAddress = 2,
                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { States.CLOSED, States.OPENED },
                Command = CommandTypes.OPEN,
                State = States.CLOSED,
            };

            //Digital d4 = new Digital()
            //{
            //    ProcContrName = "RTU-1",
            //    Name = "TEST1",
            //    RelativeAddress = 3,
            //    Class = DigitalDeviceClasses.SWITCH,
            //    ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
            //    ValidStates = { States.CLOSED, States.OPENED, States.UNKNOWN },
            //    Command = CommandTypes.OPEN,
            //    State = States.CLOSED,
            //};

            //Digital d5 = new Digital()
            //{
            //    ProcContrName = "RTU-1",
            //    Name = "TEST2",
            //    RelativeAddress = 4,
            //    Class = DigitalDeviceClasses.SWITCH,
            //    ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
            //    ValidStates = { States.CLOSED, States.OPENED },
            //    Command = CommandTypes.OPEN,
            //    State = States.CLOSED,
            //};

            // rtu 2
            Digital d6 = new Digital()
            {
                ProcContrName = "RTU-2",
                Name = "TEST3",
                RelativeAddress = 0,
                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { States.CLOSED, States.OPENED },
                Command = CommandTypes.OPEN,
                State = States.CLOSED,
            };

            Digital d7 = new Digital()
            {
                ProcContrName = "RTU-2",
                Name = "TEST4",
                RelativeAddress = 1,
                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { States.CLOSED, States.OPENED },
                Command = CommandTypes.OPEN,
                State = States.CLOSED,
            };

            Digital d8 = new Digital()
            {
                ProcContrName = "RTU-2",
                Name = "TEST5",
                RelativeAddress = 2,
                Class = DigitalDeviceClasses.SWITCH,
                ValidCommands = { CommandTypes.CLOSE, CommandTypes.OPEN },
                ValidStates = { States.CLOSED, States.OPENED },
                Command = CommandTypes.OPEN,
                State = States.CLOSED,
            };

            rtu1.AddProcessVariable(d1);
            rtu1.AddProcessVariable(d2);
            rtu1.AddProcessVariable(d3);
            //rtu1.AddProcessVariable(d4);
            //rtu1.AddProcessVariable(d5);

            rtu2.AddProcessVariable(d6);
            rtu2.AddProcessVariable(d7);
            rtu2.AddProcessVariable(d8);

            // pitanje, ako uradim delete variajble iz baze, da li se brise i iz rtu-a?
            dbContext.AddProcessVariable(d1);
            dbContext.AddProcessVariable(d2);
            dbContext.AddProcessVariable(d3);
            //dbContext.AddProcessVariable(d4);
            //dbContext.AddProcessVariable(d5);
            dbContext.AddProcessVariable(d6);
            dbContext.AddProcessVariable(d7);
            dbContext.AddProcessVariable(d8);

            //d1.Name = "PromenaD1";
        }

        public void SetupRTUs()
        {
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
            List<ProcessVariable> pvs = dbContext.GetAllProcessVariables();

            while (!shutdown)
            {
                foreach (ProcessVariable pv in pvs)
                {
                    IORequestBlock iorb = new IORequestBlock()
                    {
                        RequestType = RequestType.SEND_RECV,
                        RtuName = pv.ProcContrName
                    };

                    RTU rtu;
                    if (RTUs.TryGetValue(iorb.RtuName, out rtu))
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

                                        // sad treba skontati na koju varijablu namapirati
                                        // ono sto jednoznacno identifikuje variajblu to je njena pripadnost
                                        // rtu-u i adresa u njemu
                                        //Digital target = (Digital)db.GetProcessVariableByAddress(answer.ReqAddress);
                                        Digital target = (Digital)rtu.GetProcessVariableByAddress(answer.ReqAddress);

                                        if (target != null)
                                        {
                                            int bitNumber = (int)Math.Floor((Math.Log(target.ValidStates.Count, 2)));

                                            int[] array = new int[1];
                                            response.BitValues.CopyTo(array, 0);

                                            try
                                            {
                                                lock (dbContext.Database.SyncObject)
                                                {
                                                    if (target.State != target.ValidStates[array[0]])
                                                    {
                                                        Responser responser = new Responser();
                                                        responser.DigitalStateChanged(target.Name, target.ValidStates[array[0]]);
                                                        target.State = target.ValidStates[array[0]];
                                                    }
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
            List<ProcessVariable> pvs = dbContext.GetAllProcessVariables();

            OMSSCADACommon.Response.Response response = new OMSSCADACommon.Response.Response();

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
                digital = (Digital)dbContext.GetProcessVariableByName(id);
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
                RTUs.TryGetValue(digital.ProcContrName, out rtu);

                IORequestBlock iorb = new IORequestBlock()
                {
                    RequestType = RequestType.SEND,

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
                            StartAddr = (ushort)rtu.GetAcqAddress(digital),
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