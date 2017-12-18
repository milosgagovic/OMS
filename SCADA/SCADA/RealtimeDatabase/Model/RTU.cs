using PCCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase.Model
{
    // deo koji je fixan deo napraviti da bude static, const ili nesto
    // koji je genericki, i u RTU imamo samo niz interfejsa...

    public class RTU : ProcessController
    {
        public string HostName { get; set; }
        public short HostPort { get; set; }

        public int AcqPeriod { get; set; }

        // Channel name
        public int ChannelId { get; set; }

        public int DeviceAddress { get; set; }

        public string Name { get; set; }

        public int DInNum { get; set; }
        public int DOutNum { get; set; }
        public int AInNum { get; set; }
        public int AOutNum { get; set; }
        public int CntNum { get; set; }

        // vezano za ModbusTCPdriver, ne konkretan RTU
        // value in range 1 - 247 (0 - broadcast)
        // iz DeviceAddress  castovati u byte
        public Byte RTUAddress { get; set; }

        public IChannel Channel { get; set; } // associtad channel 

        public Dictionary<VariableTypes, ProcessVariable[]> ProcessVariables = null;

        public RTU(int DINum, int DONum, int AINum, int AONum, int CntNum)
        {
            this.DInNum = DINum;
            this.DOutNum = DONum;
            this.AInNum = AINum;
            this.AOutNum = AONum;
            this.CntNum = CntNum;

            ProcessVariables = new Dictionary<VariableTypes, ProcessVariable[]>();
        }


    }
}
