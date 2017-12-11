using System;
using System.Collections.Generic;
using System.Text;

namespace OMSSCADACommon.Commands
{
    public class WriteSingleDigital : Command
    {
        public CommandTypes command;

        public override void Execute()
        {
            this.Receiver.WriteSingleDigital(this.Id, this.command);
        }
    }
}
