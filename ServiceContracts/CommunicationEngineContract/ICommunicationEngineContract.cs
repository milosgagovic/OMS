using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationEngineContract
{
    [ServiceContract]
    public interface ICommunicationEngineContract
    {
        /// <summary>
        /// Initiall method for sending command to SCADA  
        /// </summary>
        [OperationContract]
        void SendCommand();

        /// <summary>
        /// Initiall method for receiving value from SCADA
        /// </summary>
        [OperationContract]
        bool ReceiveValue();
    }
}
