using System;
using System.Collections.Generic;
using System.Text;

namespace OMSSCADACommon
{
    public interface IReceiver
    {
        ResultMessage WriteSingleDigital(string id, CommandTypes command);
        ResultMessage WriteSingleAnalog(string id, float value);
        ResultMessage ReadSingleDigital(string id);
        ResultMessage ReadSingleAnalog(string id);
        ResultMessage ReadSingleCounter(string id);
        ResultMessage ReadAllDigital(DeviceTypes type);
        ResultMessage ReadAllAnalog(DeviceTypes type);
        ResultMessage ReadAllCounter(DeviceTypes type);
        ResultMessage RealAll();
    }
}
