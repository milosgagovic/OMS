using SCADA.RealtimeDatabase.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.RealtimeDatabase
{
    public class Database
    {
        public ConcurrentDictionary<string, ProcessVariable> ProcessVariablesName = null;
        public ConcurrentDictionary<string, RTU> RTUs = null;

        private static Database instance;

        private Database()
        {
            Console.WriteLine("Instancing Database");

            this.ProcessVariablesName = new ConcurrentDictionary<string, ProcessVariable>();
            this.RTUs = new ConcurrentDictionary<string, RTU>();
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
