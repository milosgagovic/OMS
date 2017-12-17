using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCCommon
{
    public interface IIndustryProtocolHandler
    {
        IndustryProtocols ProtocolType { get; set; }

        void PackData(ushort transId, Byte[] data);

        // mozda ovde da budu metode send data, i receive data
        // kao npr sto na TCPClient klasi imamo metode za slanje i primanje
        // tako i ovde napraviti.... 
    }
}
