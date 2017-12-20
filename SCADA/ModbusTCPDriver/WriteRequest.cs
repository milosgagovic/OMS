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

        public override byte[] getByteRequest()
        {
            byte[] stAddr = BitConverter.GetBytes(StartAddr);
            byte[] val = BitConverter.GetBytes(Value);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(stAddr);
                Array.Reverse(val);
            }

            byte[] byteRequest = new byte[5]
            {
                (byte)FunCode,
                stAddr[0], stAddr[1],
                val[0], val[1]
            };

            return byteRequest;
        }
    }
}
