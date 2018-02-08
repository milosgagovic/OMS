using OMSSCADACommon;
using OMSSCADACommon.Commands;
using OMSSCADACommon.Responses;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADAContracts
{
    [ServiceContract]
    public interface ISCADAContract
    {
        [OperationContract]
        Response ExecuteCommand(Command command);
    }
}
