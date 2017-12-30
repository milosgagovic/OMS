using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionManagerContract;

namespace TransactionManager
{
    public class TransactionCallback : ITransactionCallback
    {
        private TransactionAnswer answer;
        public TransactionCallback()
        {
            Answer = TransactionAnswer.Unanswered;
        }
        public TransactionAnswer Answer { get => answer; set => answer = value; }

        public void CallbackEnlist()
        {
            Console.WriteLine("Vratio za enlist");
        }

        public void CallbackPrepare(bool prepare)
        {
            Answer = prepare ? TransactionAnswer.Prepared : TransactionAnswer.Unprepared;
            Console.WriteLine("Odogovrio je: " + prepare);
        }

        public void CallbackCommit(string commit)
        {
            Console.WriteLine(commit);
        }

        public void CallbackRollabck(string rollback)
        {
            Console.WriteLine(rollback);
        }
    }
}
