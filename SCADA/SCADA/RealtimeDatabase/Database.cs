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
        public object SyncObject = null;

        public Dictionary<string, ProcessVariable> ProcessVariablesName = null;
        public Dictionary<string, RTU> RTUs = null;

        private static Database instance;

        private Database()
        {
            Console.WriteLine("Instancing Database");

            this.SyncObject = new object();

            this.ProcessVariablesName = new Dictionary<string, ProcessVariable>();
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
