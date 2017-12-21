using FTN.Common;
using OMSSCADACommon.Response;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationEngine
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class SCADACallback : ISCADAContract_Callback
    {
        //ovde puca proveriti to
        //MappingEngine mapEngine = new MappingEngine();

        public void ReceiveResponse(Response response)
        {
            //List<ResourceDescription> resDescList = mapEngine.MappResult(response);
        }
    }
}
