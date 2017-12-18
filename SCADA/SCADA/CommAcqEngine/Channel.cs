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
        public IndustryProtocols Protocol { get; set; }
        public int RtuCount { get; set; }

        public int ProcContCount { get; set; }

        
        public int ChannelId { get; set; }
        public int TimeOutMsc { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }

        public Channel()
        {
        }
    }
}
