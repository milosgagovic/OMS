using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTCPDriver
{
    public class ReadResponse : Response
    {
        public Byte FunCode { get; set; }
        public Byte ByteCount { get; set; }

        public override Response getObjectResponse(byte[] bResponse)
        {
            throw new NotImplementedException();
        }
    }
}
