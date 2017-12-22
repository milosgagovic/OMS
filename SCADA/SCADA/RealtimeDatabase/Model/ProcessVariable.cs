using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    public abstract class ProcessVariable
    {
        public PVID PVID { get; set; }

        public string Name { get; set; }

        public string RtuName { get; set; }

        public byte RtuAddress { get; set; }

        public ushort Address { get; set; }

        public VariableTypes Type { get; set; }
    }
}
