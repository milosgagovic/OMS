using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCCommon
{
    // Generic description of Process Controller (communication endoint)

    public class ProcessController
    {
        public string HostName { get; set; }

        public short HostPort { get; set; }

        public int DeviceAddress { get; set; } // to je onaj int - short

        public string ChannelName { get; set; } // associtad channel Id
               
        public string Name { get; set; } // unique name   
    }

}
