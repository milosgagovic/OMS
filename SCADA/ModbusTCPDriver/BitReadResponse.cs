using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    public class BitReadResponse : ReadResponse
    {
        // need to process bits, not bytes
        public BitArray BitValues;

        public override Response getObjectResponse(byte[] bResponse)
        {
            FunCode = (FunctionCodes)bResponse[0];
            ByteCount = bResponse[1];

            byte[] help = new byte[bResponse.Length - 2];
            Buffer.BlockCopy(help, 0, bResponse, 2, bResponse.Length - 2);
            BitValues = new BitArray(help);

            /// ************************************* problem
            Console.WriteLine("bits");
            Console.WriteLine(BitValues.Get(0));
            Console.WriteLine(BitValues.ToString());
            return this;
        }
    }
}
