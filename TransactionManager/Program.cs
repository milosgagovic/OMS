using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TransactionManagerContract;

namespace TransactionManager
{
    class Program
    {


        static void Main(string[] args)
        {
            TransactionManagerService tms = new TransactionManagerService();
            Console.WriteLine("Transaction Manager is started");
            TransactionManager transactionManager = new TransactionManager();
            //transactionManager.Enlist();
            //transactionManager.Prepare();

            tms.Start();

            Console.ReadLine();

            tms.Stop();
        }



    }
}
