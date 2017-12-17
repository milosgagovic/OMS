using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    public class ReadRequest
    {
        public Byte FunCode { get; set; }
        public ushort startAddr { get; set; }
        public ushort quantity { get; set; }
    }
}
