using OMSSCADACommon;
using SCADA.SecondaryDataProcessing;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ClientHandler
{
    public class Invoker : ISCADAContract
    {
        public void ExecuteCommand(Command command)
        {
            command.Receiver = new Receiver();
            command.Execute();
        }
    }
}
