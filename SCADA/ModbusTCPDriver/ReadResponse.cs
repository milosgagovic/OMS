using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    [Serializable]
    public class ReadResponse
    {
        public Byte FunCode { get; set; }
        public Byte ByteCount { get; set; }
    }
}
