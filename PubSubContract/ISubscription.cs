using System.ServiceModel;

namespace PubSubContract
{
    [ServiceContract(CallbackContract = typeof(IPublishing))]
    public interface ISubscription
    {
        [OperationContract]
        void Subscribe();

        [OperationContract]
        void UnSubscribe();
    }
}
