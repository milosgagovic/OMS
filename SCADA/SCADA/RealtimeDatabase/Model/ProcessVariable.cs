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

        public string RtuId { get; set; }

        public VariableTypes type { get; set; }

        public ProcessVariable()
        {
            // sklonila sam ovo da se "instancira" novi RTU svaki put kad se napravi PV.
            // nema mi smisla, necemo imati xxx RTUova...
            // nego RTUove napravimo ranije, i onda kad pravimo PV, asocijacijom preko IDa ih povezemo sa odgovarajucim RTUom

            //RTU = new RTU();
        }
        
    }
}
