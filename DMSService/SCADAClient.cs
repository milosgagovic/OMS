using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using SCADAContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace DMSService
{
    public class SCADAClient : ChannelFactory<ISCADAContract>, ISCADAContract, IDisposable
    {
        SCADAProxy proxy;

        public SCADAClient()
        {
            proxy = new SCADAProxy(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:4000/SCADAService"));
        }

        public Response ExecuteCommand(Command command)
        {
            return proxy.ExecuteCommand(command);
        }
    }
}
