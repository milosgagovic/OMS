using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    public class WriteResponse : Response
    {
        // Coil Addr or HoldReg Addr
        public ushort Addr { get; set; }

        public ushort Value { get; set; }

        public override Response getObjectResponse(byte[] bResponse)
        {
            FunCode = (FunctionCodes)bResponse[0];

            Array.Reverse(bResponse, 1, 2);
            Array.Reverse(bResponse, 3, 2);
          
            Addr = BitConverter.ToUInt16(bResponse, 1);
            Value = BitConverter.ToUInt16(bResponse, 3);

            return this;
        }
    }
}
