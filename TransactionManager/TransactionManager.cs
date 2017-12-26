using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManagerContract;

namespace TransactionManager
{
    public class TransactionManager : IOMSClient
    {
        void IOMSClient.UpdateSystem()
        {
            Console.WriteLine("Update System started.");
        }
    }
}
