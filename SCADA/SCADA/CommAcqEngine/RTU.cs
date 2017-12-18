using PCCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.CommAcqEngine
{
    public class RTU : ISlaveDevice
    {
        public Byte Address { get; set; }

        public Channel Channel { get; set; } // associtad channel Id

        // ovo je kao id tog rtu-a,  odnosno jedinstveno ime u sistemu. videti da li string ili int ostaviti, ili oba
        public string Name { get; set; }

        // ovo  treba izmestiti u RTU parent klasu ili interfejs, to je ono sto je potrebno da komunikacioni sloj zna da 
        // bi ubacivao podatke. komunikacioni sloj koji koristi RTU ne treba nista da zna o ProcessVariable promenljivim...

        // counts of pI/O
        public int DInNum { get; set; }
        public int DOutNum { get; set; }
        public int AInNum { get; set; }
        public int AOutNum { get; set; }
        public int CntNum { get; set; }

        public int AcqPeriod { get; set; }
    }
}
