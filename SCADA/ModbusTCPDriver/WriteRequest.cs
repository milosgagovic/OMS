using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    [Serializable]
    public class WriteRequest : Request
    {
        public ushort Value { get; set; }
    }
}
