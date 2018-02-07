using OMSSCADACommon;
using OMSSCADACommon.Responses;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ClientHandler
{
    // takodje se ne koristi?? 

    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class Responser : ISCADAContract_Callback
    {
        ISCADAContract_Callback proxy;
        public static List<OperationContext> Contexts;

        static Responser()
        {
            Contexts = new List<OperationContext>();
        }

        public void DigitalStateChanged(string mRID, States newState)
        {
            foreach (OperationContext context in Contexts)
            {
                proxy = context.GetCallbackChannel<ISCADAContract_Callback>();

                proxy.DigitalStateChanged(mRID, newState);
            }
        }

        public void ReceiveResponse(Response response)
        {
            proxy = OperationContext.Current.GetCallbackChannel<ISCADAContract_Callback>();

            proxy.ReceiveResponse(response);
        }
    }
}
