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
        public Dictionary<string, ProcessVariable> ProcessVariablesName = null;

        public Dictionary<ushort, ProcessVariable> ProcessVariablesAddress = null;

        // mora RTU da bude osnovni element u bazi ako zelimo da ih imamo vise?
        public Dictionary<string, RTU> RTUs = null;

        private static Database instance;
        public object SyncObject = null;

        private Database()
        {
            this.ProcessVariablesName = new Dictionary<string, ProcessVariable>();
            this.ProcessVariablesAddress = new Dictionary<ushort, ProcessVariable>();
            this.SyncObject = new object();
            this.RTUs = new Dictionary<string, RTU>();
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
