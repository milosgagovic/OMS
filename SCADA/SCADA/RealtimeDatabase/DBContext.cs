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

        public Digital GetSingleDigital(string name)
        {
            Database.Instance.ProcessVariables.TryGetValue(name, out ProcessVariable digital);

            return (Digital)digital;
        }

        public List<RTU> GetAllRTUs()
        {
            return Database.Instance.RTUsList;
        }
    }
}
