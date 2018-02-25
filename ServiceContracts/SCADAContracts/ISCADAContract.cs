using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using System.ServiceModel;

namespace SCADAContracts
{
    [ServiceContract]
    public interface ISCADAContract
    {
        [OperationContract]
        bool Ping();

        [OperationContract]
        Response ExecuteCommand(Command command);
    }
}
