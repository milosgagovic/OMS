using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCCommon;

namespace ModbusTCPDriver
{
    class ModbusApplicationHeader
    {
        public short TransactionId { get; set; }
        public short ProtocolId { get; set; }
        public short Length { get; set; }
        public short deviceAddress { get; set; }
    }
}
