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

        void PackData(Byte[] data);
    }
}
