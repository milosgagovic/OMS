using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCCommon;

namespace ModbusTCPDriver
{
    // Concrete protocol handler class
    public class ModbusHandler : IIndustryProtocolHandler
    {
        public IndustryProtocols ProtocolType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void PackData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
