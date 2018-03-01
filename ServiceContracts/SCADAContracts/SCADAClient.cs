using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using SCADAContracts;
using System.ServiceModel;

namespace SCADAContracts
{
    public class SCADAClient : ClientBase<ISCADAContract>, ISCADAContract
    {
        public SCADAClient(string endpointName, NetTcpBinding binding) : base(binding, new EndpointAddress(endpointName))
        {

        }

        public SCADAClient(EndpointAddress address, NetTcpBinding binding) : base(binding, address)
        {

        }

        public Response ExecuteCommand(Command command)
        {
            return Channel.ExecuteCommand(command);
        }

        public bool Ping()
        {
            return Channel.Ping();
        }
    }
}
