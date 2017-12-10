using System;
using System.Collections.Generic;
using System.Text;

namespace OMS_SCADACommon
{
    public interface IReceiver
    {
        void WriteSingleDigital(string id, DigitalStates state);
        void WriteSingleAnalog(string id, float value);
        void WriteSingleCounter(string id, long value);
        void ReadSingleDigital(string id);
        void ReadSingleAnalog(string id);
        void ReadSingleCounter(string id);
        void ReadAllDigital(VariableTypes type);
        void ReadAllAnalog(VariableTypes type);
        void ReadAllCounter(VariableTypes type);
        void RealAll();
    }
}
