using OMS_SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.SecondaryDataProcessing
{
    public class Receiver : IReceiver
    {
        public void ReadAllAnalog(VariableTypes type)
        {
            throw new NotImplementedException();
        }

        public void ReadAllCounter(VariableTypes type)
        {
            throw new NotImplementedException();
        }

        public void ReadAllDigital(VariableTypes type)
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

        public void WriteSingleCounter(string id, long value)
        {
            throw new NotImplementedException();
        }

        public void WriteSingleDigital(string id, DigitalStates state)
        {
            throw new NotImplementedException();
        }
    }
}
