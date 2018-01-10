using SCADA.RealtimeDatabase.Catalogs;
using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase
{
    public class DBContext
    {
        public Database Database { get; set; }

        public DBContext()
        {
            Database = Database.Instance;
        }

        public void AddRTU(RTU rtu)
        {
            Database.Instance.RTUs.Add(rtu.Name, rtu);
        }

        public RTU GetRTUByName(string name)
        {
            RTU rtu;
            Database.Instance.RTUs.TryGetValue(name, out rtu);

            return rtu;
        }

        public Dictionary<string, RTU> GettAllRTUs()
        {
            return Database.RTUs;
        }
        public void AddProcessVariable(ProcessVariable pv)
        {
            Database.Instance.ProcessVariablesName.Add(pv.Name, pv);
        }

        public ProcessVariable GetProcessVariableByName(string name)
        {
            ProcessVariable pv;
            Database.Instance.ProcessVariablesName.TryGetValue(name, out pv);

            return pv;
        }

        public ProcessVariable GetProcessVariableByAddress(ushort address)
        {
            ProcessVariable pv;
            Database.Instance.ProcessVariablesAddress.TryGetValue(address, out pv);
            return pv;
        }

        public List<ProcessVariable> GetAllProcessVariables()
        {
            return Database.ProcessVariablesName.Values.ToList();
        }
    }
}
