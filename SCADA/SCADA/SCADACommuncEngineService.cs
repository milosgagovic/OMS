using CommunicationEngineContract;
using OMSSCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class SCADACommuncEngineService : ICommuncEngineContract_CallBack
    {
        public bool InvokeMeasurements()
        {
            Console.WriteLine("Communication Engine wants measurements");
            return true;
        }

        public bool SendCommand(Command command)
        {

            Console.WriteLine("Message from Server");
            return true;
        }
    }
}
