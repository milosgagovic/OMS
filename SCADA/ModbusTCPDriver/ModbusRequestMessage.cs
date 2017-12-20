using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    [Serializable]
    public class ModbusRequestMessage
    {
        public ModbusApplicationHeader Header { get; set; }
        public Request Request { get; set; }

        public ModbusRequestMessage()
        {
            Header = new ModbusApplicationHeader();
        }
    }
}
