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

        /// <summary>
        /// Storing the Process Varible in dictionary. Key= pv.Name, Value=pv
        /// </summary>
        /// <param name="pv"></param>
        public void AddProcessVariable(ProcessVariable pv)
        {
            Database.Instance.ProcessVariablesName.Add(pv.Name, pv);
        }

        /// <summary>
        /// Return Process Variable if exists; 
        /// otherwise pv=null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool GetProcessVariableByName(string name, out ProcessVariable pv)
        {
            return (Database.Instance.ProcessVariablesName.TryGetValue(name, out pv));
        }

        public List<ProcessVariable> GetAllProcessVariables()
        {
            return Database.ProcessVariablesName.Values.ToList();
        }
    }
}
