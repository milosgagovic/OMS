﻿using System;
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
            Buffer.BlockCopy(bResponse, 2, help, 0, bResponse.Length - 2);
            BitValues = new BitArray(help);          
            return this;
        }
    }
}
