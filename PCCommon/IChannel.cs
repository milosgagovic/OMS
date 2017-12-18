using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCCommon
{
    public interface IChannel
    {
        IndustryProtocols Protocol { get; set; }

        int TimeOutMsc { get; set; }

        string Name { get; set; }

        String Info { get; set; }       

        //IPSide ipSide { get; set; }
    }
}
