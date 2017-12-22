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

        // ovde poslati iorbe za 3 dig uredjaja
        // da citas za svaki od njih
        // pa procesuiranje reply-ova
        // citanje digitalnih ulaza odnosno stanja 
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
                        // RTUAddress = pv.RtuAddress, // ne koristim
                        RtuName = pv.RtuName,
                        Address = pv.Address
                    };

                    RTU rtu;
                    if (RTUs.TryGetValue(iorb.RtuName, out rtu))
                    {
                        if (!ProtocolSetter(rtu.Protocol))
                        {
                            continue;
                            //return ResultMessage.INTERNAL_SERVER_ERROR;
                        }

                        //treba da bude switch u zavisnosti od protokola
                        ModbusHandler mdbHandler = (ModbusHandler)protHandler;

                        switch (pv.Type)
                        {
                            case VariableTypes.DIGITAL:
                                Digital digital = (Digital)pv;
                                mdbHandler.Request = new ReadRequest()
                                {
                                    FunCode = FunctionCodes.ReadDiscreteInput,
                                    StartAddr = digital.Address,
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
                        iorb.SendMsgLength = iorb.SendBuff.Length;
                        //Console.WriteLine("     data for send ->");
                        //Console.WriteLine(BitConverter.ToString(iorb.SendBuff));

                        IORequests.EnqueueIOReqForProcess(iorb);
                    }
                    else
                    {
                        // dodati message da je nevalidno podesavanje PV, da taj RTU ne postoji bla bla
                        // ishedlovati nekako tu pv ili rtu....
                        continue;
                    }

                    // znaci ovde siba zahteve za akvizicijom bez ikakvog sleep-a, za sve pv
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
                        // treba takodje onaj switch case sa protocol handler-om
                        if (!ProtocolSetter(IndustryProtocols.ModbusTCP))
                        {
                            //return ResultMessage.INTERNAL_SERVER_ERROR;
                        }

                        ModbusHandler mdbHandler = (ModbusHandler)protHandler;
                        mdbHandler.UnpackData(answer.RcvBuff, answer.RcvMsgLength);

                        switch (mdbHandler.Response.FunCode)
                        {
                            case FunctionCodes.ReadDiscreteInput:
                                BitReadResponse response = (BitReadResponse)mdbHandler.Response;

                                Digital target = (Digital)db.PVLookup(answer.Address);

                                // updating database
                                // tu treba neki sync
                                target.SetState(response.BitValues);                               
                                break;
                        }




                    }
                }

                Thread.Sleep(millisecondsTimeout: timerMsc);
            }
        }

        private static bool ProtocolSetter(IndustryProtocols protocol)
        {
            switch (protocol)
            {
                case IndustryProtocols.ModbusTCP:
                    protHandler = new ModbusHandler();
                    return true;
            }

            return false;
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
                digital = db.GetSingleDigital(id);
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

                if (!ProtocolSetter(rtu.Protocol))
                {
                    return ResultMessage.INTERNAL_SERVER_ERROR;
                }

                protHandler = new ModbusHandler();

                switch (rtu.Protocol)
                //switch (rtu.Channel.Protocol)
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