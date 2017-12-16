using PCCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    // Generic description of Process Controller
    public class RTU  : ISlaveDevice
    {
        public int ID { get; set; }

        public string HostName { get; set; }
        public short HostPort { get; set; }

        // vezano za ModbusTCPdriver, ne konkretan RTU
        // value in range 1 - 247 (0 - broadcast)
        public Byte RTUAddress { get; set; }

        public int channelId { get; set; } // associtad channel Id

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

        // raw data tables
        byte[] DITable = null;
        byte[] DOTable = null;
        byte[] AITable = null;
        byte[] AOTable = null;
        byte[] CNTTable = null;

        
        public Dictionary<VariableTypes, ProcessVariable[]> ProcessVariables = null;

        public RTU(int DINum, int DONum, int AINum, int AONum, int CntNum)
        {
            this.DInNum = DINum;
            this.DOutNum = DONum;
            this.AInNum = AINum;
            this.AOutNum = AONum;
            this.CntNum = CntNum;

            ProcessVariables = new Dictionary<VariableTypes, ProcessVariable[]>();

            // stavila brojeve tek tako, da bih mogla da testiram sto god xD 
            DITable = new byte[10]; // 8*10 dI
            DOTable = new byte[10];
            AITable = new byte[100];
            AOTable = new byte[100];
            CNTTable = new byte[100];

        }

        public int AcqPeriod { get; set; }

    }
}
