using CommunicationEngineContract;
using FTN.Common;
using OMSSCADACommon.Response;
using PubSubscribe;
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

        private static CommunicationEngine instance;
        private Response responseFromSCADA;
        public static CommunicationEngine Instance
        {
            get
            {
                if (instance == null)
                    instance = new CommunicationEngine();
                return instance;
            }
        }

        public Response ResponseFromSCADA
        {
            get
            {
                return responseFromSCADA;
            }

            set
            {
                responseFromSCADA = value;
            }
        }

        public static ICommuncEngineContract_CallBack Callback
        {
            get { return callback; }
            set { callback = value; }
        }

        public CommunicationEngine()
        {
        }

        public bool SendResponseToClient()
        {
            List<ResourceDescription> result = MappingEngine.Instance.MappResult(ResponseFromSCADA);
            //proslijediti klijentu
            return true;
        }

        public bool ReceiveValue()
        {
            Callback = OperationContext.Current.GetCallbackChannel<ICommuncEngineContract_CallBack>();
            Console.WriteLine("Something comes from SCADA");
            return true;
        }

    }
}
