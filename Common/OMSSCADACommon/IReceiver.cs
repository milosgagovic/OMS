using System;
using System.Collections.Generic;
using System.Text;

namespace OMSSCADACommon
{
    public interface IReceiver
    {
        void WriteSingleDigital(string id, CommandTypes command);
        void WriteSingleAnalog(string id, float value);
        void ReadSingleDigital(string id);
        void ReadSingleAnalog(string id);
        void ReadSingleCounter(string id);
        void ReadAllDigital(DeviceTypes type);
        void ReadAllAnalog(DeviceTypes type);
        void ReadAllCounter(DeviceTypes type);
        void RealAll();
    }
}
