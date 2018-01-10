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
        public int DeviceAddress { get; set; }

        // unique name  
        public string Name { get; set; }

        public int AcqPeriod { get; set; }

        public string HostName { get; set; }

        public short HostPort { get; set; }
    }

}
