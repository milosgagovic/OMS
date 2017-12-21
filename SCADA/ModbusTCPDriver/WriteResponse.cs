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

            Array.Reverse(bResponse, 1, 2);
            Array.Reverse(bResponse, 3, 2);

            // samo ovo mi treba, ono drugo koment
            FunCode = (FunctionCodes)bResponse[0];
            Addr = BitConverter.ToUInt16(bResponse, 0);
            Value = BitConverter.ToUInt16(bResponse, 2);

            WriteResponse wRes = new WriteResponse()
            {
                FunCode = (FunctionCodes)bResponse[0],
                Addr = BitConverter.ToUInt16(bResponse, 0),
                Value = BitConverter.ToUInt16(bResponse, 2)
            };

            return wRes;
        }
    }
}
