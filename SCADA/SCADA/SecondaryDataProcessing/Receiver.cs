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

        public void ReadAllAnalog(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public void ReadAllCounter(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public void ReadAllDigital(OMSSCADACommon.DeviceTypes type)
        {
            throw new NotImplementedException();
        }

        public void ReadSingleAnalog(string id)
        {
            throw new NotImplementedException();
        }

        public void ReadSingleCounter(string id)
        {
            throw new NotImplementedException();
        }

        public void ReadSingleDigital(string id)
        {
            throw new NotImplementedException();
        }

        public void RealAll()
        {
            throw new NotImplementedException();
        }

        public void WriteSingleAnalog(string id, float value)
        {
            throw new NotImplementedException();
        }

        public void WriteSingleDigital(string id, CommandTypes command)
        {
            Digital digital = db.GetSingleDigital(id);

            if (digital == null)
            {
                return;
            }

            if (!CommandValidator.ValidateDigitalCommand(digital, command))
            {
                return;
            }

            // send to mdbsim

            CommandValidator.CheckCommandExecution();
        }
    }
}
