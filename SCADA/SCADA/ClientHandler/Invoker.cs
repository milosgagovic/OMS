using OMSSCADACommon;
using OMSSCADACommon.Commands;
using SCADA.CommAcqEngine;
using SCADA.SecondaryDataProcessing;
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
        public void CheckIn()
        {
            Responser.Contexts.Add(OperationContext.Current);
        }

        public ResultMessage ExecuteCommand(Command command)
        {
            command.Receiver = new ACQEngine();
            return command.Execute();
        }
    }
}
