using OMSSCADACommon;
using OMSSCADACommon.Responses;
using SCADA.CommAcqEngine;
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
    public class CommandReceiver : ICommandReceiver
    {
        DBContext db = new DBContext();
        ACQEngine acqe = new ACQEngine();

        public Response ReadAllAnalog(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public Response ReadAllCounter(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public Response ReadAllDigital(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public Response ReadSingleAnalog(string id)
        {
            throw new NotImplementedException();
        }

        public Response ReadSingleCounter(string id)
        {
            throw new NotImplementedException();
        }

        public Response ReadSingleDigital(string id)
        {
            throw new NotImplementedException();
        }

        public Response ReadAll()
        {
            throw new NotImplementedException();
        }

        public Response WriteSingleAnalog(string id, float value)
        {
            throw new NotImplementedException();
        }

        public Response WriteSingleDigital(string id, CommandTypes command)
        {
            //    Digital digital = null;

            //    try
            //    {
            //       digital = (Digital)db.GetProcessVariableByName(id);
            //    }
            //    catch(Exception e)
            //    {
            //        return ResultMessage.ID_NOT_SET;
            //    }

            //    if (digital == null)
            //    {
            //        return ResultMessage.INVALID_ID;
            //    }

            //    if (!CommandValidator.ValidateDigitalCommand(digital, command))
            //    {
            //        return ResultMessage.INVALID_DIG_COMM;
            //    }

            //    //acqe.FormRequestOnCommand();

            //    CommandValidator.CheckCommandExecution();

            //    return ResultMessage.OK;
            //}

            return new Response();
        }
    }
}
