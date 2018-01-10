using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public abstract class ProcessVariable
    {
        public string Name { get; set; }

        // Associated ProcessController
        public string ProcContrName { get; set; }

        // Relative address in configuration file
        public ushort RelativeAddress { get; set; }

        // Address in associated Process Controller memory.
        // public ushort ProcContrAddress { get; set; }

        public VariableTypes Type { get; set; }

        public ProcessVariable()
        {
        }
    }
}
