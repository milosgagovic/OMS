using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMSSCADACommon.Commands
{
    public class ReadAll : Command
    {
        public override ResultMessage Execute()
        {
            return this.Receiver.ReadAll();
        }
    }
}
