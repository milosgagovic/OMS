using OMSSCADACommon;
using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA.ClientHandler
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class Invoker : ISCADAContract
    {
        public Response ExecuteCommand(Command command)
        {
            Console.WriteLine("Invoker.ExecuteCommand");
            command.Receiver = new CommunicationAndControlling.SecondaryDataProcessing.CommAcqEngine();
            return command.Execute();
        }

        public bool Ping()
        {
            return true;
        }
    }
}
