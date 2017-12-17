using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OMSSCADACommon.Commands
{
    [DataContract]
    public class WriteSingleDigital : Command
    {
        [DataMember]
        public CommandTypes command;

        public override ResultMessage Execute()
        {
            return this.Receiver.WriteSingleDigital(this.Id, this.command);
        }
    }
}
