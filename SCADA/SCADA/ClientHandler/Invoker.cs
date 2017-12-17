using OMSSCADACommon;
using OMSSCADACommon.Commands;
using SCADA.SecondaryDataProcessing;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA.ClientHandler
{
    public class Invoker : ISCADAContract
    {
        public ResultMessage ExecuteCommand(Command command)
        {
            command.Receiver = new Receiver();
            return command.Execute();
        }
    }
}
