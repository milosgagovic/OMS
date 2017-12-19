using PCCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.CommAcqEngine
{
    public class RTU 
    {
        // vezano za ModbusTCPdriver, ne konkretan RTU
        // value in range 1 - 247 (0 - broadcast)
        public Byte Address { get; set; }

        // razmisliti sta sa ovim channelom za sada, kasnije ce nekad trebati...mnogo kasnije xD
        public Channel Channel { get; set; } 

        public string Name { get; set; }

        // counts of pI/O
        public int DInNum { get; set; }
        public int DOutNum { get; set; }
        public int AInNum { get; set; }
        public int AOutNum { get; set; }
        public int CntNum { get; set; }

        public int AcqPeriod { get; set; }
    }
}
