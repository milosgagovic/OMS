using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{

    [Serializable]
    public class ReadRequest : Request
    {
        public ushort Quantity { get; set; }
    }
}
