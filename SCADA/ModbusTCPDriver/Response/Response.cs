using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    
    public abstract class Response
    {
        public FunctionCodes FunCode { get; set; }

        public abstract Response GetObjectResponse(byte[] bResponse);
    }
}
