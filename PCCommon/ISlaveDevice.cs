using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCCommon
{   
    // enpoint u komunikaciji. Metoda specificnog protokola koja salje podatke
    // mora samo da zna ove parametre, ne mora da zna unutrasnjost RTU-a koja je vezana za konkretan scada model
    // razmisliti o komunikaciji 'unazad'...
    public interface ISlaveDevice
    {
    }
}
