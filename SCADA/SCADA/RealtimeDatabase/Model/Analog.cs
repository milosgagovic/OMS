using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public class Analog : ProcessVariable
    {
        public Analog()
        {
            this.Type = VariableTypes.ANALOG;
        }

        public float Value { get; set; }
    }
}
