using PCCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    [Serializable]
    public abstract class Request
    {
        public FunctionCodes FunCode { get; set; }
        public ushort StartAddr { get; set; }
    }
}
