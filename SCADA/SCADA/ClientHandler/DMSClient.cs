using DMSContract;
using OMSSCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace SCADA.ClientHandler
{
    public class DMSClient : ChannelFactory<IDMSToSCADAContract>, IDMSToSCADAContract, IDisposable
    {
        DMSToSCADAProxy proxy;

        public DMSClient()
        {
            proxy = new DMSToSCADAProxy(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:8039/IDMSToSCADAContract"));
        }

        public void ChangeOnSCADA(string mrID, States state)
        {
            proxy.ChangeOnSCADA(mrID, state);
        }
    }
}
