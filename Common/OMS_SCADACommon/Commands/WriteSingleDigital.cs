using System;
using System.Collections.Generic;
using System.Text;

namespace OMS_SCADACommon.Commands
{
    public class WriteSingleDigital : Command
    {
        public DigitalStates state;

        public override void Execute()
        {
            this.Receiver.WriteSingleDigital(this.Id, this.state);
        }
    }
}
