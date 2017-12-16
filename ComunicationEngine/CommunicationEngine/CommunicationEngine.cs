using CommunicationEngineContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationEngine
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class CommunicationEngine: ICommunicationEngineContract
    {
        private static ICommuncEngineContract_CallBack callback = null;

        public static ICommuncEngineContract_CallBack Callback
        {
            get { return callback; }
            set { callback = value; }
        }

        public CommunicationEngine()
        {
        }

        public bool ReceiveValue()
        {
            Callback = OperationContext.Current.GetCallbackChannel<ICommuncEngineContract_CallBack>();
            Console.WriteLine("Something comes from SCADA");
            return true;
        }

    }
}
