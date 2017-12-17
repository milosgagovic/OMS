using OMSSCADACommon;
using SCADA.RealtimeDatabase;
using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.SecondaryDataProcessing
{
    public class Receiver : IReceiver
    {
        DBContext db = new DBContext();

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

        public ResultMessage RealAll()
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

            try
            {
               digital = db.GetSingleDigital(id);
            }
            catch(Exception e)
            {
                return ResultMessage.ID_NOT_SET;
            }

            if (digital == null)
            {
                return ResultMessage.INVALID_ID;
            }

            if (!CommandValidator.ValidateDigitalCommand(digital, command))
            {
                return ResultMessage.INVALID_DIG_COMM;
            }

            // send to mdbsim

            CommandValidator.CheckCommandExecution();

            return ResultMessage.OK;
        }
    }
}
