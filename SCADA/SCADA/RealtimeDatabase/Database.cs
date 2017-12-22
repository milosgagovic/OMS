using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase
{
    public class Database
    {
        public Dictionary<string, ProcessVariable> ProcessVariables = null;

        public Dictionary<ushort, ProcessVariable> LookupPVs = null;

        private static Database instance;

        private Database()
        {
            this.ProcessVariables = new Dictionary<string, ProcessVariable>();
            this.LookupPVs = new Dictionary<ushort, ProcessVariable>();
        }

        public static Database Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Database();
                }
                return instance;
            }
        }
    }
}
