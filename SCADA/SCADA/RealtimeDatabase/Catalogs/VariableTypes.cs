using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public enum VariableTypes
    {
        /* prvo je ideja bila da se koriste ANALOGIN i ANLOGOUT za analogne, ali mislim da ce biti lakse samo sa ANALOG. da bude slicno kao sa DIGITAL
            znaci ako ti je lakse zanemari klase anlogin i analogout...to je po profesorovoj knjizi sve dizajnirano inicijalno
             */
        ANALOG = 0,
        ANALOGIN, // verovatno nece trebati, izbrisati kasnije..
        ANALOGOUT,
        DIGITAL,
        COUNTER,
        OBJECT
    }
}
