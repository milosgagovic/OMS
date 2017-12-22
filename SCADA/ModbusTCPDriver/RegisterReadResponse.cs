using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    public class RegisterReadResponse : ReadResponse
    {
        public short[] RegValues;

        public override Response getObjectResponse(byte[] bResponse)
        {
            FunCode = (FunctionCodes)bResponse[0];
            ByteCount = bResponse[1];

            // ovde neki for da ide i invertuje short po short

            return this;
        }
    }
}
