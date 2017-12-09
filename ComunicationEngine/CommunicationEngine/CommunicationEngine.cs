using CommunicationEngineContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationEngine
{
    public class CommunicationEngine: ICommunicationEngineContract
    {

        public CommunicationEngine()
        {
        }

        public bool ReceiveValue()
        {
            throw new NotImplementedException();
        }

        public void SendCommand()
        {
            throw new NotImplementedException();
        }
    }
}
