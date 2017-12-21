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
        private static Database database = null;

        public DBContext()
        {
            database = Database.Instance;
        }

        public void AddProcessVariable(ProcessVariable pv)
        {
            Database.Instance.ProcessVariables.Add(pv.Name, pv);
        }

        public Digital GetSingleDigital(string name)
        {
            ProcessVariable digital;
            Database.Instance.ProcessVariables.TryGetValue(name, out  digital);

            return (Digital)digital;
        }

        public List<ProcessVariable> GetAllProcessVariables()
        {
            return database.ProcessVariables.Values.ToList();
        }
    }
}
