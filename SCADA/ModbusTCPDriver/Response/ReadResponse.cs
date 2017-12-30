using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    public abstract class ReadResponse : Response
    {
        public Byte ByteCount { get; set; }       
    }
}
