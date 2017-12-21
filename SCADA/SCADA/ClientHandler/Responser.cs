using OMSSCADACommon.Response;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ClientHandler
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class Responser : ISCADAContract_Callback
    {
        ISCADAContract_Callback proxy;

        public void ReceiveResponse(Response response)
        {
            proxy = OperationContext.Current.GetCallbackChannel<ISCADAContract_Callback>();

            proxy.ReceiveResponse(response);
        }
    }
}
