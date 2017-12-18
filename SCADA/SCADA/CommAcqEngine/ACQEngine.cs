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
            timerMsc = 1000;

            RTUs = new Dictionary<string, RTU>();
            db = new DBContext();
        }

        // ovo sam za test krenula da pravim
        public void SetupRTUs()
        {
        }

        public void StartAcquisition()
        {
            while (!shutdown)
            {
                if (RTUs.Count > 0)
                {
                    foreach (RTU rtu in RTUs.Values)
                    {
                        IORequestBlock iorb = new IORequestBlock()
                        {
                            RequestType = RequestType.SEND_RECV,
                            ChannelId = rtu.Channel.Name,
                            RTUAddress = rtu.Address
                        };

                        if (!ProtocolSetter(rtu.Channel.Protocol))
                        {
                            continue;
                        }

                        //iorb.SendBuff = protHandler.PackData();

                        IORequests.EnqueueIOReqForProcess(iorb);
                    }
                }

                Thread.Sleep(millisecondsTimeout: timerMsc);
            }
        }

        private static bool ProtocolSetter(IndustryProtocols protocol)
        {
            switch (protocol)
            {
                case IndustryProtocols.Modbus:
                    protHandler = new ModbusHandler();
                    return true;
            }

            return false;
        }

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
            throw new NotImplementedException();
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

                RTUs.TryGetValue(digital.RtuId, out RTU rtu);

                IORequestBlock iorb = new IORequestBlock()
                {
                    RequestType = RequestType.SEND,
                    ChannelId = rtu.Channel.Name,
                    RTUAddress = rtu.Address
                };

                if (!ProtocolSetter(rtu.Channel.Protocol))
                {
                    return ResultMessage.INTERNAL_SERVER_ERROR;
                }

                protHandler = new ModbusHandler();

                // form data

                IORequests.EnqueueIOReqForProcess(iorb);

                CommandValidator.CheckCommandExecution();

                return ResultMessage.OK;
            }

            return ResultMessage.OK;
        }
    }
}