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
        public Dictionary<string, RTU> RTUs = new Dictionary<string, RTU>();
        public Dictionary<string, ProcessVariable> ProcessVariables = new Dictionary<string, ProcessVariable>();

        private static Database instance;

        private Database()
        {
            this.RTUs = new Dictionary<string, RTU>();
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
