using System.ServiceModel;

namespace DMSContract
{
    [ServiceContract]
    public interface IDMSCallContract
    {
        [OperationContract]
        void SendCall(string mrid);
    }
}
