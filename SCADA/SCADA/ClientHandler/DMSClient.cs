using DMSContract;
using OMSSCADACommon;
using System;
using System.ServiceModel;

namespace SCADA.ClientHandler
{
    public class DMSClient : ChannelFactory<IDMSToSCADAContract>, IDMSToSCADAContract, IDisposable
    {
        DMSToSCADAProxy proxy;

        public DMSClient()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            proxy = new DMSToSCADAProxy(binding, new EndpointAddress("net.tcp://localhost:8039/IDMSToSCADAContract"));
        }

        public void ChangeOnSCADA(string mrID, States state)
        {
            proxy.ChangeOnSCADA(mrID, state);
            Console.WriteLine("Scada changed time {0}", DateTime.Now.ToLongTimeString());
        }
    }
}
