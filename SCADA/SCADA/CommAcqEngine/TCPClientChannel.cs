using PCCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.CommAcqEngine
{
    // mozda visak
    public class Channel 
    {
        public IndustryProtocols Protocol { get; set; }
        public int TimeOutMsc { get; set; } // connect time out
        public string Name { get; set; }
        public string Info { get; set; }
    }
}
