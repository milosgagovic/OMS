using CommunicationEngineContract;
using OMSSCADACommon.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommunicationEngine
{
    public class ClientCommEngine : ICommunicationEngineContractUpdate
    {
        public bool ReceiveAllMeasValue()
        {
            ReadAll ra =(ReadAll) MappingEngine.Instance.MappCommand();
            SCADAClient client = new SCADAClient();
            client.ExecuteCommand(ra);
            return true;
        }

        public bool ReceiveValue()
        {
            throw new NotImplementedException();
        }
    }
}
