using System;
using System.ServiceModel;

namespace DMSContract
{
    // unused
    public class DMSCallProxy : ChannelFactory<IDMSCallContract>, IDMSCallContract, IDisposable
    {
        IDMSCallContract factory;

        public DMSCallProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }
        public DMSCallProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public void SendCall(string mrid)
        {
            try
            {
                factory.SendCall(mrid);
            }
            catch (Exception e )
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
