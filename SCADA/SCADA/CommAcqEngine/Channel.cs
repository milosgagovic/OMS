using ModbusTCPDriver;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCCommon;

namespace SCADA.CommAcqEngine
{
    class Channel : IChannel
    {
        public int channelId { get; set; }

        public IndustryProtocols Protocol { get; set; }

        public int TimeOutMsc { get; set; }

        public string Name { get; set; }

        public String Info { get; set; }

        // logic address of port
        public CommunicationPort COMPort { get; set; }

        public List<RTU> RtuList { get; set; }

        public int RtuCount { get; set; }

        public Channel()
        {
            RtuList = new List<RTU>();
        }
    }
}
