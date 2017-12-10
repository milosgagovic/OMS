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
            database = new Database();
        }
    }
}
