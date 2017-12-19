using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public class AnalogIn : ProcessVariable
    {
        public AnalogIn()
        {
            this.Type = VariableTypes.ANALOGIN;
        }

        public float Value { get; set; }
    }
}
