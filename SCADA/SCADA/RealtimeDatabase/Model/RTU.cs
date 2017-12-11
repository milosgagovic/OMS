using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public class RTU
    {
        public string Name { get; set; }

        public Dictionary<VariableTypes, ProcessVariable[]> ProcessVariables = null;

        public RTU()
        {
            ProcessVariables = new Dictionary<VariableTypes, ProcessVariable[]>();
        }

    }
}
