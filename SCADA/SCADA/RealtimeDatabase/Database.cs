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

        private static Database instance;

        private Database()
        {
            this.ProcessVariables = new Dictionary<string, ProcessVariable>();
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
